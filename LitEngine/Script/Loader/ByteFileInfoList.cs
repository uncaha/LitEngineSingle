using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;
namespace LitEngine.LoadAsset
{
    public class ByteFileInfo
    {
        public string fileFullPath { get; set; }
        public string resName = "";
        public string fileMD5 = "";
        public long fileSize = 0;
        public int priority = 1;
    }
    public class ByteFileInfoList
    {
        private List<ByteFileInfo> _fileInfoList = new List<ByteFileInfo>();
        public List<ByteFileInfo> fileInfoList { get { return _fileInfoList; } }
        public Dictionary<string, ByteFileInfo> fileMap { get { return _fileMap; } }
        Dictionary<string, ByteFileInfo> _fileMap = new Dictionary<string, ByteFileInfo>();

        public ByteFileInfoList()
        {
        }
        
        public ByteFileInfoList(byte[] pData)
        {
           Load(pData);
        }

        public ByteFileInfoList(string pFullPath)
        {
            if (File.Exists(pFullPath))
            {
                byte[] tdata = File.ReadAllBytes(pFullPath);
                Load(tdata);
            }
        }

        public void AddRange(List<ByteFileInfo> pList)
        {
            if(pList == null) return;
            foreach (var item in pList)
            {
                Add(item);  
            }
        }

        public bool Add(ByteFileInfo pInfo)
        {
            if (!fileMap.ContainsKey(pInfo.resName))
            {
                fileMap.Add(pInfo.resName, pInfo);
                fileInfoList.Add(pInfo);
            }

            return false;
        }

        public void RemoveRangeWithOutList(Dictionary<string,string> pTable)
        {
            if(pTable == null) return;
            for (int i = fileInfoList.Count - 1; i >= 0; i--)
            {
                var item = fileInfoList[i];
                string tkey = item.resName;
                if(!pTable.ContainsKey(item.resName))
                {
                    Remove(item.resName);
                }
            }
        }

        public void RemoveRange(List<string> pKeys)
        {
            if(pKeys == null) return;
            foreach (var item in pKeys)
            {
                Remove(item);
            }
        }

        public void Remove(string pKey)
        {
            if (fileMap.ContainsKey(pKey))
            {
                var item = fileMap[pKey];
                fileMap.Remove(pKey);
                fileInfoList.Remove(item);
            }
        }

        public ByteFileInfo this[string pKey]
        {
            get
            {
                if(fileMap.ContainsKey(pKey))
                {
                    return fileMap[pKey];
                }
                return null;
            }
        }

        public ByteFileInfo this[int pIndex]
        {
            get
            {
                if(pIndex >=0 && pIndex < fileInfoList.Count)
                {
                    return fileInfoList[pIndex];
                }
                return null;
            }
        }

        public void Load(byte[] pData)
        {
            if (pData != null)
            {
                string item = null;
                try
                {
                    fileMap.Clear();
                    fileInfoList.Clear();
                    List<string> tlines = new List<string>();
                    StreamReader treader = new StreamReader(new MemoryStream(pData));
                    item = treader.ReadLine();
                    while (item != null)
                    {
                        tlines.Add(item);
                        item = treader.ReadLine();
                    }
                    LoadByLines(tlines.ToArray());
                }
                catch (System.Exception erro)
                {
                    Debug.LogErrorFormat("初始化数据列出错.item = {0},erro = {1}", item, erro.Message);
                }
            }
        }

        public void Save(string pFullPath)
        {
            try
            {
                string tfilePath = pFullPath;
                if (File.Exists(tfilePath))
                {
                    File.Delete(tfilePath);
                }
                StringBuilder tstrbd = new StringBuilder();

                for (int i = 0,tcount = fileInfoList.Count; i < tcount; i++)
                {
                    var item = fileInfoList[i];
                    string tline = UnityEngine.JsonUtility.ToJson(item);
                    tstrbd.AppendLine(tline);
                }
                File.AppendAllText(tfilePath, tstrbd.ToString());
            }
            catch (System.Exception ex)
            {
                Debug.LogError("生成文件信息出现错误, error:" + ex.Message);
            }

            Debug.Log("生成文件信息完成.");
        }

        private void LoadByLines(string[] pLines)
        {
            if (pLines == null || pLines.Length == 0) return;

            int i = 0, len = pLines.Length;
            try
            {
                for (; i < len; i++)
                {
                    ByteFileInfo tinfo = UnityEngine.JsonUtility.FromJson<ByteFileInfo>(pLines[i]);
                    Add(tinfo);
                }
            }
            catch (System.Exception erro)
            {
                Debug.LogErrorFormat("初始化json数据出现错误.line = {0}, str = {1}, erro = {2}", i, pLines[i], erro.Message);
            }
        }
    
        public List<ByteFileInfo> Comparison(ByteFileInfoList pSor)
        {
            if(pSor == null) return new List<ByteFileInfo>();
            List<ByteFileInfo> ret = new List<ByteFileInfo>();
            foreach (var item in pSor.fileInfoList)
            {
                bool isNeedUpdate = false;
                if(!fileMap.ContainsKey(item.resName))
                {
                    isNeedUpdate = true;
                }
                else
                {
                    var ttar = fileMap[item.resName];
                    if(ttar.fileSize != item.fileSize || !ttar.fileMD5.Equals(item.fileMD5))
                    {
                        isNeedUpdate = true;
                    }
                }


                if(isNeedUpdate)
                {
                    ret.Add(item);
                }
            }
            return ret;
        }
    }
}
