using System;
using UnityEngine;
using System.Text;
using System.Collections.Generic;


public class DLog
{
    public enum DLogType
    {
        Log = 1,
        Warning,
        Error,
        Assert,

        NoLog = 999,
    }

    public static string LogTag = "DLog";

    public static DLogType MinLogType = DLogType.Log;

    private static bool IsShow(DLogType type)
    {
        if (!Debug.unityLogger.logEnabled) return false;
        int ret = (int) type - (int) MinLogType;
        if (ret < 0) return false;
        return true;
    }

    #region notag

    public static void LogJson(object pJsonObj)
    {
        if (pJsonObj == null) return;
        if (!IsShow(DLogType.Log)) return;
        try
        {
            var tmsg = UnityEngine.JsonUtility.ToJson(pJsonObj);

            Log(null, tmsg);
        }
        catch (Exception e)
        {
            LogError("LogJsonError:" + pJsonObj);
        }
    }

    public static void LogException(string msg, System.Exception error)
    {
        LogException(null, msg, error);
    }

    public static void Log(object _object)
    {
        OutputString(DLogType.Log, null, _object);
    }

    public static void LogWarning(object _object)
    {
        OutputString(DLogType.Warning, null, _object);
    }

    public static void LogError(object _object)
    {
        OutputString(DLogType.Error, null, _object);
    }

    public static void LogAssertion(object _object)
    {
        OutputString(DLogType.Assert, null, _object);
    }


    public static void LogFormat(string format, params object[] args)
    {
        OutputFormatString(DLogType.Log, null, format, args);
    }

    public static void LogWarningFormat(string format, params object[] args)
    {
        OutputFormatString(DLogType.Warning, null, format, args);
    }

    public static void LogErrorFormat(string format, params object[] args)
    {
        OutputFormatString(DLogType.Error, null, format, args);
    }

    #endregion

    public static void LogJson(string tag, object pJsonObj)
    {
        if (pJsonObj == null) return;
        if (!IsShow(DLogType.Log)) return;
        try
        {
            var tmsg = UnityEngine.JsonUtility.ToJson(pJsonObj);

            Log(tag, tmsg);
        }
        catch (Exception e)
        {
            LogError(tag, "LogJsonError:" + pJsonObj);
        }
    }


    public static void Log(string tag, object _object)
    {
        OutputString(DLogType.Log, tag, _object);
    }

    public static void LogWarning(string tag, object _object)
    {
        OutputString(DLogType.Warning, tag, _object);
    }

    public static void LogError(string tag, object _object)
    {
        OutputString(DLogType.Error, tag, _object);
    }

    public static void LogAssertion(string tag, object _object)
    {
        OutputString(DLogType.Assert, tag, _object);
    }

    public static void LogException(string tag, string msg, System.Exception error)
    {
        if (!IsShow(DLogType.Error)) return;
        try
        {
            Log(tag, msg);
            LogErrorFormat(tag, "[Exception]{0}", error);
        }
        catch (System.Exception e)
        {
            LogError(tag, e);
        }
    }

    public static void LogFormat(string tag, string format, params object[] args)
    {
        OutputFormatString(DLogType.Log, tag, format, args);
    }

    public static void LogWarningFormat(string tag, string format, params object[] args)
    {
        OutputFormatString(DLogType.Warning, tag, format, args);
    }

    public static void LogErrorFormat(string tag, string format, params object[] args)
    {
        OutputFormatString(DLogType.Error, tag, format, args);
    }


    private static void OutputString(DLogType type, string tag, object tar)
    {
        try
        {
            if (!IsShow(type)) return;
            OutputLog(type, tag, tar == null ? "Null" : tar.ToString());
        }
        catch (System.Exception ex)
        {
            Debug.LogError(tar);
            Debug.LogError(ex);
        }
    }

    private static void OutputFormatString(DLogType type, string tag, string format, params object[] args)
    {
        try
        {
            if (!IsShow(type)) return;
            string msg = null;
            if (args.Length == 1)
            {
                msg = string.Format(format, args[0]);
            }
            else if (args.Length == 2)
            {
                msg = string.Format(format, args[0], args[1]);
            }
            else if (args.Length == 3)
            {
                msg = string.Format(format, args[0], args[1], args[2]);
            }
            else
            {
                msg = string.Format(format, args);
            }

            OutputLog(type, tag, msg);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(format);
            Debug.LogError(ex);
        }
    }

    private static void OutputLog(DLogType type, string tag, string msg)
    {
        string tmsg = null;
        if (string.IsNullOrEmpty(tag))
        {
            tmsg = $"[{LogTag}]: {msg} ";
        }
        else
        {
            tmsg = $"[{LogTag}][{tag}]: {msg} ";
        }

        switch (type)
        {
            case DLogType.Log:
                UnityEngine.Debug.Log(tmsg);
                break;
            case DLogType.Error:
                UnityEngine.Debug.LogError(tmsg);
                break;
            case DLogType.Warning:
                UnityEngine.Debug.LogWarning(tmsg);
                break;
            case DLogType.Assert:
                UnityEngine.Debug.LogAssertion(tmsg);
                break;
            case DLogType.NoLog:
                break;
            default:
                break;
        }
    }
}