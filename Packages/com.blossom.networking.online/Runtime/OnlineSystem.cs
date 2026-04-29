namespace Blossom.Networking.Online {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine;

    internal static class OnlineSystem {

        #region Const.

        private static readonly string[] HttpUrls = { "https://www.google.com" };
        private const float HttpTimeout = 3f;
        private static readonly string[] PingHosts = { "8.8.8.8" };
        private const float PingTimeout = 3f;
        private const int BaseRetryInterval = 100;
        private const int MaxRetryInterval = 5000;
        private const float RetryIntervalMultiplier = 1.5f;

        #endregion

        #region Properties

        public static bool IsConnected {
            get => _currentConnectionState;
            internal set {
                if (_currentConnectionState == value) return;
                _currentConnectionState = value;
                OnConnectionStateChanged?.Invoke(_currentConnectionState);
                if (_currentConnectionState) OnConnectionSucceeded?.Invoke();
                else OnConnectionFailed?.Invoke();
            }
        }

        public static CancellationToken CurrentCancellationToken {
            get {
                lock (ConnectionLock) {
                    if (_connectionTrackingCts == null || _connectionTrackingCts.IsCancellationRequested)
                        _connectionTrackingCts = new();
                    return _connectionTrackingCts.Token;
                }
            }
        }

        #endregion
        
        #region Fields
        
        // State.
        private static bool _currentConnectionState;
        private static bool _isTracking;

        private static CancellationTokenSource _connectionTrackingCts;
        private static readonly object ConnectionLock = new();
        
        // Collections.
        private static readonly Dictionary<OnlineValidationType, IOnlineAccessValidator> Validators;

        // Events.
        internal static event Action OnConnectionSucceeded;
        internal static event Action OnConnectionFailed;
        internal static event Action<bool> OnConnectionStateChanged;
        internal static event Action<Exception> OnError;
        
        #endregion

        #region Constructor

        static OnlineSystem() {
            Validators = new Dictionary<OnlineValidationType, IOnlineAccessValidator> {
                [OnlineValidationType.UnityNetwork] = new UnityAccessValidator(),
                [OnlineValidationType.Http] = new HttpAccessValidator(HttpUrls, HttpTimeout),
                [OnlineValidationType.Ping] = new PingAccessValidator(PingHosts, PingTimeout),
            };
        }

        #endregion

        #region Connection

        public static void Validate(OnlineValidationType type = OnlineValidationType.Default,
            Action cbOnSucceeded = null,
            Action cbOnFailed = null) {
            _ = InternalValidateAsync(type, cbOnSucceeded, cbOnFailed, CurrentCancellationToken);    
        }

        public static async Task<bool> ValidateAsync(OnlineValidationType type = OnlineValidationType.Default) {
            return await InternalValidateAsync(type, token: CurrentCancellationToken);
        }

        public static void TryConnect(float duration, OnlineValidationType type = OnlineValidationType.Default,
            Action cbOnSucceeded = null, Action cbOnFailed = null) {
            ResetCancellationToken();
            _ = InternalTryConnectAsync(duration, type, CurrentCancellationToken)
                .ContinueWith(task => {
                    if (task.Result) {
                        OnConnectionSucceeded?.Invoke();
                        cbOnSucceeded?.Invoke();
                    }
                    else {
                        OnConnectionFailed?.Invoke();
                        cbOnFailed?.Invoke();
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public static async Task<bool> TryConnectAsync(float duration, OnlineValidationType type = OnlineValidationType.Default) {
           ResetCancellationToken();
           return await InternalTryConnectAsync(duration, type, CurrentCancellationToken);
        }

        private static async Task<bool> InternalValidateAsync(OnlineValidationType type, Action cbOnSucceeded = null,
            Action cbOnFailed = null, CancellationToken token = default) {
            try {
                if (type.HasFlag(OnlineValidationType.UnityNetwork)
                    && !await Validators[OnlineValidationType.UnityNetwork].Validate(token)) {
                    cbOnFailed?.Invoke();
                    IsConnected = false;
                    return false;
                }

                List<Task<bool>> tasks = new();
                if (type.HasFlag(OnlineValidationType.Http)) tasks.Add(Validators[OnlineValidationType.Http].Validate(token));
                if (type.HasFlag(OnlineValidationType.Ping)) tasks.Add(Validators[OnlineValidationType.Ping].Validate(token));
                if (tasks.Count == 0) {
                    cbOnSucceeded?.Invoke();
                    IsConnected = true;
                    return true;
                }
                
                bool[] results = await Task.WhenAll(tasks);
                bool isConnected = results.All(b => b);
                IsConnected = isConnected;
                if (isConnected) cbOnSucceeded?.Invoke();
                else cbOnFailed?.Invoke();
                return isConnected;
            }
            catch (OperationCanceledException) {
                cbOnFailed?.Invoke();
                IsConnected = false;
                throw;
            }
        }
        
        private static async Task<bool> InternalTryConnectAsync(float duration, OnlineValidationType type, CancellationToken token) {
            Stopwatch stopWatch = Stopwatch.StartNew();
            int delay = BaseRetryInterval;

            try {
                while (stopWatch.Elapsed.TotalSeconds < duration && !token.IsCancellationRequested) {
                    if (await ValidateAsync(type)) {
                        IsConnected = true;
                        return true;
                    }

                    delay = Mathf.Min(Mathf.RoundToInt(delay * RetryIntervalMultiplier), MaxRetryInterval);
                    await Task.Delay(delay, token);
                }
            }
            catch (OperationCanceledException) {
                IsConnected = false;
                throw;
            }
            IsConnected = false;
            return false;
        }

        #endregion

        #region Tracking

        public static CancellationTokenSource StartTrackConnection(OnlineValidationType type = OnlineValidationType.Default) {
            lock (ConnectionLock) {
                if (_isTracking) return _connectionTrackingCts;
                ResetCancellationToken();
                _isTracking = true;
                _ = InternalTrackConnection(CurrentCancellationToken, type);
                return _connectionTrackingCts;
            }
        }

        public static void StopTrackConnection() {
            lock (ConnectionLock) {
                _isTracking = false;
                _connectionTrackingCts?.Cancel();
            }
        }
        
        private static async Task InternalTrackConnection(CancellationToken token, OnlineValidationType type) {
            try {
                int delay = BaseRetryInterval;
                while (_isTracking && !token.IsCancellationRequested) {
                    bool currentState = await ValidateAsync(type);
                    IsConnected = currentState;

                    delay = currentState ? BaseRetryInterval : Mathf.Min(Mathf.RoundToInt(delay * RetryIntervalMultiplier), MaxRetryInterval);
                    await Task.Delay(delay, token);
                }
            }
            catch (OperationCanceledException) {
                IsConnected = false;
            }
            catch (Exception e) {
                OnError?.Invoke(e);
                IsConnected = false;
            }
            finally {
                _isTracking = false;
            }
        }

        #endregion
        
        #region Utilities

        public static async Task<T> Retry<T>(Func<CancellationToken, Task<T>> operation, float timeout, CancellationToken token = default) {
            Stopwatch stopWatch = Stopwatch.StartNew();
            int delay = BaseRetryInterval;

            while (stopWatch.Elapsed.TotalSeconds < timeout && !token.IsCancellationRequested) {
                try {
                    T result = await operation(token);
                    if (result != null && !result.Equals(default(T))) return result;
                }
                catch (OperationCanceledException) {
                    throw;
                }
                catch (Exception e) {
                    Nyo.Warning($"Retry operation failed: {e.Message}");
                }

                try {
                    await Task.Delay(delay, token);
                    delay = Mathf.Min((int)Mathf.Round(delay * RetryIntervalMultiplier), MaxRetryInterval);
                }
                catch (OperationCanceledException) {
                    
                }
            }

            return default;
        }

        #endregion

        private static void ResetCancellationToken() {
            lock (ConnectionLock) {
                _connectionTrackingCts?.Cancel();
                _connectionTrackingCts?.Dispose();
                _connectionTrackingCts = new();
            }
        }
        
    }

    [Flags]
    public enum OnlineValidationType {
        None = 0,
        UnityNetwork = 1 << 0,
        Http = 1 << 1,
        Ping = 1 << 2,

        Default = UnityNetwork,
        All = UnityNetwork | Http | Ping,
        WebCheck = UnityNetwork | Http,
        PingCheck = UnityNetwork | Ping
    }

}
