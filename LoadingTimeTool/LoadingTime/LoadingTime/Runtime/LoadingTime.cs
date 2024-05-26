
using System;
using System.Collections.Generic;
using LoadingTimeTool;
using UnityEngine;

public class LoadingTime
{
    public static bool OpenLoadingTime = false;
    private static LoadingTime sInstance = null;
    public static LoadingTime Instance
    {
        get
        {
            if (sInstance == null)
            {
                sInstance = new LoadingTime();
            }
            return sInstance;
        }
    }
    
    const string mainKey = "Main";
    LoadingObject main = new LoadingObject(mainKey);
    
    Dictionary<string,LoadingObject> loadingObjectDic = new Dictionary<string, LoadingObject>();

    private LoadingTime()
    {
        loadingObjectDic.Add(mainKey,main);
    }
    public static void Rest()
    {
        sInstance = null;
    }
    
    public static string GetJson()
    {
        var tdic = Instance.loadingObjectDic;
        var tdata = new Dictionary<string,JsonDataOutput>();
        foreach (var t in tdic)
        {
            var tobj = new JsonDataOutput();
            tobj.totalTime = t.Value.totalTime;
            tobj.stepCount = t.Value.stepIndex;
            tobj.dataList = t.Value.GetLoadingDataList();
            tdata.Add(t.Key, tobj);
        }
        return LoadingObject.ToJson(tdata);
    }
    
    public static void SaveJson()
    {
        var json = GetJson();
        string formattedTime = DateTime.Now.ToString("HH-mm-ss");
        var path = UnityEngine.Application.persistentDataPath + $"/LoadingTime_{formattedTime}.json";
        Debug.Log($"[LoadingTime] save path:{path}");
        System.IO.File.WriteAllText(path,json);
    }
    
    public static void Step(string tag = null,string type = mainKey)
    {
        if (!OpenLoadingTime || !UnityEngine.Debug.unityLogger.logEnabled) return;
        if(mainKey.Equals(type))
        {
            Instance.main.Step(tag);
        }
        else
        {
            if (Instance.loadingObjectDic.ContainsKey(type))
            {
                Instance.loadingObjectDic[type].Step(tag);
            }
            else
            {
                LoadingObject tObject = new LoadingObject(type.ToString());
                Instance.loadingObjectDic.Add(type,tObject);
                tObject.Step(tag);
            }
        }

    }
}
