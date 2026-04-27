namespace Blossom.Presentation.Review {
    using System;
    using Core.UI;
    using Internal;
    
    public static class Review {
        
        private const string DefaultPopupKey = "UI_Popup_Review(Default)";
        
        public static event Action OnReviewed {
            add => ReviewRequester.OnReviewed += value;
            remove => ReviewRequester.OnReviewed -= value;
        }
        
        public static event Action OnRequestComplete {
            add => ReviewRequester.OnRequestComplete += value;
            remove => ReviewRequester.OnRequestComplete -= value;
        }

        public static UI_Popup_Review Open(string key = DefaultPopupKey, Action<int> onOpenFeedback = null,
            Action<ReviewResult, int> onComplete = null) {
            if (string.IsNullOrWhiteSpace(key)) key = DefaultPopupKey;
            UI_Popup_Review popup = UI.OpenPopup<UI_Popup_Review>(key);
            if (popup == null && key != DefaultPopupKey) popup = UI.OpenPopup<UI_Popup_Review>(DefaultPopupKey);
            if (popup == null) popup = UI.OpenPopup<UI_Popup_Review>();
            if (popup == null) return null;
            return popup.Set(onOpenFeedback, onComplete);
        }
    }
}