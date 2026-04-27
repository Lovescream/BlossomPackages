namespace Blossom.Presentation.Review {
    using UnityEngine;
    using Core.UI;
    using DG.Tweening;
    
    public sealed class UI_ReviewStar : UI_Button {

        public int Rate { get; set; }

        private Sequence _sequence;

        public override void OnRelease() {
            base.OnRelease();
            _sequence?.Kill();
            _sequence = null;
        }
        
        public void Select(bool selected, Color activeColor, Color inactiveColor) {
            SetColor(selected ? activeColor : inactiveColor);
            PlayAnimation();
        }

        public void Select(bool selected, Sprite activeSprite, Sprite inactiveSprite) {
            SetSprite(selected ? activeSprite : inactiveSprite, true);
            PlayAnimation();
        }

        private void PlayAnimation() {
            _sequence?.Kill();
            _sequence = DOTween.Sequence()
                .Append(this.transform.DOScale(new Vector3(1.2f, 0.8f, 1.0f), 0.12f).SetEase(Ease.OutQuad))
                .Append(this.transform.DOScale(1f, 0.2f).SetEase(Ease.OutQuad));
        }

    }
}