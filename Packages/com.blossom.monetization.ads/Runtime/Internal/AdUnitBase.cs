namespace Blossom.Monetization.Ads.Internal {
    using System;
    
    internal abstract class AdUnitBase {
        public abstract AdType AdType { get; }
        public bool IsLoaded { get; protected set; }
        public bool IsShowing { get; protected set; }

        public event Action OnLoaded;
        public event Action OnDisplayed;
        public event Action OnClosed;
        public event Action<AdRevenueInfo> OnRevenuePaid;

        private Action<bool> _showCallback;

        public abstract void Load();
        public abstract void Show(Action<bool> callback = null);
        public virtual void Hide() { }
        public virtual void Destroy() { }

        protected void BeginShow(Action<bool> callback) {
            _showCallback = callback;
            IsShowing = true;
        }

        protected void CompleteShow(bool result) {
            _showCallback?.Invoke(result);
            _showCallback = null;
            IsShowing = false;
        }
        
        protected void CallOnLoaded() => OnLoaded?.Invoke();
        protected void CallOnDisplayed() => OnDisplayed?.Invoke();
        protected void CallOnClosed() => OnClosed?.Invoke();
        protected void CallOnRevenuePaid(AdRevenueInfo info) => OnRevenuePaid?.Invoke(info);
        
    }
}