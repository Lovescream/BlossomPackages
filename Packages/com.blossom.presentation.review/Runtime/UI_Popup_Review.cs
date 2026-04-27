namespace Blossom.Presentation.Review {
    using System;
    using UnityEngine;
    using Core;
    using Core.UI;
    using Internal;
    
    public sealed class UI_Popup_Review : UI_Popup {

        #region Properties

        public override bool AllowDuplicate => false;

        #endregion

        #region Fields

        [Header("Stars"), SerializeField] private UI_ReviewStar[] stars;
        [SerializeField] private Sprite activeStar;
        [SerializeField] private Sprite inactiveStar;
        [SerializeField] private Color activeColor;
        [SerializeField] private Color inactiveColor;
        [Header("Buttons"), SerializeField] private UI_Button btnConfirm;
        [SerializeField] private UI_Button btnClose;

        private int _rate;

        private Action<int> _onOpenFeedback;
        private Action<ReviewResult, int> _onComplete;
        
        #endregion

        #region Initialize / Set

        protected override void OnInitialize() {
            base.OnInitialize();

            if (stars == null || stars.Length == 0) stars = this.GetComponentsInChildren<UI_ReviewStar>(true);
            if (stars != null) {
                for (int i = 0; i < stars.Length; i++) {
                    UI_ReviewStar star = stars[i];
                    if (star == null) continue;
                    int rate = i + 1;
                    star.Rate = rate;
                    star.SetEvent(() => SelectRate(rate));
                }
            }
            if (btnConfirm == null) btnConfirm = this.gameObject.FindChild<UI_Button>("btnConfirm");
            btnConfirm?.SetEvent(OnButtonConfirm);
            if (btnClose == null) btnClose = this.gameObject.FindChild<UI_Button>("btnClose");
            btnClose?.SetEvent(OnButtonClose);
        }

        public UI_Popup_Review Set(Action<int> onOpenFeedback = null, Action<ReviewResult, int> onComplete = null) {
            _onOpenFeedback = onOpenFeedback;
            _onComplete = onComplete;
            return this;
        }

        public override void OnSpawned() {
            base.OnSpawned();
            SelectRate(5);
        }

        #endregion

        #region Events

        private void SelectRate(int rate) {
            _rate = Mathf.Clamp(rate, 1, 5);

            if (stars == null) return;
            foreach (UI_ReviewStar star in stars) {
                if (star == null) continue;

                if (activeStar != null && inactiveStar != null)
                    star.Select(star.Rate <= _rate, activeStar, inactiveStar);
                else
                    star.Select(star.Rate <= _rate, Color.white, Color.gray);
            }
        }

        private void OnButtonConfirm() {
            ReviewResult result = _rate >= 5 ? ReviewResult.StoreReview : ReviewResult.Feedback;
            
            switch (result) {
                case ReviewResult.Feedback: _onOpenFeedback?.Invoke(_rate); break;
                case ReviewResult.StoreReview: ReviewRequester.Request(); break;
            }
            
            _onComplete?.Invoke(result, _rate);
            Close();
        }

        private void OnButtonClose() {
            _onComplete?.Invoke(ReviewResult.Canceled, _rate);
            Close();
        }

        #endregion

    }
}