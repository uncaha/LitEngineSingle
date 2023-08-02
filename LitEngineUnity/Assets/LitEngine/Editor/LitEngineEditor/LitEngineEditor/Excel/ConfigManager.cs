//using System;
//using System.Collections.Generic;
//namespace Config{
//    public abstract class ConfigBase { }
//    public class ConfigManager{
//        private static object lockobj = new object();
//        private static ConfigManager sInstance = null;
//        public static ConfigManager Instance{
//            get{
//                if (sInstance == null){
//                    lock (lockobj){
//                        if (sInstance == null){
//                            sInstance = new ConfigManager();
//                        }
//                    }
//                }
//                return sInstance;
//            }
//        }
//        private Dictionary<Type, ConfigBase> Dic = new Dictionary<Type, ConfigBase>();
//        private ConfigManager() { }
//        private void Add<T>() where T : ConfigBase, new(){
//            T tcfg = new T();
//            Dic.Add(typeof(T), tcfg);
//        }

//        public static T Get<T>() where T : ConfigBase{
//            if (!Instance.Dic.ContainsKey(typeof(T))) return null;
//            return (T)Instance.Dic[typeof(T)];
//        }

//    }
//}
