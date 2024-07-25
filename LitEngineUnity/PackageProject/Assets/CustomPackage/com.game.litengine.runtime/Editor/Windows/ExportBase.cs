using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LitEngineEditor
{
    public enum ExportWType
    {
        AssetsWindow = 0,
        ExcelWindow,
        PrptoWindow,
        EncryptToolWindow,
        MeshToolWindow,
        
    }
    public abstract class ExportBase
    {
        public ExportWindow mWindow;
        private static ExportConfig sConfig = null;
        public static ExportConfig Config
        {
            get
            {
                if (sConfig == null)
                {
                    sConfig = new ExportConfig();
                }
                    
                return sConfig;
            }
        }
        public ExportWType ExWType { get; protected set; }
        public static void RestConfig()
        {
            ExportSetting.LoadCFG();
        }
        public ExportBase()
        {
            
        }

        public void NeedSaveSetting()
        {
            mWindow.NeedSaveSetting = true;
        }

        abstract public void OnGUI();
    }
}
