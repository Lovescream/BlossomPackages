using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Blossom.Presentation.Receive.Internal {
    internal static class ReceiveSystem {

        #region Properties

        internal static Transform ReceiveUIParent => _canvas?.transform ?? null;
        
        internal static bool IsInitialized { get; private set; }

        #endregion
        
        #region Fields

        private static Canvas _canvas;

        private static IReceiveRewardHandler _rewardHandler;
        private static IReceiveTargetResolver _targetResolver;
        private static IReceiveSpawner _spawner;
        private static IReceiveIconProvider _iconProvider;
        private static IReceiveAmountSplitter _splitter;
        private static IReceiveAmountTextFormatter _formatter;
        private static IReceiveEffectSelector _effectSelector;
        private static ReceiveSettings _settings;
        private static ReceiveRunner _runner;

        #endregion

        #region Initialize

        internal static void Initialize(IReceiveRewardHandler rewardHandler, IReceiveTargetResolver targetResolver,
            IReceiveSpawner spawner, IReceiveIconProvider iconProvider, IReceiveAmountSplitter amountSplitter,
            IReceiveAmountTextFormatter formatter, IReceiveEffectSelector effectSelector, ReceiveSettings settings) {
            if (IsInitialized) return;
            _rewardHandler = rewardHandler;
            _targetResolver = targetResolver ?? new DefaultReceiveTargetResolver();
            _spawner = spawner;
            _iconProvider = iconProvider;
            _settings = settings ?? new ReceiveSettings();
            _splitter = amountSplitter ?? new DefaultReceiveAmountSplitter();
            _formatter = formatter ?? new DefaultReceiveAmountTextFormatter(_settings);
            _effectSelector = effectSelector ?? new DefaultReceiveEffectSelector(new DefaultBurstArcEffect());

            GameObject obj = new GameObject("[ReceiveRunner]");
            Object.DontDestroyOnLoad(obj);
            _runner = obj.AddComponent<ReceiveRunner>();

            InitializeCanvas();
            
            IsInitialized = true;
        }

        private static void InitializeCanvas() {
            CanvasScaler scaler;
            
            GameObject obj = GameObject.Find("[ReceiveCanvas]");
            if (obj == null) {
                obj = new("[ReceiveCanvas]");
                Object.DontDestroyOnLoad(obj);
                _canvas = obj.AddComponent<Canvas>();
                scaler = obj.AddComponent<CanvasScaler>();
                obj.AddComponent<GraphicRaycaster>();
            }
            else {
                _canvas = obj.GetComponent<Canvas>();
                scaler = obj.GetComponent<CanvasScaler>();
            }
            
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.overrideSorting = true;
            _canvas.sortingOrder = 6262;

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = _settings.ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        internal static void Reset() {
            if (_runner != null) {
                Object.Destroy(_runner.gameObject);
            }

            _rewardHandler = null;
            _targetResolver = null;
            _spawner = null;
            _iconProvider = null;
            _splitter = null;
            _formatter = null;
            _effectSelector = null;
            _settings = null;
            _runner = null;

            if (_canvas != null) {
                Object.Destroy(_canvas.gameObject);
                _canvas = null;
            }
            
            IsInitialized = false;
        }

        #endregion

        internal static void Apply(ReceiveKey key, int amount) {
            if (!IsInitialized) {
                Debug.LogError($"[Blossom:Receive] Apply({key}, {amount}): System is not initialized.");
                return;
            }

            if (!key.IsValid) return;
            if (amount <= 0) return;

            _rewardHandler?.Apply(key, amount);
        }

        internal static void Play(ReceiveRequest request) {
            if (!IsInitialized) {
                Debug.LogError($"[Blossom:Receive] Play({request.Key}): System is not initialized.");
                return;
            }

            if (!request.Key.IsValid) return;
            if (request.Amount <= 0) {
                request.OnCompleted?.Invoke();
                return;
            }
            
            _runner.StartCoroutine(CoPlay(request));
        }

        internal static void ApplyAndPlay(ReceiveRequest request) {
            if (!IsInitialized) {
                Debug.LogError($"[Blossom:Receive] ApplyAndPlay({request.Key}): System is not initialized.");
                return;
            }
            
            if (!request.Key.IsValid) return;
            if (request.Amount <= 0) {
                request.OnCompleted?.Invoke();
                return;
            }
            
            _rewardHandler?.Apply(request.Key, request.Amount);
            _runner.StartCoroutine(CoPlay(request));
        }

        private static IEnumerator CoPlay(ReceiveRequest request) {
            if (_targetResolver == null || !_targetResolver.TryResolve(request.Key, out IReceiveTarget target) || target == null) {
                request.OnCompleted?.Invoke();
                yield break;
            }

            if (_spawner == null) {
                request.OnCompleted?.Invoke();
                yield break;
            }

            ReceiveSpace space = target.Space;
            if (!_spawner.Supports(space)) {
                request.OnCompleted?.Invoke();
                yield break;
            }

            List<int> chunks = _splitter.Split(request.Key, request.Amount);
            if (chunks == null || chunks.Count <= 0) {
                request.OnCompleted?.Invoke();
                yield break;
            }

            int arrivedCount = 0;
            int pendingCount = 0;

            foreach (int chunkAmount in chunks) {
                IReceiveEffect effect = _effectSelector?.Select(request);
                if (effect == null) continue;

                IReceiveObject obj = _spawner.Spawn(new(request.Key, space, chunkAmount));
                if (obj == null) continue;
                
                pendingCount++;

                obj.SetIcon(_iconProvider?.GetIcon(request.Key));
                obj.SetAmountText(_formatter?.Format(request.Key, chunkAmount) ?? string.Empty);
                
                Vector3 startPosition = ReceiveSpaceUtility.Convert(request.StartPosition, request.Space, space,
                    _settings.WorldCamera, _settings.DefaultWorldDepth,
                    target.Space == ReceiveSpace.World ? target.Position : null);
                Vector3 targetPosition = ReceiveSpaceUtility.Convert(target.Position, target.Space, space,
                    _settings.WorldCamera, _settings.DefaultWorldDepth,
                    target.Space == ReceiveSpace.World ? target.Position : null);
                obj.SetPosition(startPosition);

                int amount = chunkAmount;
                ReceiveEffectContext context = new ReceiveEffectContext {
                    Object = obj,
                    StartPosition = startPosition,
                    TargetPosition = targetPosition,
                    Direction = request.ArcDirection,
                    Amount = chunkAmount,
                    OnArrived = () => {
                        _rewardHandler?.ApplyDisplay(request.Key, amount);
                        target?.NotifyReceived(request.Key, amount);
                        arrivedCount++;
                        obj.Release();
                    }
                };

                effect.Play(context);

                yield return new WaitForSeconds(Random.Range(0.02f, 0.08f));
            }

            if (pendingCount <= 0) {
                request.OnCompleted?.Invoke();
                yield break;
            }

            yield return new WaitUntil(() => arrivedCount >= pendingCount);

            _rewardHandler?.SyncDisplay(request.Key);
            request.OnCompleted?.Invoke();
        }
        
        internal sealed class ReceiveRunner : MonoBehaviour { }

    }
}