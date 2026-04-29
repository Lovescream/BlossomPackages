namespace Blossom.Core.UI {
    using UnityEngine;
    using UnityEngine.UI;
    
    public class UICanvas : UIBase {

        #region Properties
        
        public Canvas Canvas {
            get {
                Initialize();
                return _canvas;
            }
        }

        public CanvasScaler Scaler {
            get {
                Initialize();
                return _scaler;
            }
        }

        public GraphicRaycaster Raycaster {
            get {
                Initialize();
                return _raycaster;
            }
        }

        public int Order {
            get => _canvas?.sortingOrder ?? 0;
            set {
                Initialize();
                if (_canvas != null) _canvas.sortingOrder = value;
            }
        }

        public RectTransform SafeArea {
            get {
                Initialize();
                return _safeArea;
            }
        }

        public float SafeTopHeight {
            get {
                Initialize();
                return _safeTopHeight;
            }
        }

        public float SafeBottomHeight {
            get {
                Initialize();
                return _safeBottomHeight;
            }
        }

        #endregion

        #region Fields
        
        private Canvas _canvas;
        private CanvasScaler _scaler;
        private GraphicRaycaster _raycaster;

        private RectTransform _safeArea;
        private float _safeTopHeight;
        private float _safeBottomHeight;

        #endregion

        #region Initialize
        
        protected override void OnInitialize() {
            base.OnInitialize();
            
            _canvas = this.GetComponent<Canvas>();
            if (_canvas == null) _canvas = this.gameObject.AddComponent<Canvas>();
            _scaler = this.GetComponent<CanvasScaler>();
            if (_scaler == null) _scaler = this.gameObject.AddComponent<CanvasScaler>();
            _raycaster = this.GetComponent<GraphicRaycaster>();
            if (_raycaster == null) _raycaster = this.gameObject.AddComponent<GraphicRaycaster>();
            
            _safeArea = this.gameObject.FindChild<RectTransform>("# SafeArea");
            
            SetCanvas();
        }

        protected virtual void SetCanvas() {
            if (_canvas != null) {
                _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                _canvas.overrideSorting = true;
            }

            if (_scaler != null) {
                _scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                _scaler.referenceResolution = UI.ReferenceResolution;
            }
        }

        #endregion

        #region SafeArea

        protected void ApplySafeArea() {
            if (_safeArea == null) return;
            
            Rect safe = Screen.safeArea;
            Vector2 anchorMin = safe.position;
            Vector2 anchorMax = safe.position + safe.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            _safeArea.anchorMin = anchorMin;
            _safeArea.anchorMax = anchorMax;
            _safeArea.offsetMin = Vector2.zero;
            _safeArea.offsetMax = Vector2.zero;

            _safeTopHeight = (1 - anchorMax.y) * Screen.height;
            _safeBottomHeight = anchorMin.y * Screen.height;
        }

        #endregion
        
    }
}