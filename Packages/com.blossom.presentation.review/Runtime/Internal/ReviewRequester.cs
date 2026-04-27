namespace Blossom.Presentation.Review.Internal {
    using System;
    using UnityEngine;
#if UNITY_IOS
    using UnityEngine.iOS;
#endif
#if UNITY_ANDROID
    using System.Collections;
    using Google.Play.Review;
    using Blossom.Common;
#endif

    internal static class ReviewRequester {

        #region Fields

        internal static event Action OnReviewed;
        internal static event Action OnRequestComplete;

        #endregion

        #region Request

        internal static void Request() {
#if UNITY_EDITOR
            Debug.Log("[Blossom:Review] Store review is not supported in this environment.");
            OnRequestComplete?.Invoke();
#elif UNITY_IOS
            if (Device.RequestStoreReview()) OnReviewed?.Invoke();
            OnRequestComplete?.Invoke();
#elif UNITY_ANDROID
            CoroutineRunner.Run(CoRequestAndroid());
#else
            Debug.Log("[Blossom:Review] Store review is not supported on this platform.");
            OnRequestComplete?.Invoke();
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private static IEnumerator CoRequestAndroid() {
            ReviewManager reviewManager = new();

            var requestOperation = reviewManager.RequestReviewFlow();
            yield return requestOperation;
            if (requestOperation.Error != ReviewErrorCode.NoError) {
                Debug.LogError($"[Blossom:Review] Android review request error: {requestOperation.Error}");
                Application.OpenURL($"market://details?id={Application.identifier}");
                OnRequestComplete?.Invoke();
                yield break;
            }

            PlayReviewInfo reviewInfo = requestOperation.GetResult();

            var launchOperation = reviewManager.LaunchReviewFlow(reviewInfo);
            yield return launchOperation;

            if (launchOperation.Error != ReviewErrorCode.NoError) {
                Debug.LogError($"[Blossom:Review] Android review launch error: {launchOperation.Error}");
                OnRequestComplete?.Invoke();
                yield break;
            }
            
            OnReviewed?.Invoke();
            OnRequestComplete?.Invoke();
        }
#endif

        #endregion
        
    }
}