using UnityEngine;
using System.Text;
using System.Collections.Generic;

public enum LogColor
{
    NONE = 0,
    BLUE,
    YELLO,
    RED,
    GREEN,
    AQUA,
    WHITE,
}
public enum DLogType
{
    Log = 1,
    Warning,
    Error,
    Assert,
    TrueLog,
}

public class DLog
{
   
    public static DLogType MinLogType = DLogType.Log;
    private static bool IsShow(DLogType _type)
    {
        int ret = (int)_type - (int)MinLogType;
        if (ret < 0) return false;
        return true;
    }
    protected static string ColorString(LogColor _color)
    {
        string ret = null;
        switch(_color)
        {
            case LogColor.BLUE:
                ret = "<color=blue>";
                break;
            case LogColor.YELLO:
                ret = "<color=yellow>";
                break;
            case LogColor.RED:
                ret = "<color=red>";
                break;
            case LogColor.GREEN:
                ret = "<color=green>";
                break;
            case LogColor.AQUA:
                ret = "<color=aqua>";
                break;
            case LogColor.WHITE:
                ret = "<color=white>";
                break;
        }
        return ret;
    }

    public static void LOGColor(DLogType _type, string _msg, LogColor _color)
    {
        if (!IsShow(_type)) return;
        
        StringBuilder tbuilder = new StringBuilder();
        tbuilder.Append(_msg == null ? "Null" : _msg);

        string tcolorstr = ColorString(_color);
        if (tcolorstr != null)
        {
            tbuilder.Insert(0, tcolorstr);
            tbuilder.Append("</color>");
        }

        string tmsg = tbuilder.ToString();
        switch (_type)
        {
            case DLogType.TrueLog:
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
            default:
                break;
        }
    }
    public static void Log(object _object)
    {
        if (!IsShow(DLogType.Log)) return;
        LOGColor(DLogType.Log, _object == null ? "Null" : _object.ToString(), LogColor.NONE);
    }

    public static void LogWarning(object _object)
    {
        if (!IsShow(DLogType.Warning)) return;
        LOGColor(DLogType.Warning, _object == null ? "Null" : _object.ToString(), LogColor.NONE);
    }

    public static void LogError(object _object)
    {
        if (!IsShow(DLogType.Error)) return;
        LOGColor(DLogType.Error, _object == null ? "Null" : _object.ToString(), LogColor.NONE);
    }

    public static void LogAssertion(object _object)
    {
        if (!IsShow(DLogType.Assert)) return;
        LOGColor(DLogType.Assert, _object == null ? "Null" : _object.ToString(), LogColor.NONE);
    }

    public static void LogFormat(string _formatstr, params object[] _params)
    {
        if (!IsShow(DLogType.Log)) return;
        LOGColor(DLogType.Log, string.Format(_formatstr, _params), LogColor.NONE);
    }

    public static void LogWarningFormat(string _formatstr, params object[] _params)
    {
        if (!IsShow(DLogType.Warning)) return;
        LOGColor(DLogType.Warning, string.Format(_formatstr, _params), LogColor.NONE);
    }

    public static void LogErrorFormat(string _formatstr, params object[] _params)
    {
        if (!IsShow(DLogType.Error)) return;
        LOGColor(DLogType.Error, string.Format(_formatstr, _params), LogColor.NONE);
    }

}

