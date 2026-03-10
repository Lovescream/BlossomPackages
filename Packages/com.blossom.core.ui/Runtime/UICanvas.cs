using UnityEngine;
using UnityEngine.UI;

namespace Blossom.Core.UI {
    public class UICanvas : UIBase {
        
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

        public int Order {
            get => _canvas?.sortingOrder ?? 0;
            set {
                Initialize();
                if (_canvas != null) _canvas.sortingOrder = value;
            }
        }
        
        private Canvas _canvas;
        private CanvasScaler _scaler;
        
        protected override void OnInitialize() {
            base.OnInitialize();
            _canvas = this.GetComponent<Canvas>();
            if (_canvas == null) _canvas = this.gameObject.AddComponent<Canvas>();
            _scaler = this.GetComponent<CanvasScaler>();
            if (_scaler == null) _scaler = this.gameObject.AddComponent<CanvasScaler>();
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
        
    }
}