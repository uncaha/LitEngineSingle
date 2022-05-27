
using System.Net;
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LitEngine.Net
{
    public class HttpCacheObject
    {
        const int fieldMax = 4;
        public string Url { get { return dataList[0]; } internal set { dataList[0] = value; } }
        public string ETag { get { return dataList[1]; } internal set { dataList[1] = value; } }
        public string LastModified { get { return dataList[2]; } internal set { dataList[2] = value; } }
        public string responseData { get { return dataList[3]; } internal set { dataList[3] = value; } }

        public bool waitSave { get; private set; } = false;
        public bool cached { get; private set; } = false;
        public string filePath { get; private set; } = "";
        string[] dataList = new string[fieldMax];

        public HttpCacheObject(string pUrl, bool cache = false)
        {
            for (int i = 0; i < fieldMax; i++)
            {
                dataList[i] = "";
            }

            Url = pUrl;
            cached = cache;

            filePath = HttpCacheManager.Instance.GetFIlePathByKey(Url);
        }

        internal void LoadCache()
        {
            try
            {
                cached = false;
                if (File.Exists(filePath))
                {
                    var tlist = File.ReadAllLines(filePath);
                    if (tlist.Length == fieldMax)
                    {
                        dataList = tlist;
                        cached = true;
                    }
                    else
                    {
                        throw new ArgumentNullException($"load cache failed.len = {tlist.Length}, url = {Url}");
                    }
                }

            }
            catch (System.Exception e)
            {
                DLog.LogError(e.Message);
            }
        }

        string savePath = "";
        string[] waitSaveData = new string[fieldMax];
        bool dataUpdated = false;
        internal async void SaveCache()
        {
            if (string.IsNullOrEmpty(ETag) || string.IsNullOrEmpty(Url) || string.IsNullOrEmpty(responseData)) return;

            if (waitSave)
            {
                dataUpdated = true;
                return;
            }

            waitSave = true;
            Array.Copy(dataList, 0, waitSaveData, 0, fieldMax);
            savePath = filePath;
            await Task.Run((Action)TaskSave);

            waitSave = false;

            if (dataUpdated)
            {
                dataUpdated = false;
                SaveCache();
            }
        }

        void TaskSave()
        {
            try
            {
                File.WriteAllLines(savePath, waitSaveData);
                cached = true;
            }
            catch (System.Exception e)
            {
                DLog.LogError(e.Message);
            }
        }

    }
}
