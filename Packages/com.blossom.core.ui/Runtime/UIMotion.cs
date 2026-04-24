namespace Blossom.Core.UI {
    using DG.Tweening;
    using UnityEngine;

    [DisallowMultipleComponent]
    public sealed class UIMotion : MonoBehaviour {

        #region Fields
        
        [Header("Common")]
        [SerializeField] private UIMotionType openType = UIMotionType.FadeScale;
        [SerializeField] private UIMotionType closeType = UIMotionType.FadeScale;
        [SerializeField] private float duration = 0.25f;
        [SerializeField] private float delay = 0f;
        [SerializeField] private Ease ease = Ease.OutCubic;

        [Header("Scale")]
        [SerializeField] private float startScale = 0.85f;
        [SerializeField] private float closeScale = 0.9f;

        [Header("Slide")]
        [SerializeField] private Vector2 slideOffset = new Vector2(0f, 80f);

        private bool _isInitialized;
        
        private Vector3 _originScale;
        private Vector2 _originAnchoredPosition;
        
        private RectTransform _rect;
        private CanvasGroup _canvasGroup;

        #endregion

        #region Initialize
        
        private void Initialize() {
            if (_isInitialized) return;

            _rect = this.GetComponent<RectTransform>();
            if (this.TryGetComponent(out _canvasGroup)) _canvasGroup = this.gameObject.AddComponent<CanvasGroup>();

            _originScale = this.transform.localScale;
            _originAnchoredPosition = _rect.anchoredPosition;
            
            duration = Mathf.Max(1e-4f, duration);
            
            _isInitialized = true;
        }

        #endregion

        #region Play

        public Sequence PlayOpen() {
            Initialize();
            if (openType == UIMotionType.None) return null;
            return Play(openType, true);
        }

        public Sequence PlayClose() {
            Initialize();
            if (closeType == UIMotionType.None) return null;
            return Play(closeType, false);
        }

        private Sequence Play(UIMotionType type, bool isOpen) {
            Sequence sequence = DOTween.Sequence();
            sequence.SetUpdate(true);
            Reset();

            switch (type) {
                case UIMotionType.Fade: JoinFade(sequence, isOpen); break;
                case UIMotionType.Scale: JoinScale(sequence, isOpen); break;
                case UIMotionType.SlideFromBottom:
                    JoinSlide(sequence, isOpen, Vector2.down * Mathf.Abs(slideOffset.y));
                    break;
                case UIMotionType.SlideFromTop:
                    JoinSlide(sequence, isOpen, Vector2.up * Mathf.Abs(slideOffset.y));
                    break;
                case UIMotionType.SlideFromLeft:
                    JoinSlide(sequence, isOpen, Vector2.left * Mathf.Abs(slideOffset.x == 0f ? 80f : slideOffset.x));
                    break;
                case UIMotionType.SlideFromRight:
                    JoinSlide(sequence, isOpen, Vector2.right * Mathf.Abs(slideOffset.x == 0f ? 80f : slideOffset.x));
                    break;
                case UIMotionType.FadeScale:
                    JoinFade(sequence, isOpen);
                    JoinScale(sequence, isOpen);
                    break;
            }

            if (delay > 0f) sequence.PrependInterval(delay);

            sequence.OnComplete(Reset);
            sequence.OnKill(Reset);

            return sequence;
        }

        #endregion

        #region Sequences

        private void JoinFade(Sequence sequence, bool isOpen) {
            if (_canvasGroup == null) return;

            _canvasGroup.alpha = isOpen ? 0f : 1f;
            sequence.Join(DOTween.To(
                () => _canvasGroup.alpha,
                x => _canvasGroup.alpha = x,
                isOpen ? 1f : 0f,
                duration
            ).SetEase(ease));
        }

        private void JoinScale(Sequence sequence, bool isOpen) {
            this.transform.localScale = _originScale * (isOpen ? startScale : 1f);
            sequence.Join(this.transform.DOScale(_originScale * (isOpen ? 1f : closeScale), duration).SetEase(ease));
        }

        private void JoinSlide(Sequence sequence, bool isOpen, Vector2 fromOffset) {
            _rect.anchoredPosition = _originAnchoredPosition + (isOpen ? fromOffset : Vector2.zero);
            sequence.Join(DOTween.To(
                () => _rect.anchoredPosition,
                x => _rect.anchoredPosition = x,
                _originAnchoredPosition + (isOpen ? Vector2.zero : fromOffset),
                duration
            ).SetEase(ease));
        }

        #endregion

        #region Reset

        public void Reset() {
            Initialize();
            this.transform.localScale = _originScale;
            _rect.anchoredPosition = _originAnchoredPosition;
            if (_canvasGroup != null) _canvasGroup.alpha = 1f;
        }

        #endregion
        
    }

    public enum UIMotionType {
        None = 0,
        Fade = 1,
        Scale = 2,
        SlideFromBottom = 3,
        SlideFromTop = 4,
        SlideFromLeft = 5,
        SlideFromRight = 6,
        FadeScale = 7,
    }
}