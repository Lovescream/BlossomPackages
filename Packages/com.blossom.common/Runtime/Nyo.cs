using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using Debug = UnityEngine.Debug;

public static class Nyo {

    private const string Tag = "NYO";

    public static bool Enabled = true;

    [Conditional(Tag)]
    public static void Log(object message, [CallerFilePath] string file = "", [CallerMemberName] string member = "", [CallerLineNumber] int line = 0) {
        if (!Enabled) return;
        Write(LogType.Log, message, file, member, line);
    }

    [Conditional(Tag)]
    public static void Warning(object message, [CallerFilePath] string file = "",[CallerMemberName] string member = "", [CallerLineNumber] int line = 0) {
        if (!Enabled) return;
        Write(LogType.Warning, message, file, member, line);
    }

    [Conditional(Tag)]
    public static void Error(object message, [CallerFilePath] string file = "",[CallerMemberName] string member = "", [CallerLineNumber] int line = 0) {
        if (!Enabled) return;
        Write(LogType.Error, message, file, member, line);
    }

    [Conditional(Tag)]
    private static void Write(LogType type, object message, string file, string member, int line) {
        string className = System.IO.Path.GetFileNameWithoutExtension(file);
        string text = $"[{Tag}:{className}:{line}] {member}(): {message}";

        switch (type) {
            case LogType.Warning: Debug.LogWarning(text); break;
            case LogType.Error:
            case LogType.Exception: Debug.LogError(text); break;
            default: Debug.Log(text); break;
        }
    }
}