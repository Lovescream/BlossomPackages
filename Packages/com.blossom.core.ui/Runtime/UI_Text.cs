using TMPro;
using UnityEngine;

namespace Blossom.Core.UI {
    public class UI_Text : UIBase {

        public TextMeshProUGUI TMP {
            get {
                Initialize();
                return _text;
            }
        }

        public string Text {
            get => TMP?.text;
            set {
                Initialize();
                if (_text != null) _text.text = value;
            }
        }

        public Color Color {
            get => TMP?.color ?? Color.white;
            set {
                Initialize();
                if (_text != null) _text.color = value;
            }
        }

        public float Size {
            get => TMP?.fontSize ?? 0;
            set {
                Initialize();
                if (_text == null) return;
                if (value < 0) _text.enableAutoSizing = true;
                else _text.fontSize = value;
            }
        }

        public TextAlignmentOptions Alignment {
            get => TMP?.alignment ?? TextAlignmentOptions.Center;
            set {
                Initialize();
                if (_text != null) _text.alignment = value;
            }
        }

        public Material SharedMaterial {
            get => TMP?.fontSharedMaterial;
            set {
                Initialize();
                if (_text != null) _text.fontSharedMaterial = value;
            }
        }
        
        private TextMeshProUGUI _text;

        protected override void OnInitialize() {
            base.OnInitialize();
            _text = this.GetComponent<TextMeshProUGUI>();
        }

        public UI_Text SetText(string text) {
            Text = text;
            return this;
        }

        public UI_Text SetColor(Color color) {
            Color = color;
            return this;
        }

        public UI_Text SetAlignment(TextAlignmentOptions alignment) {
            Alignment = alignment;
            return this;
        }

        public UI_Text SetMaterial(Material material) {
            SharedMaterial = material;
            return this;
        }

    }
}