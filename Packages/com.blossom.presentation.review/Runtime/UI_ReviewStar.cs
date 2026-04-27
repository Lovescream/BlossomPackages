namespace Blossom.Presentation.Review {
    using UnityEngine;
    using Core.UI;
    
    public sealed class UI_ReviewStar : UI_Button {

        public int Rate { get; set; }

        public void Select(bool selected, Color activeColor, Color inactiveColor) {
            SetColor(selected ? activeColor : inactiveColor);
        }

        public void Select(bool selected, Sprite activeSprite, Sprite inactiveSprite) {
            SetSprite(selected ? activeSprite : inactiveSprite, true);
        }

    }
}