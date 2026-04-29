namespace Blossom.Presentation.Receive.Internal {
    using DG.Tweening;
    using UnityEngine;
    
    internal sealed class DefaultBurstArcEffect : IReceiveEffect {

        #region Const.

        private const float ScatterRadiusMin = 0f;
        private const float ScatterRadiusMax = 40f;
        private const float ScatterDuration = 0.25f;
        private const float ScatterScale = 1.15f;
        private const Ease ScatterEase = Ease.OutBack;
        private const float CurveStrength = 0.2f;
        private const float FlyDuration = 0.5f;
        private const float EndScale = 1f;
        private const Ease FlyEase = Ease.InOutQuad;

        #endregion
        
        public void Play(ReceiveEffectContext context) {
            if (context?.Object == null) return;

            Transform t = context.Object.Transform;
            t.position = context.StartPosition;
            t.localScale = Vector3.zero;

            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float radius = Random.Range(ScatterRadiusMin, ScatterRadiusMax);
            Vector3 offset = new(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
            Vector3 scatterPosition = context.StartPosition + offset;
            
            Vector3 start = scatterPosition;
            Vector3 target = context.TargetPosition;
            float distance = Vector3.Distance(start, target);

            Vector3 curveOffset = context.Direction switch {
                ReceiveArcDirection.Up => Vector3.up * distance * CurveStrength,
                ReceiveArcDirection.Down => Vector3.down * distance * CurveStrength,
                ReceiveArcDirection.Left => Vector3.left * distance * CurveStrength,
                ReceiveArcDirection.Right => Vector3.right * distance * CurveStrength,
                _ => Vector3.up * distance * CurveStrength,
            };

            Vector3 control = Vector3.Lerp(start, target, 0.5f) + curveOffset;

            DOTween.Sequence()
                .Append(t.DOScale(ScatterScale, ScatterDuration).SetEase(ScatterEase))
                .Join(t.DOMove(scatterPosition, ScatterDuration).SetEase(Ease.OutQuad))
                .Append(t.DOPath(new[] { start, control, target }, FlyDuration, PathType.CatmullRom).SetEase(FlyEase))
                .Join(t.DOScale(EndScale, FlyDuration * 0.5f).SetEase(Ease.InOutSine))
                .OnComplete(() => context.OnArrived?.Invoke());
        }
        
    }
}