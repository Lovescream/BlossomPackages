using UnityEngine;
using UnityEngine.UI;

namespace Blossom.Core.UI {
    public class UIImage : UIBase{

        public Image Image {
            get {
                Initialize();
                return _image;
            }
        }

        public Sprite Sprite {
            get => Image?.sprite;
            set {
                Initialize();
                if (_image != null) _image.sprite = value;
            }
        }

        public Color Color {
            get => Image?.color ?? Color.white;
            set {
                Initialize();
                if (_image != null) _image.color = value;
            }
        }

        public float Fill {
            get => Image?.fillAmount ?? 0f;
            set {
                Initialize();
                if (_image != null) _image.fillAmount = value;
            }
        }

        private Image _image;

        protected override void OnInitialize() {
            base.OnInitialize();
            _image = this.GetComponent<Image>();
        }

        public UIImage SetSprite(Sprite sprite, bool resetColor = false) {
            Sprite = sprite;
            if (resetColor) Color = sprite == null ? Color.clear : Color.white;
            return this;
        }

        public UIImage SetColor(Color color) {
            Color = color;
            return this;
        }

        public UIImage SetAlpha(float alpha) {
            Initialize();
            if (_image == null) return this;
            
            Color color = _image.color;
            color.a = alpha;
            _image.color = color;
            
            return this;
        }

        public UIImage SetFill(float amount) {
            Fill = amount;
            return this;
        }
        
    }
}