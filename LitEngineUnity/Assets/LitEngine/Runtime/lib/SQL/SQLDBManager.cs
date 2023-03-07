using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace LitEngine.SQL
{
    public class SQLDBManager : MonoBehaviour
    {
        private static SQLDBManager sInstance;

        private static SQLDBManager Instance
        {
            get
            {
                if (sInstance == null)
                {
                    GameObject tobj = new GameObject("SQLDBManager");
                    GameObject.DontDestroyOnLoad(tobj);
                    tobj.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
                    sInstance = tobj.AddComponent<SQLDBManager>();
                }

                return sInstance;
            }
        }

        private ConcurrentDictionary<string, SQLDBObject> dbMap = new ConcurrentDictionary<string, SQLDBObject>();

        public static SQLDBObject GetDB(string pDBName)
        {
            try
            {
                if (Instance.dbMap.ContainsKey(pDBName))
                {
                    if (Instance.dbMap.TryGetValue(pDBName, out SQLDBObject ret))
                    {
                        return ret;
                    }
                }
                else
                {
                    var newDB = new SQLDBObject(pDBName);
                    if (newDB.Inited)
                    {
                        Instance.dbMap.TryAdd(pDBName, newDB);
                        return newDB;
                    }
                }
            }
            catch (Exception e)
            {
                SQLLog.LogError(e);
            }

            SQLLog.LogError($"Creat db failed.name = {pDBName}");
            return null;
        }

        public static void CloseDB(string pDBName)
        {
            try
            {
                if (Instance.dbMap.TryRemove(pDBName, out SQLDBObject outDb))
                {
                    outDb.Close();
                }
            }
            catch (Exception e)
            {
                SQLLog.LogError(e);
            }
        }

        public static void CloseAll()
        {
            Instance.Clear();
        }

        private void Clear()
        {
            try
            {
                foreach (var cur in dbMap)
                {
                    cur.Value.Close();
                }
                dbMap.Clear();
            }
            catch (Exception e)
            {
                SQLLog.LogError(e);
            }
        }

        private void OnDestroy()
        {
            Clear();
        }
    }
}