namespace Blossom.Core.UI {
    using System;
    using UnityEngine;
    using UnityEngine.EventSystems;
    
    public class UI_Toggle : UI_Image, IPointerClickHandler {

        public bool IsOn {
            get => _isOn;
            set {
                _isOn = value;
                _cbOnToggle?.Invoke(value);
                Sprite = IsOn ? toggleOnSprite : toggleOffSprite;
            }
        }
        
        [Header("Sprites")]
        [SerializeField] private Sprite toggleOnSprite;
        [SerializeField] private Sprite toggleOffSprite;

        private bool _isOn;

        private event Action<bool> _cbOnToggle;

        public UI_Toggle SetEvent(Action<bool> callback) {
            _cbOnToggle -= callback;
            _cbOnToggle += callback;
            return this;
        }

        public void SetToggleStateWithoutNotify(bool value) {
            _isOn = value;
            Sprite = IsOn ? toggleOnSprite : toggleOffSprite;
        }

        public void OnPointerClick(PointerEventData _) {
            IsOn = !IsOn;
        }
    }
}