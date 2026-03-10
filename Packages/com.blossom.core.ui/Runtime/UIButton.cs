using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Blossom.Core.UI {
    public class UIButton : UIImage, IPointerDownHandler, IPointerUpHandler {

        #region Const.

        private static Color DefaultActiveColor = Color.white;
        private static Color DefaultInactiveColor = new(100 / 255f, 100 / 255f, 100 / 255f, 1); 

        #endregion
        
        #region Properties
        
        public Button Button {
            get {
                Initialize();
                return _button;
            }
        }

        public UIText ContextText {
            get {
                Initialize();
                return _contextText;
            }
        }

        public bool IsInteractable {
            get {
                Initialize();
                return _button != null && _button.interactable;
            }
        }

        public bool UsePressEffect { get; set; }
        public float PressEffectDuration { get; set; }
        public float PressDownScale { get; set; }

        #endregion

        #region Fields

        private Vector3 _defaultScale;
        
        private Tween _tween;
        
        private Button _button;
        private UIText _contextText;

        private event Action _cbOnButtonDown;
        private event Action _cbOnButtonUp;

        #endregion
        
        #region Initialize
        
        protected override void OnInitialize() {
            base.OnInitialize();
            _button = this.GetComponent<Button>();
            _contextText = this.GetComponentInChildren<UIText>();
            _defaultScale = this.transform.localScale;
        }

        public override void OnSpawned() {
            base.OnSpawned();

            _tween?.Kill();
            this.transform.localScale = _defaultScale;
        }

        public override void OnRelease() {
            base.OnRelease();
            
            _tween?.Kill();
            this.transform.localScale = _defaultScale;
        }

        #endregion
        
        #region Events
        
        public UIButton SetEvent(UnityAction action) {
            Initialize();
            if (_button == null || action == null) return this;
            _button.onClick.RemoveListener(action);
            _button.onClick.AddListener(action);
            return this;
        }

        public UIButton SetDownEvent(Action action) {
            if (action == null)  return this;
            _cbOnButtonDown -= action;
            _cbOnButtonDown += action;
            return this;
        }
        
        public UIButton SetUpEvent(Action action) {
            if (action == null) return this;
            _cbOnButtonUp -= action;
            _cbOnButtonUp += action;
            return this;
        }
        
        public void ClearAllEvents() {
            Initialize();
            _button?.onClick.RemoveAllListeners();
            _cbOnButtonDown = null;
            _cbOnButtonUp = null;
        }

        #endregion

        #region Active

        public UIButton SetActive(bool active, bool setColor = true, Color? activeColor = null,
            Color? inactiveColor = null) {
            Initialize();
            if (_button != null) _button.interactable = active;

            if (setColor)
                SetColor(active ? activeColor ?? DefaultActiveColor : inactiveColor ?? DefaultInactiveColor);

            return this;
        }

        #endregion

        #region Events

        public void OnPointerDown(PointerEventData _) {
            if (!IsInteractable) return;
            PlayButtonDownEffect();
            _cbOnButtonDown?.Invoke();
        }

        public void OnPointerUp(PointerEventData _) {
            if (!IsInteractable) return;
            PlayButtonUpEffect();
            _cbOnButtonUp?.Invoke();
        }

        #endregion

        #region Effects

        private void PlayButtonDownEffect() {
            if (!UsePressEffect) return;

            _tween?.Kill();
            _tween = this.transform.DOScale(PressDownScale, Mathf.Max(1e-4f, PressEffectDuration))
                .OnKill(() => this.transform.localScale = Vector3.one * PressDownScale);
        }

        private void PlayButtonUpEffect() {
            if (!UsePressEffect) return;
            
            _tween?.Kill();
            _tween = this.transform.DOScale(_defaultScale, Mathf.Max(1e-4f, PressEffectDuration))
                .OnKill(() => this.transform.localScale = Vector3.one * PressDownScale);
        }

        #endregion
        
    }
}