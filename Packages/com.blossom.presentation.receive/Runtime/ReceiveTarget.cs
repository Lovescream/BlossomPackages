namespace Blossom.Presentation.Receive {
    using Internal;
    using DG.Tweening;
    using UnityEngine;
    
    public sealed class ReceiveTarget : MonoBehaviour, IReceiveTarget {

        #region Properties

        public ReceiveKey Key => new(group, key);
        public ReceiveSpace Space => space;
        public Vector3 Position => this.transform.position;

        #endregion

        #region Fields

        [Header("Identity")]
        [SerializeField] private string group;
        [SerializeField] private string key;

        [Header("Space")]
        [SerializeField] private ReceiveSpace space = ReceiveSpace.Screen;

        [Header("Animation")]
        [SerializeField] private bool useScaleAnimation = true;
        [SerializeField] private float scaleAnimationDuration = 0.2f;
        [SerializeField] private float scaleAnimationScale = 1.1f;
        
        private Vector3 _originScale;
        private Tween _tween;

        #endregion

        #region MonoBehaviours

        void OnEnable() {
            _originScale = this.transform.localScale;
            ReceiveTargetRegistry.Register(this);
        }

        void OnDisable() {
            ReceiveTargetRegistry.Unregister(this);
            _tween?.Kill();
            _tween = null;
            this.transform.localScale = _originScale;
        }

        #endregion

        public void NotifyReceived(ReceiveKey receiveKey, int amount) {
            if (!useScaleAnimation) return;

            _tween?.Kill();
            _tween = DOTween.Sequence()
                .Append(this.transform.DOScale(_originScale * scaleAnimationScale, scaleAnimationDuration * 0.5f)
                    .SetEase(Ease.OutQuad))
                .Append(this.transform.DOScale(_originScale, scaleAnimationDuration * 0.5f).SetEase(Ease.OutQuad));
        }

    }
}