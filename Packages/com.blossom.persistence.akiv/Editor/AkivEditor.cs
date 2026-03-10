#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine.Device;

namespace Blossom.Persistence.Akiv.Editor {
    internal static class AkivEditor {
        [MenuItem("Blossom/Akiv/Delete All Akiv")]
        private static void DeleteAllAkivData() {
            string directory = Path.Combine(Application.persistentDataPath, "Akiv");
            if (!Directory.Exists(directory)) return;
            if (!EditorUtility.DisplayDialog("Akiv 삭제", "저장된 모든 Akiv를 삭제합니다.", "삭제", "취소")) return;
            Directory.Delete(directory, true);
        }

        [MenuItem("Blossom/Akiv/Open Akiv Folder")]
        private static void OpenSaveFolder() {
            string path = Application.persistentDataPath;
            EditorUtility.RevealInFinder(path);
        }
    }
}

#endif