using System;
using System.Collections.Generic;
using Blossom.Core.UI.Internal;
using UnityEngine;

namespace Blossom.Core.UI {
    public static class UI {
        
        #region Properties
        
        /// <summary>
        /// UI 시스템 초기화 여부.
        /// </summary>
        public static bool IsInitialized => UISystem.IsInitialized;
        
        /// <summary>
        /// UI가 하나 이상 열려있는지 여부.
        /// </summary>
        public static bool UIOpened => UISystem.UIOpened;
        
        /// <summary>
        /// 현재 열린 패널 목록.
        /// </summary>
        public static IReadOnlyList<UIPanel> Panels => UISystem.Panels;

        /// <summary>
        /// 현재 열린 팝업 목록.
        /// </summary>
        public static IReadOnlyList<UIPopup> Popups => UISystem.Popups;

        /// <summary>
        /// UI 기준 해상도.
        /// </summary>
        public static Vector2 ReferenceResolution =>
            IsInitialized ? UISystem.Settings.referenceResolution : new(1080f, 1920f);
        
        #endregion

        #region Events
        
        /// <summary>
        /// UI 열림 상태 변경 시 이벤트.
        /// </summary>
        public static event Action<bool> OnUIOpenedStateChanged {
            add => UISystem.OnUIOpenedStateChanged += value;
            remove => UISystem.OnUIOpenedStateChanged -= value;
        }
        
        /// <summary>
        /// 패널이 열렸을 때의 이벤트.
        /// </summary>
        public static event Action<UIPanel> OnPanelOpened {
            add => UISystem.OnPanelOpened += value;
            remove => UISystem.OnPanelOpened -= value;
        }

        /// <summary>
        /// 패널이 닫혔을 때의 이벤트.
        /// </summary>
        public static event Action<UIPanel> OnPanelClosed {
            add => UISystem.OnPanelClosed += value;
            remove => UISystem.OnPanelClosed -= value;
        }

        /// <summary>
        /// 팝업이 열렸을 때의 이벤트.
        /// </summary>
        public static event Action<UIPopup> OnPopupOpened {
            add => UISystem.OnPopupOpened += value;
            remove => UISystem.OnPopupOpened -= value;
        }

        /// <summary>
        /// 팝업이 닫혔을 때의 이벤트.
        /// </summary>
        public static event Action<UIPopup> OnPopupClosed {
            add => UISystem.OnPopupClosed += value;
            remove => UISystem.OnPopupClosed -= value;
        }

        #endregion

        #region Initialize

        /// <summary>
        /// UI 시스템 초기화.
        /// </summary>
        /// <param name="prefabProvider">prefab 키를 바탕으로 prefab을 제공해주는 provider</param>
        /// <param name="spawner">실제 생성/반환을 담당하는 spawner</param>
        /// <param name="settings">UI 전역 설정</param>
        public static void Initialize(IPrefabProvider prefabProvider, IUISpawner spawner, UISettings settings = null) =>
            UISystem.Initialize(prefabProvider, spawner, settings);
        
        /// <summary>
        /// UI 시스템을 정리하고 내부 상태 초기화.
        /// </summary>
        public static void Clear() => UISystem.Clear();
        
        /// <summary>
        /// UI 시스템을 완전히 초기화. 
        /// </summary>
        public static void Reset() => UISystem.Reset();

        #endregion

        #region Generals

        /// <summary>
        /// 지장한 타입의 일반 UI 생성.
        /// </summary>
        /// <typeparam name="T">생성할 UI 타입</typeparam>
        /// <param name="parent">생성 후 설정할 부모</param>
        /// <param name="usePooling">풀링 사용 여부</param>
        /// <returns>생성된 UI. 실패하면 null.</returns>
        public static T Instantiate<T>(Transform parent = null, bool usePooling = true) where T : UIBase =>
            UISystem.Instantiate<T>(parent, usePooling);

        /// <summary>
        /// 지정한 UI 인스턴스 반환 또는 제거
        /// </summary>
        /// <param name="ui">반환 또는 제거할 UI 인스턴스</param>
        public static void Destroy(UIBase ui) => UISystem.Destroy(ui);
        
        #endregion
        
        #region Scene

        /// <summary>
        /// 지정한 타입의 씬 UI 열기.
        /// </summary>
        /// <typeparam name="T">열 씬 UI 타입</typeparam>
        /// <returns>생성된 씬 UI. 실패하면 null.</returns>
        public static T OpenSceneUI<T>() where T : UIScene => UISystem.OpenSceneUI<T>();

        #endregion

        #region Panel

        /// <summary>
        /// 지정한 타입의 패널 찾기.
        /// </summary>
        /// <typeparam name="T">찾을 패널 타입</typeparam>
        /// <returns>찾은 패널 인스턴스. 실패하면 null.</returns>
        public static T GetPanel<T>() where T : UIPanel => UISystem.GetPanel<T>();
        
        /// <summary>
        /// 지정한 타입의 패널 열기. 중복이 허용되지 않은 패널이 이미 열려있다면, 맨 앞으로 이동.
        /// </summary>
        /// <typeparam name="T">열 패널 타입</typeparam>
        /// <returns>열린 패널 인스턴스. 실패하면 null.</returns>
        public static T OpenPanel<T>() where T : UIPanel => UISystem.OpenPanel<T>();

        /// <summary>
        /// 지정한 패널 닫기.
        /// </summary>
        /// <param name="panel">닫을 패널</param>
        public static void ClosePanel(UIPanel panel) => UISystem.ClosePanel(panel);

        /// <summary>
        /// 지정한 타입의 패널 모두 닫기.
        /// </summary>
        /// <typeparam name="T">닫을 패널 타입</typeparam>
        public static void ClosePanels<T>() where T : UIPanel => UISystem.ClosePanels<T>();

        /// <summary>
        /// 모든 패널 닫기.
        /// </summary>
        public static void CloseAllPanels() => UISystem.CloseAllPanels();

        /// <summary>
        /// 가장 마지막에 열린 패널 찾기.
        /// </summary>
        /// <returns>가장 위에 있는 패널. 없으면 null.</returns>
        public static UIPanel GetLatestPanel() => UISystem.GetLatestPanel();

        #endregion

        #region Popup

        /// <summary>
        /// 지정한 타입의 팝업 찾기.
        /// </summary>
        /// <typeparam name="T">찾을 팝업 타입</typeparam>
        /// <returns>찾은 팝업 인스턴스. 실패하면 null.</returns>
        public static T GetPopup<T>() where T : UIPopup => UISystem.GetPopup<T>();
        
        /// <summary>
        /// 지정한 타입의 팝업 열기. 중복이 허용되지 않은 팝업이 이미 열려있다면, 맨 앞으로 이동.
        /// </summary>
        /// <typeparam name="T">열 팝업 타입</typeparam>
        /// <returns>열린 팝업 인스턴스. 실패하면 null.</returns>
        public static T OpenPopup<T>() where T : UIPopup => UISystem.OpenPopup<T>();

        /// <summary>
        /// 지정한 팝업 닫기.
        /// </summary>
        /// <param name="popup">닫을 팝업</param>
        public static void ClosePopup(UIPopup popup) => UISystem.ClosePopup(popup);

        /// <summary>
        /// 지정한 타입의 팝업 모두 닫기.
        /// </summary>
        /// <typeparam name="T">닫을 팝업 타입</typeparam>
        public static void ClosePopups<T>() where T : UIPopup => UISystem.ClosePopups<T>();

        /// <summary>
        /// 모든 팝업 닫기.
        /// </summary>
        public static void CloseAllPopups() => UISystem.CloseAllPopups();

        /// <summary>
        /// 가장 마지막에 열린 팝업 찾기.
        /// </summary>
        /// <returns>가장 위에 있는 팝업. 없으면 null.</returns>
        public static UIPopup GetLatestPopup() => UISystem.GetLatestPopup();

        #endregion
        
    }
}