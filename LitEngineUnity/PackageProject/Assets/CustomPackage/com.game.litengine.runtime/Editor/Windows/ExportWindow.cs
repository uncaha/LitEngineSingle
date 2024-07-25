using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace LitEngineEditor
{
    public class ExportWindow : EditorWindow
    {
        #region Windows
        private static ExportWindow window = null;
        [UnityEditor.MenuItem("LitEngine/ExportTool")]
        static void Init()
        {
            if (window != null)
            {
                window.Close();
            }
            
            window = (ExportWindow)EditorWindow.GetWindow(typeof(ExportWindow));
            window.minSize = new Vector2(430, 450);
            window.maxSize = new Vector2(500, 500);
            window.name = "ExportTool";
                
            window.InitGUI();
            window.Show();
            window.Focus();
        }
        #endregion
        #region class
        #region field
        public bool NeedSaveSetting { get; set; }
        private int mToolbarOption = 0;
        private string[] mToolbarTexts = { "Assets","Excel", "ProtoToCS", "EnCryptTool" ,"MeshTool"};
        private Dictionary<ExportWType, ExportBase> mMap = new Dictionary<ExportWType, ExportBase>();
        #endregion

        public ExportWindow()
        {
            
        }

        void InitGUI()
        {
            ExportBase.RestConfig();
            AddWindow<ExportObject>();
            AddWindow<ExportExcelWiindow>();
            //AddWindow<ExportProtoTool>();
            AddWindow<EncryptTool>();
            AddWindow<MeshTool>();
        }

        protected void AddWindow<T>()where T : ExportBase,new()
        {
            T twd = new T();
            twd.mWindow = this;
            mMap.Add(twd.ExWType, twd);
        }

        void UpdateGUI()
        {
            if (mMap.ContainsKey((ExportWType)mToolbarOption))
                mMap[(ExportWType)mToolbarOption].OnGUI();
        }

        void RestGUI()
        {
            mMap.Clear();
            InitGUI();
        }
        
        void OnGUI()
        {
            NeedSaveSetting = false;
            if (GUILayout.Button("Rest Config"))
            {
                RestGUI();
            }
            mToolbarOption = GUILayout.Toolbar(mToolbarOption, mToolbarTexts);
  
            UpdateGUI();

            if(NeedSaveSetting)
                ExportSetting.SaveCFG();
        }
        #endregion


    }
}

