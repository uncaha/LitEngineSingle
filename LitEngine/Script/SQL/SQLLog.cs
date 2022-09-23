using UnityEngine;
namespace LitEngine.SQL
{
    public class SQLLog
    {
        public static string LogTag = "SQL";


        public static void Log(object pobj)
        {
            Debug.Log($"[{LogTag}] {pobj}");
        }

        public static void LogWarning(object pobj)
        {
            Debug.LogWarning($"[{LogTag}] {pobj}");
        }

        public static void LogError(object pobj)
        {
            Debug.LogError($"[{LogTag}] {pobj}");
        }

        public static void LogAssertion(object pobj)
        {
            Debug.LogAssertion($"[{LogTag}] {pobj}");
        }


        public static void LogFormat(string format, params object[] args)
        {
            Debug.LogFormat($"[{ LogTag}] " + format, args);

        }
        public static void LogWarningFormat(string format, params object[] args)
        {
            Debug.LogWarningFormat($"[{ LogTag}] " + format, args);
        }

        public static void LogErrorFormat(string format, params object[] args)
        {
            Debug.LogErrorFormat($"[{ LogTag}] " + format, args);
        }
    }
}