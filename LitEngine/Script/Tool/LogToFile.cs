using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
public class LogToFile
{
    private static string sFilePath = "";
    private static bool sIsInit = false;
    private static Dictionary<UnityEngine.LogType, string> sStackLogTypeList = new Dictionary<LogType, string>();
    private static Dictionary<UnityEngine.LogType, string> sSaveToFileLogTypeList = new Dictionary<LogType, string>();
    public static void InitLogCallback()
    {
        if (sIsInit) return;
        sIsInit = true;

        AddSaveToFileLogType(UnityEngine.LogType.Error);
        AddSaveToFileLogType(UnityEngine.LogType.Assert);
        AddSaveToFileLogType(UnityEngine.LogType.Exception);

        AddStackLogType(UnityEngine.LogType.Error);
        AddStackLogType(UnityEngine.LogType.Assert);
        AddStackLogType(UnityEngine.LogType.Exception);

        Application.logMessageReceivedThreaded += logCallback;

        if (Application.platform == RuntimePlatform.WindowsEditor)
            sFilePath = Application.dataPath + "/../LogFile/";
        else
            sFilePath = Application.persistentDataPath + "/LogFile/";

        if (!Directory.Exists(sFilePath))
            Directory.CreateDirectory(sFilePath);
        string filename = GetNowLogName();
        File.AppendAllText(filename, GetTitleStr(), System.Text.Encoding.UTF8);
    }

    public static void AddSaveToFileLogType(UnityEngine.LogType _type)
    {
        if (sSaveToFileLogTypeList.ContainsKey(_type)) return;
        sSaveToFileLogTypeList.Add(_type, _type.ToString());
    }

    public static void AddStackLogType(UnityEngine.LogType _type)
    {
        if (sStackLogTypeList.ContainsKey(_type)) return;
        sStackLogTypeList.Add(_type, _type.ToString());
    }

    public static string GetTagStr(int _len, string _tag)
    {
        StringBuilder ret = new StringBuilder();
        for (int i = 0; i < _len; i++)
            ret.Append(_tag);
        return ret.ToString();
    }

    public static string GetTitleStr()
    {
        string filename = GetNowLogName();

        StringBuilder ttopbuilder = new StringBuilder();

        List<string> tmsglist = new List<string>();
        tmsglist.Add(string.Format("* 时间:{0} ", DateTime.Now));
        tmsglist.Add(string.Format("* 文件:{0} ", filename));
        int tmaxlen = 0;
        for (int i = 0; i < tmsglist.Count; i++)
        {
            if (tmaxlen < tmsglist[i].Length)
                tmaxlen = tmsglist[i].Length;
        }
        tmaxlen += 1;
        string ttitle = " Application Start Message ";
        int ttitlelen = ttitle.Length;
        int ttaglen = Math.Abs(tmaxlen - ttitlelen) + 2;
        ttaglen = (ttaglen % 2 == 0 ? ttaglen : ttaglen + 1) / 2;

        string ttoptag = GetTagStr(ttaglen, "*");
        string ttopstr = string.Format("{0}{1}{2}", ttoptag, ttitle, ttoptag);

        string tdownmsg = " Message End ";
        string tdowntag = GetTagStr((ttopstr.Length - tdownmsg.Length) / 2, "*");
        string tdownstr = string.Format("{0}{1}{2} ", tdowntag, tdownmsg, tdowntag);


        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine();
        sb.AppendLine(ttopstr);

        for (int i = 0; i < tmsglist.Count; i++)
        {
            sb.AppendLine(tmsglist[i] + GetTagStr(ttopstr.Length - tmsglist[i].Length - 3, " ") + "*");
        }

        sb.AppendLine(tdownstr);

        return sb.ToString();
    }

    public static void logCallback(string log, string stackTrace, UnityEngine.LogType _type)
    {
        if (!sSaveToFileLogTypeList.ContainsKey(_type)) return;
        SaveToFile(_type.ToString(), log, stackTrace, _type);
    }

    public static string GetNowLogName()
    {
        DateTime now = DateTime.Now;
        return GetLogName(now.Year, now.Month, now.Day);
    }

    public static string GetLogName(int _Year, int _Month, int _Day)
    {
        return string.Format("{0}/log_{1}_{2}_{3}.log", sFilePath, _Year, _Month, _Day);
    }

    static void SaveToFile(string prefix, string content, string callstack, UnityEngine.LogType _type)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append("[");
        sb.Append(prefix);
        sb.Append("] ");
        sb.Append(string.Format("[{0}] ", System.DateTime.Now));
        sb.AppendLine(content);

        if(sStackLogTypeList.ContainsKey(_type))
        {
            sb.AppendLine("*******************堆栈*******************");
            if (callstack.Length > 2)
                sb.AppendLine(callstack);
            else
                sb.AppendLine(GetStackTrace());
            sb.AppendLine("******************************************");    
        }

        System.IO.File.AppendAllText(GetNowLogName(), sb.ToString(), System.Text.Encoding.UTF8);
    }

    #region 堆栈信息
    public static string GetStackTrace()
    {
        StringBuilder tstacktracebuilder = new StringBuilder();

        StackTrace stackTrace = new StackTrace(true);
        StackFrame[] stackFrame = stackTrace.GetFrames();
        if (stackFrame != null)
        {
            for (int i = 0; i < stackFrame.Length; i++)
            {
                StackFrame tframe = stackFrame[i];
                MethodBase tmethod = tframe.GetMethod();
                if (tmethod.DeclaringType == typeof(LogToFile)
                    || tmethod.DeclaringType == typeof(DLog)
                    || tmethod.DeclaringType == typeof(LitEngine.CodeTool_LS)
                    || tmethod.DeclaringType == typeof(Logger)
                    || tmethod.DeclaringType == typeof(Application)
                    || tmethod.DeclaringType.ToString().Contains("DebugLogHandler")
                    || tmethod.DeclaringType.ToString().Contains("ILRuntime."))
                    continue;
                ParameterInfo[] tparams = tmethod.GetParameters();
                StringBuilder tparamsstr = new StringBuilder();
                for (int j = 0; j < tparams.Length; j++)
                {
                    tparamsstr.Append(tparams[j].ParameterType.ToString());
                    if (j < tparams.Length - 1)
                        tparamsstr.Append(",");
                }
                string tmsg = string.Format("{0}:{1}({2})(line:{3})", tmethod.DeclaringType.ToString(), tmethod.Name, tparamsstr.ToString(), tframe.GetFileLineNumber());
                tstacktracebuilder.AppendLine(tmsg);
            }
        }

        return tstacktracebuilder.ToString();
    }
    #endregion

}


