using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Blossom.PackageManager.Editor {
    internal static class BlossomPackageInstaller {

        private static ListRequest _listRequest;
        private static AddRequest _addRequest;
        private static RemoveRequest _removeRequest;

        public static void RefreshInstalledPackages(Action<List<string>, Dictionary<string, string>> onComplete) {
            _listRequest = Client.List(true);
            EditorApplication.update += ProgressList;

            void ProgressList() {
                if (!_listRequest.IsCompleted) return;
                EditorApplication.update -= ProgressList;

                List<string> installedNames = new();
                Dictionary<string, string> installedVersions = new();

                if (_listRequest.Status == StatusCode.Success) {
                    foreach (PackageInfo package in _listRequest.Result) {
                        installedNames.Add(package.name);
                        installedVersions[package.name] = package.version;
                    }
                }

                onComplete?.Invoke(installedNames, installedVersions);
            }
        }

        public static void Install(string installId, Action<bool, string> onComplete) {
            _addRequest = Client.Add(installId);
            EditorApplication.update += ProgressAdd;

            void ProgressAdd() {
                if (!_addRequest.IsCompleted) return;
                EditorApplication.update -= ProgressAdd;

                if (_addRequest.Status == StatusCode.Success) onComplete?.Invoke(true, null);
                else onComplete?.Invoke(false, _addRequest.Error?.message);
            }
        }

        public static void Remove(string packageName, Action<bool, string> onComplete) {
            _removeRequest = Client.Remove(packageName);
            EditorApplication.update += ProgressRemove;

            void ProgressRemove() {
                if (!_removeRequest.IsCompleted) return;
                EditorApplication.update -= ProgressRemove;

                if (_removeRequest.Status == StatusCode.Success) onComplete?.Invoke(true, null);
                else onComplete?.Invoke(false, _removeRequest.Error?.message);
            }
        }
    }
}