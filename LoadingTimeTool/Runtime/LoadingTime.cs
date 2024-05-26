
using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class LoadingTime
{
    public static void Step(string tag = null)
    {
        StackTrace stackTrace = new StackTrace();

        // 获取堆栈中的帧信息
        StackFrame[] stackFrames = stackTrace.GetFrames();

        string tstr = null;

        if (stackFrames != null && stackFrames.Length > 1)
        {
            var frame = stackFrames[1];
            MethodBase method = frame.GetMethod();
            tstr = $"Method: {method.Name}, Class: {method.ReflectedType.Name}";
        }
        string ttag = string.IsNullOrEmpty(tag) ? "Step" : tag;
        Log($"Tag: {ttag}, {tstr}");
    }

    static long lastStepTicks = 0;
    public static void Log(string str)
    {
        DateTime currentTime = DateTime.Now;
        long tc = 0;
        if (lastStepTicks != 0)
        {
            tc = currentTime.Ticks - lastStepTicks;
        }
        lastStepTicks = currentTime.Ticks;
        // 使用标准日期格式输出
        string formattedTime = currentTime.ToString("HH:mm:ss.fff");
        
        Debug.Log($"[{formattedTime}][LoadingTime] Delay: {Mathf.FloorToInt(tc / 10000f)}ms, {str}");
    }
}
