namespace Blossom.Core.UI {
    using System;
    using Blossom.Core.UI.Internal;
    using DG.Tweening;
    using UnityEngine;

    public class UI_Popup : UICanvas {

        /// <summary>
        /// 동일 타입 중복 열기 허용 여부.
        /// </summary>
        public virtual bool AllowDuplicate => true;

        /// <summary>
        /// 닫힐 때 호출되고, 닫히면 초기화됨.
        /// </summary>
        public event Action OnClosed;

        private CanvasGroup _transitionCanvasGroup;
        private Tween _transitionTween;
        private bool _isTransitionPlaying;
        private bool _isClosing;

        #region Initialize

        protected override void OnInitialize() {
            base.OnInitialize();

            _rect = this.gameObject.FindChild<RectTransform>("Popup");
            _transitionCanvasGroup = this.GetComponent<CanvasGroup>();
            if (_transitionCanvasGroup == null) _transitionCanvasGroup = this.gameObject.AddComponent<CanvasGroup>();
        }

        public override void OnSpawned() {
            base.OnSpawned();
            PlayOpenTransition();
        }

        public override void OnRelease() {
            base.OnRelease();
            KillTransition();
            _isClosing = false;
            _isTransitionPlaying = false;
            SetInteractable(true);
        }
        
        #endregion

        protected virtual void PlayOpenTransition() {
            KillTransition();
            SetInteractable(false);

            Sequence sequence = UISystem.GetOpenSequence(_rect != null ? _rect : this.transform);
            if (sequence == null || sequence.Duration(false) <= 0f) {
                SetInteractable(true);
                return;
            }

            _isTransitionPlaying = true;
            _transitionTween = sequence.OnComplete(() => {
                _isTransitionPlaying = false;
                SetInteractable(true);
            });
        }

        protected virtual void PlayCloseTransition(Action onComplete) {
            if (_isClosing) return;
            _isClosing = true;
            KillTransition();
            SetInteractable(false);
            
            Sequence sequence = UISystem.GetCloseSequence(_rect != null ? _rect : this.transform);
            if (sequence == null || sequence.Duration(true) <= 0f) {
                _isClosing = false;
                onComplete?.Invoke();
                return;
            }

            _isTransitionPlaying = true;
            _transitionTween = sequence.OnComplete(() => {
                _isTransitionPlaying = false;
                _isClosing = false;
                onComplete?.Invoke();
            });
        }
        
        public virtual void Close() {
            if (_isClosing) return;
            PlayCloseTransition(() => {
                OnClosed?.Invoke();
                OnClosed = null;
                UI.ClosePopup(this);
            });
        }

        private void SetInteractable(bool interactable) {
            if (_transitionCanvasGroup == null) return;
            _transitionCanvasGroup.interactable = interactable;
            _transitionCanvasGroup.blocksRaycasts = interactable;
        }
        
        private void KillTransition() {
            _transitionTween?.Kill();
            _transitionTween = null;
        }
        
    }
}