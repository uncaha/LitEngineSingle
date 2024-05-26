using System;
using System.Collections.Generic;
using System.IO;
namespace ExportTool
{
    public class ExportConfigManager
    {
        string cfgMgrUp = @"
using System;
using System.Collections.Generic;
namespace Config{
    public class ConfigManager{
        private static ConfigManager sInstance = null;
        public static ConfigManager Instance{
            get{
                if (sInstance == null){
                    sInstance = new ConfigManager();
                    sInstance.Init();
                }
                return sInstance;
            }
        }
        
        private ConfigManager() { }";
        string cfgMgrdown = @"
    }
}
        ";

        List<string> configNames;
        string fullPathName;

        string tempFile;
        public ExportConfigManager(string pFullPath, List<string> pNamses)
        {
            fullPathName = pFullPath;
            configNames = pNamses;

            tempFile = fullPathName + ".temp";
        }

        public void StartExport()
        {
            try
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
                using (FileStream tfile = File.OpenWrite(tempFile))
                {
                    ExportTool.TextWriter twt = new ExportTool.TextWriter(tfile);

                    twt.WriteLine(cfgMgrUp);
                    twt.Indent().Indent();
                    foreach (string tcfg in configNames)
                    {
                        twt.WriteLine($"public {tcfg} {tcfg}Data {{ get; private set; }}");
                    }
                    twt.Outdent().Outdent();

                    twt.Indent().Indent();
                    twt.WriteLine("void Init(){");
                    twt.Indent();
                    foreach (string tcfg in configNames)
                    {
                        twt.WriteLine($"{tcfg}Data = new {tcfg}();");
                    }
                    twt.Outdent();
                    twt.WriteLine("}");
                    twt.Outdent().Outdent();
                    
                    
                    twt.WriteLine(cfgMgrdown);
                    twt.Close();
                    tfile.Close();

                    if (File.Exists(fullPathName))
                        File.Delete(fullPathName);
                    File.Move(tempFile, fullPathName);
                }
            }
            catch (Exception pErro)
            {
               DLog.LogError(pErro.ToString());
            }

        }
    }
}
