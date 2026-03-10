using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Blossom.Networking.Online {
    
    internal interface IOnlineAccessValidator {
        Task<bool> Validate(CancellationToken token = default);
    }
    
    internal class UnityAccessValidator : IOnlineAccessValidator {
        
        public Task<bool> Validate(CancellationToken token = default) {
            return token.IsCancellationRequested ? Task.FromResult(false) : Task.FromResult(Application.internetReachability != NetworkReachability.NotReachable);
        }
        
    }
    
    internal class PingAccessValidator : IOnlineAccessValidator {
        
        private readonly string[] _hosts;
        private readonly float _timeout;

        public PingAccessValidator(string[] hosts, float timeout) {
            _hosts = hosts;
            _timeout = timeout;
        }

        public async Task<bool> Validate(CancellationToken token = default) {
            return await OnlineSystem.Retry(TryAllHosts, _timeout, token);
        }

        private async Task<bool> TryAllHosts(CancellationToken token) {
            foreach (string host in _hosts)
                if (await TryHost(host, token)) return true;
            return false;
        }

        private async Task<bool> TryHost(string host, CancellationToken token) {
            try {
                Ping ping = new(host);
                Stopwatch stopWatch = Stopwatch.StartNew();

                while (!ping.isDone) {
                    if (token.IsCancellationRequested || stopWatch.Elapsed.TotalSeconds > _timeout) return false;
                    await Task.Delay(50, token);
                }

                return ping.isDone && ping.time >= 0;
            }
            catch (Exception) {
                return false;
            }
        }
        
    }
    
    internal class HttpAccessValidator : IOnlineAccessValidator {
        
        private readonly string[] _httpUrls;
        private readonly float _timeout;

        public HttpAccessValidator(string[] httpUrls, float timeout) {
            _httpUrls = httpUrls;
            _timeout = timeout;
        }

        public async Task<bool> Validate(CancellationToken token = default) {
            return await OnlineSystem.Retry(TryAllUrls, _timeout, token);
        }

        private async Task<bool> TryAllUrls(CancellationToken token) {
            foreach (string url in _httpUrls)
                if (await TryUrl(url, token)) return true;
            return false;
        }

        private async Task<bool> TryUrl(string url, CancellationToken token) {
            try {
                using UnityWebRequest request = UnityWebRequest.Head(url);
                request.timeout = Mathf.CeilToInt(_timeout);

                UnityWebRequestAsyncOperation operation = request.SendWebRequest();
                while (!operation.isDone) {
                    if (token.IsCancellationRequested) {
                        request.Abort();
                        return false;
                    }

                    await Task.Delay(50, token);
                }

                return request.result == UnityWebRequest.Result.Success;
            }
            catch (Exception) {
                return false;
            }
        }
        
    }
    
}
