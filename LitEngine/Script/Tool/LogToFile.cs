using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Threading;
using System.Collections.Generic;
namespace LitEngine.Log
{
    public class LogToFile : IDisposable
    {
        private class LogData
        {
            public string logStr;
            public string stackTrace;
            public LogType type;
        }
        private static LogToFile sInstance = null;

        private string sFilePath = "";
        private int mainThreadID = -1;

        private Queue logQue = Queue.Synchronized(new Queue());
        private Thread saveThread;
        private bool saveThreadRunning = false;

        private StreamWriter logWriger = null;
        public static void InitLogCallback()
        {
            if (sInstance != null) return;
            sInstance = new LogToFile();
        }

        private LogToFile()
        {
            mainThreadID = Thread.CurrentThread.ManagedThreadId;

            Application.logMessageReceived += logCallback;
            Application.logMessageReceivedThreaded += logCallbackTrhread;
            Application.quitting += OnQuit;

            this.saveThreadRunning = true;
            saveThread = new Thread(SaveFileThread);
            saveThread.Start();



            if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor)
                sFilePath = Application.dataPath + "/../LogFile/";
            else
                sFilePath = Application.persistentDataPath + "/LogFile/";

            if (!Directory.Exists(sFilePath))
                Directory.CreateDirectory(sFilePath);
            string filename = GetNowLogName();
            logWriger = new StreamWriter(filename, true, Encoding.UTF8);

            logWriger.WriteLine(GetTitleStr());
            logWriger.Flush();
        }

        ~LogToFile()
        {
            Dispose(false);
        }

        protected bool mDisposed = false;
        public void Dispose()
        {
            Application.logMessageReceived -= logCallback;
            Application.logMessageReceivedThreaded -= logCallbackTrhread;
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        protected void Dispose(bool _disposing)
        {
            if (mDisposed)
                return;
            mDisposed = true;
            this.saveThreadRunning = false;

            try
            {
                if (logWriger != null)
                {
                    logWriger.Close();
                }
                logWriger = null;
            }
            catch (Exception ex)
            {
            }

            try
            {
                saveThread.Abort();
            }
            catch (ThreadInterruptedException ex)
            {
            }

        }


        public string GetTagStr(int _len, string _tag)
        {
            StringBuilder ret = new StringBuilder();
            for (int i = 0; i < _len; i++)
                ret.Append(_tag);
            return ret.ToString();
        }

        public string GetNowLogName()
        {
            DateTime now = DateTime.Now;
            return GetLogName(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
        }

        public string GetLogName(int _Year, int _Month, int _Day, int _hour, int _min, int _sec)
        {
            return string.Format("{0}/log_{1}_{2}_{3}.log", sFilePath, _Year, _Month, _Day);
        }


        public string GetTitleStr()
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

        void logCallback(string log, string pStackTrace, UnityEngine.LogType pType)
        {
            if (mainThreadID != Thread.CurrentThread.ManagedThreadId) return;
            LogData tdata = new LogData() { logStr = log, stackTrace = pStackTrace, type = pType };
            LogOutPut(tdata);
        }

        void logCallbackTrhread(string log, string pStackTrace, UnityEngine.LogType pType)
        {
            if (mainThreadID == Thread.CurrentThread.ManagedThreadId) return;
            LogData tdata = new LogData() { logStr = log, stackTrace = pStackTrace, type = pType };
            LogOutPut(tdata);
        }

        void LogOutPut(LogData pData)
        {
            logQue.Enqueue(pData);
        }

        void OnQuit()
        {
            Dispose();
        }

        private void SaveFileThread()
        {
            while (saveThreadRunning)
            {
                if (logQue.Count == 0)
                {
                    continue;
                }

                while (this.logQue.Count > 0)
                {
                    LogData log = (LogData)this.logQue.Dequeue();
                    SaveToFile(log.type.ToString(), log.logStr, log.stackTrace, log.type);
                }
            }
        }

        void SaveToFile(string prefix, string content, string callstack, UnityEngine.LogType pType)
        {
            try
            {

                if (pType == LogType.Assert || pType == LogType.Error || pType == LogType.Exception)
                {

                    logWriger.WriteLine("┌──────────────────────────────────ERROR──────────────────────────────────┐");
                    logWriger.WriteLine(string.Format("[{0}][{1}] {2}", prefix, System.DateTime.Now, content));
                    logWriger.WriteLine(callstack);
                    logWriger.WriteLine("└─────────────────────────────────────────────────────────────────────────┘");
                }
                else
                {
                    logWriger.WriteLine(string.Format("[{0}][{1}] {2}", prefix, System.DateTime.Now, content));
                }

                logWriger.Flush();
            }
            catch (Exception _e)
            {
            }

        }
    }
}




