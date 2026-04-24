using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

namespace Blossom.Core.UI.Internal {
    internal static class UISystem {

        #region Properties

        internal static bool IsInitialized { get; private set; }
        
        internal static bool UIOpened => _panels.Count > 0 || _popups.Count > 0;
        
        internal static IReadOnlyList<UI_Panel> Panels => _panels;
        internal static IReadOnlyList<UI_Popup> Popups => _popups;

        internal static UISettings Settings { get; private set; }

        internal static Transform Root {
            get {
                if (_root == null) {
                    GameObject obj = GameObject.Find(Settings.rootName);
                    if (obj == null) obj = new GameObject(Settings.rootName);
                    _root = obj.transform;
                }

                return _root;
            }
        }

        #endregion
        
        #region Fields

        private static IPrefabProvider _prefabProvider;
        private static IUISpawner _spawner;

        private static readonly List<UI_Panel> _panels = new();
        private static readonly List<UI_Popup> _popups = new();
        
        private static Transform _root;
        private static int _panelOrder;
        private static int _popupOrder;

        internal static event Action<bool> OnUIOpenedStateChanged;
        internal static event Action<UI_Panel> OnPanelOpened;
        internal static event Action<UI_Panel> OnPanelClosed;
        internal static event Action<UI_Popup> OnPopupOpened;
        internal static event Action<UI_Popup> OnPopupClosed;

        #endregion

        #region Initialize

        internal static void Initialize(IPrefabProvider prefabProvider, IUISpawner spawner, UISettings settings = null) {
            if (IsInitialized) return;
            _prefabProvider = prefabProvider ?? throw new ArgumentNullException(nameof(prefabProvider));
            _spawner = spawner ?? throw new ArgumentNullException(nameof(spawner));
            Settings = settings ?? new UISettings();
            _panelOrder = Settings.initialPanelOrder;
            _popupOrder = Settings.initialPopupOrder;

            IsInitialized = true;
        }

        internal static void Clear() {
            if (!IsInitialized) return;

            CloseAllPanels();
            CloseAllPopups();
            
            _panelOrder = Settings.initialPanelOrder;
            _popupOrder = Settings.initialPopupOrder;
        }

        internal static void Reset() {
            Clear();
            _prefabProvider = null;
            _spawner = null;
            _root = null;
            Settings = null;
            IsInitialized = false;

            OnUIOpenedStateChanged = null;
            OnPanelOpened = null;
            OnPanelClosed = null;
            OnPopupOpened = null;
            OnPopupClosed = null;
        }

        #endregion

        #region Generals

        internal static T Instantiate<T>(Transform parent = null, bool usePooling = true) where T : UIBase {
            CheckInitialized();

            string key = typeof(T).Name;
            GameObject prefab = _prefabProvider.Provide(key);
            if (prefab == null) {
                Debug.LogError($"[Blossom:UI] Instantiate<{key}>(): prefab not found.");
                return null;
            }

            Transform spawnParent = parent ?? Root;
            GameObject obj = _spawner.Spawn(prefab, spawnParent, usePooling);
            if (obj == null) {
                Debug.LogError($"[Blossom:UI] Instantiate<{key}>(): Instantiate failed");
                return null;
            }

            if (!obj.TryGetComponent(out T ui) || ui == null) {
                Debug.LogError($"[Blossom:UI] Instantiate<{key}>(): Component not found on instance.");
                _spawner.Despawn(obj, key);
                return null;
            }
            obj.transform.localScale = Vector3.one;
            
            ui.Initialize();
            ui.SetPoolKey(key);
            ui.OnSpawned();
            
            return ui;
        }

        internal static void Destroy(UIBase ui) {
            if (ui == null || ui.gameObject == null) return;
            ui.OnRelease();
            _spawner.Despawn(ui.gameObject, ui.PoolKey);
        }

        #endregion

        #region Validation

        private static void CheckInitialized() {
            if (IsInitialized) return;
            throw new InvalidOperationException("[Blossom:UI] UISystem has not yet initialized.");
        }

        #endregion

        #region Scene

        internal static T OpenSceneUI<T>() where T : UI_Scene {
            return Instantiate<T>(null, false);
        }

        #endregion

        #region Panel
        
        internal static T GetPanel<T>() where T : UI_Panel {
            foreach (UI_Panel panel in _panels) if (panel is T p) return p;
            return null;
        }

        internal static T OpenPanel<T>() where T : UI_Panel {
            CheckInitialized();

            foreach (UI_Panel openedPanel in _panels) {
                if (openedPanel is not T panel) continue;
                if (panel.AllowDuplicate) break;
                SetPanelToFront(panel);
                return panel;
            }

            T newPanel = Instantiate<T>();
            if (newPanel == null) return null;

            _panels.Add(newPanel);
            newPanel.Order = _panelOrder++;

            OnPanelOpened?.Invoke(newPanel);
            OnUIOpenedStateChanged?.Invoke(UIOpened);

            return newPanel;
        }

        internal static void ClosePanel(UI_Panel panel) {
            if (panel == null) return;
            bool wasOpened = UIOpened;
            if (!_panels.Remove(panel)) return;

            Destroy(panel);

            if (_panels.Count == 0) _panelOrder = Settings.initialPanelOrder;
            else ReorderAllPanels();

            OnPanelClosed?.Invoke(panel);
            if (wasOpened != UIOpened) OnUIOpenedStateChanged?.Invoke(UIOpened);
        }

        internal static void ClosePanels<T>() where T : UI_Panel {
            _panels.Where(p => p is T).ToList().ForEach(ClosePanel);
        }

        internal static void CloseAllPanels() {
            if (_panels.Count == 0) return;

            bool wasOpened = UIOpened;
            for (int i = _panels.Count - 1; i >= 0; i--) {
                UI_Panel panel = _panels[i];
                if (panel == null) continue;
                Destroy(panel);
                OnPanelClosed?.Invoke(panel);
            }

            _panels.Clear();
            _panelOrder = Settings.initialPanelOrder;

            if (wasOpened != UIOpened) OnUIOpenedStateChanged?.Invoke(UIOpened);
        }

        internal static UI_Panel GetLatestPanel() {
            return _panels.Count == 0 ? null : _panels[^1];
        }

        internal static void ReorderAllPanels() {
            _panelOrder = Settings.initialPanelOrder;
            _panels.ForEach(p => p.Order = _panelOrder++);
        }

        internal static void SetPanelToFront(UI_Panel panel) {
            if (!_panels.Remove(panel)) return;
            _panels.Add(panel);
            ReorderAllPanels();
        }
        
        #endregion

        #region Popup
        
        internal static T GetPopup<T>() where T : UI_Popup {
            foreach (UI_Popup popup in _popups) if (popup is T p) return p;
            return null;
        }

        internal static T OpenPopup<T>() where T : UI_Popup {
            CheckInitialized();

            foreach (UI_Popup openedPopup in _popups) {
                if (openedPopup is not T popup) continue;
                if (popup.AllowDuplicate) break;
                SetPopupToFront(popup);
                return popup;
            }

            T newPopup = Instantiate<T>();
            if (newPopup == null) return null;

            _popups.Add(newPopup);
            newPopup.Order = _popupOrder++;

            OnPopupOpened?.Invoke(newPopup);
            OnUIOpenedStateChanged?.Invoke(UIOpened);

            return newPopup;
        }

        internal static void ClosePopup(UI_Popup popup) {
            if (popup == null) return;
            bool wasOpened = UIOpened;
            if (!_popups.Remove(popup)) return;

            Destroy(popup);

            if (_popups.Count == 0) _popupOrder = Settings.initialPopupOrder;
            else ReorderAllPopups();

            OnPopupClosed?.Invoke(popup);
            if (wasOpened != UIOpened) OnUIOpenedStateChanged?.Invoke(UIOpened);
        }

        internal static void ClosePopups<T>() where T : UI_Popup {
            _popups.Where(p => p is T).ToList().ForEach(ClosePopup);
        }

        internal static void CloseAllPopups() {
            if (_popups.Count == 0) return;

            bool wasOpened = UIOpened;
            for (int i = _popups.Count - 1; i >= 0; i--) {
                UI_Popup popup = _popups[i];
                if (popup == null) continue;
                Destroy(popup);
                OnPopupClosed?.Invoke(popup);
            }

            _popups.Clear();
            _popupOrder = Settings.initialPopupOrder;

            if (wasOpened != UIOpened) OnUIOpenedStateChanged?.Invoke(UIOpened);
        }

        internal static UI_Popup GetLatestPopup() {
            return _popups.Count == 0 ? null : _popups[^1];
        }

        internal static void ReorderAllPopups() {
            _popupOrder = Settings.initialPopupOrder;
            _popups.ForEach(p => p.Order = _popupOrder++);
        }

        internal static void SetPopupToFront(UI_Popup popup) {
            if (!_popups.Remove(popup)) return;
            _popups.Add(popup);
            ReorderAllPopups();
        }

        #endregion

        #region Utility

        internal static List<UIMotion> Collect(Transform root) {
            List<UIMotion> result = new();
            if (root == null) return result;
            
            UIMotion[] motions = root.GetComponentsInChildren<UIMotion>(true);
            if (motions == null || motions.Length == 0) return result;

            foreach (UIMotion motion in motions) {
                if (motion == null) continue;
                result.Add(motion);
            }

            result.Sort(CompareByHierarchy);
            return result;
        }

        internal static Sequence GetOpenSequence(Transform root) {
            List<UIMotion> motions = Collect(root);
            Sequence sequence = DOTween.Sequence();
            sequence.SetUpdate(true);

            foreach (UIMotion motion in motions) {
                sequence.Append(motion.PlayOpen());
            }
            
            return sequence;
        }

        internal static Sequence GetCloseSequence(Transform root) {
            List<UIMotion> motions = Collect(root);
            Sequence sequence = DOTween.Sequence();
            sequence.SetUpdate(true);

            foreach (UIMotion motion in motions) {
                sequence.Append(motion.PlayClose());
            }
            
            return sequence;
        }

        private static int CompareByHierarchy(UIMotion left, UIMotion right) {
            if (ReferenceEquals(left, right)) return 0;
            if (left == null) return 1;
            if (right == null) return -1;

            List<int> leftPath = GetSiblingPath(left.transform);
            List<int> rightPath = GetSiblingPath(right.transform);

            int count = Mathf.Min(leftPath.Count, rightPath.Count);
            for (int i = 0; i < count; i++) {
                int compare = leftPath[i].CompareTo(rightPath[i]);
                if (compare != 0) return compare;
            }

            return leftPath.Count.CompareTo(rightPath.Count);
        }
        
        private static List<int> GetSiblingPath(Transform target) {
            List<int> path = new List<int>();
            Transform current = target;

            while (current != null) {
                path.Add(current.GetSiblingIndex());
                current = current.parent;
            }

            path.Reverse();
            return path;
        }
        
        #endregion

    }
}