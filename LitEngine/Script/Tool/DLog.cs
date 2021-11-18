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
        TrueLog,
    }

    public static string LogTag = "[GuildSDK]";

    public static DLogType MinLogType = DLogType.Log;
    private static bool IsShow(DLogType type)
    {
        int ret = (int)type - (int)MinLogType;
        if (ret < 0) return false;
        return true;
    }



    public static void TagLog(DLogType logType, string tag, object pObject)
    {
        if (!IsShow(DLogType.Log)) return;

        OutputLog(logType, string.Format("[{0}]{1}", tag, pObject));
    }

    public static void TagLogFormat(DLogType logType, string tag, string format, params object[] paramObjs)
    {
        if (!IsShow(DLogType.Log)) return;

        OutputLog(logType, string.Format("[{0}]{1}", tag, string.Format(format, paramObjs)));
    }

    public static void Log(object _object)
    {
        OutputString(DLogType.Log, _object);
    }

    public static void LogWarning(object _object)
    {
        OutputString(DLogType.Warning, _object);
    }

    public static void LogError(object _object)
    {
        OutputString(DLogType.Error, _object);
    }

    public static void LogAssertion(object _object)
    {
        OutputString(DLogType.Assert, _object);
    }

    public static void LogException(string msg, System.Exception error)
    {
        if (!IsShow(DLogType.Error)) return;
        try
        {
            StringBuilder tbuild = new StringBuilder();
            tbuild.Append("[MSG]");
            tbuild.AppendLine(msg);

            tbuild.Append("[Exception]");
            tbuild.AppendLine(error.ToString());

            UnityEngine.Debug.LogError(tbuild.ToString());
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError(e.ToString());
        }
    }

    public static void LogFormat(string format, params object[] args)
    {
        OutputFormatString(DLogType.Log, format, args);
    }

    public static void LogWarningFormat(string format, params object[] args)
    {
        OutputFormatString(DLogType.Warning, format, args);
    }

    public static void LogErrorFormat(string format, params object[] args)
    {
        OutputFormatString(DLogType.Error, format, args);
    }


    private static void OutputString(DLogType type, object tar)
    {
        try
        {
            if (!IsShow(type)) return;
            OutputLog(type, tar == null ? "Null" : tar.ToString());
        }
        catch (System.Exception ex)
        {
            Debug.LogError(tar);
            Debug.LogError(ex.ToString());
        }
    }

    private static void OutputFormatString(DLogType type, string format, params object[] args)
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

            OutputLog(type, msg);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(format);
            Debug.LogError(ex.ToString());
        }
    }

    private static void OutputLog(DLogType type, string msg)
    {
        //var tmsg = string.Format("{0}[Tick:{1}]: {2} ", LogTag, System.DateTime.Now.TimeOfDay, msg);
        switch (type)
        {
            case DLogType.TrueLog:
            case DLogType.Log:
                UnityEngine.Debug.Log(msg);
                break;
            case DLogType.Error:
                UnityEngine.Debug.LogError(msg);
                break;
            case DLogType.Warning:
                UnityEngine.Debug.LogWarning(msg);
                break;
            case DLogType.Assert:
                UnityEngine.Debug.LogAssertion(msg);
                break;
            default:
                break;
        }
    }

}

