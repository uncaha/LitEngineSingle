using UnityEngine;
namespace LitEngine.SQL
{
    public class SQLLog
    {
        public static string LogTag = "SQL";


        public static void Log(object pobj)
        {
            DLog.Log($"[{LogTag}] {pobj}");
        }

        public static void LogWarning(object pobj)
        {
            DLog.LogWarning($"[{LogTag}] {pobj}");
        }

        public static void LogError(object pobj)
        {
            DLog.LogError($"[{LogTag}] {pobj}");
        }

        public static void LogAssertion(object pobj)
        {
            DLog.LogAssertion($"[{LogTag}] {pobj}");
        }


        public static void LogFormat(string format, params object[] args)
        {
            DLog.LogFormat($"[{ LogTag}] " + format, args);

        }
        public static void LogWarningFormat(string format, params object[] args)
        {
            DLog.LogWarningFormat($"[{ LogTag}] " + format, args);
        }

        public static void LogErrorFormat(string format, params object[] args)
        {
            DLog.LogErrorFormat($"[{ LogTag}] " + format, args);
        }
    }
}