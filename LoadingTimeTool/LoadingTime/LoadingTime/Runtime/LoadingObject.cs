using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;
namespace LoadingTimeTool
{
    public class LoadingData
    {
        public string title;
        public string content;
        public string methodName;
        public string className;
        public double logUseTime = 0;
        public long delayTime = 0;
        public int stepIndex = 0;
       
    }
    
    public class JsonDataOutput
    {
        public long totalTime = 0;
        public int stepCount = 0;
        public List<LoadingData> dataList;
    }
    
    public class LoadingObject
    {
        public string Key { get; private set; }
        public long totalTime { get; private set; } = 0;
        public int stepIndex { get; private set; } = 0;
        
        long startTicks;
        long endTicks;
        long lastStepTicks = 0;
        long startStepTicks = 0;
        double allLogTime = 0;
        
        
        List<LoadingData> loadingDataList = new List<LoadingData>(50);
        public LoadingObject(string pTag)
        {
            Key = pTag;
        }

        public List<LoadingData> GetLoadingDataList()
        {
            return loadingDataList;
        }
        public string GetJson()
        {
            return ToJson(loadingDataList);
        }

        public void Step(string title, string content = null)
        {
            try
            {
                startTicks = DateTime.Now.Ticks;

                StackTrace stackTrace = new StackTrace();

                // 获取堆栈中的帧信息
                StackFrame[] stackFrames = stackTrace.GetFrames();

                string tstr = null;

                var tdata = new LoadingData();
                tdata.content = content;
                tdata.title = title;
                
                if (stackFrames != null && stackFrames.Length > 2)
                {
                    var frame = stackFrames[2];

                    MethodBase method = frame.GetMethod();
                    
                    tdata.className = method.ReflectedType.Name;
                    tdata.methodName = method.Name;
                    
                    tstr = $"Method: {tdata.methodName}, Class: {tdata.className}";
                }
                
                loadingDataList.Add(tdata);
                
                string ttag = string.IsNullOrEmpty(title) ? "Step" : title;
                Log(tdata, $"Tag: {ttag}, {tstr}");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e);
            }

        }


        public void Log(LoadingData data, string str)
        {
            DateTime currentTime = DateTime.Now;
            long tc = 0;
            if (lastStepTicks != 0)
            {
                tc = currentTime.Ticks - lastStepTicks;
            }
            else
            {
                startStepTicks = currentTime.Ticks;
            }

            lastStepTicks = currentTime.Ticks;
            endTicks = DateTime.Now.Ticks;

            // 使用标准日期格式输出
            string formattedTime = currentTime.ToString("HH:mm:ss.fff");

            long alltime = Mathf.FloorToInt((lastStepTicks - startStepTicks) / 10000f);
            long delay = Mathf.FloorToInt(tc / 10000f);
            double delay2 = (endTicks - startTicks) / 10000f;
            allLogTime += delay2;
            totalTime = alltime;
            
            data.delayTime = delay;
            data.logUseTime = delay2;
            data.stepIndex = stepIndex;
            
            UnityEngine.Debug.Log(
                $"[{formattedTime}][LoadingTime][{Key}][Step({stepIndex++})] AllTime: {alltime}ms, Delay: {delay}ms, {str}, logDelay:{delay2}ms, allLogTime:{allLogTime}");
        }
        
        public static string ToJson(object pData)
        {
            if (pData == null) return null;
            try
            {
                if (pData is string) return (string)pData;

                var setting = new JsonSerializerSettings();
                //setting.NullValueHandling = NullValueHandling.Ignore;

                return Newtonsoft.Json.JsonConvert.SerializeObject(pData, null, setting);
            }
            catch (System.Exception error)
            {
                UnityEngine.Debug.LogError($"ToJson: error={error.Message}, Json={pData}");
            }
            return null;
        }
    }
}