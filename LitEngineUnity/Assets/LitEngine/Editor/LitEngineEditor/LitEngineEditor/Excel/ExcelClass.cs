using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Aspose.Cells;
using System.Diagnostics;

namespace ExportTool
{
    public class ExcelData
    {
        public const int sStartLine = 4;
        public const int sCSLine = 0;
        public const int sContext = 1;
        public const int sTypeLine = 2;
        public const int sFieldNameLine = 3;
        public const string sNeedType = "c";

        public int startC { get; private set; }
        public string name { get; private set; }
        public int c { get; private set; }
        public int r { get; private set; }
        public string[,] objects { get; private set; }
        private bool inited = false;

        public bool IsNeed(int col)
        {
            string tcs = objects[sCSLine, col]?.ToLowerInvariant().Trim();
            if (string.IsNullOrEmpty(tcs)) return false;

            return tcs.StartsWith(sNeedType) || tcs.Equals("sc");
        }
        public void ReadExcelToArray(Worksheet pSheet)
        {
            if (inited) return;
            name = pSheet.Name;
            var tcells = pSheet.Cells;

            c = tcells.MaxDataColumn + 1;
            r = tcells.MaxDataRow + 1;
            objects = new string[r, c];
            int i = 0;
            for (; i < r; i++)
            {

                for (int j = 0; j < c; j++)
                {
                    var cur = tcells[i, j];
                    objects[i, j] = cur.StringValue == null ? "" : cur.StringValue.Trim();
                }

            }

            inited = true;
            r = i;

            for (int j = 0; j < c; j++)
            {
                var cur = tcells[0, j]?.StringValue.ToLowerInvariant().Trim();
                if(cur.StartsWith(sNeedType))
                {
                    startC = j;
                    UnityEngine.Debug.Log($"初始化 {name} 数据。第{startC}列开始。");
                    break;
                }
            }
        }
    }
    class ExcelClass
    {

        private string fileName = null;
        private string savepath;
        
        private Workbook mWorkbook;

        
        public ExcelClass(string _filename, string _savepath)
        {
            fileName = _filename;
            savepath = _savepath;
            if (!File.Exists(fileName)) return;
            try
            {
                if (fileName.EndsWith(".xls") || fileName.EndsWith(".xlsx")) 
                    mWorkbook = new Workbook(fileName);
                else
                    DLog.LogError("只支持xls 和 xlsx格式。");
            }
            catch (Exception ex)
            {
                DLog.LogError("Exception: " + ex.Message);
            }
        }
        

        public ExcelData GetContentHaveValue(Worksheet pSheet)
        {
            ExcelData ret = new ExcelData();
            if (pSheet != null)
            {
                ret.ReadExcelToArray(pSheet);
            }
            return ret;
        }

        public void Close()
        {

        }

        public void SaveToJson()
        {
            if (mWorkbook == null) return;

            foreach (var curSheet in mWorkbook.Worksheets)
            {
                if(curSheet.Name.StartsWith("#")) continue;
                
                ExcelData tdata = GetContentHaveValue(curSheet);
                string tfullname = savepath + "/" + curSheet.Name + ".json";

                ExportToJsonData tExp = new ExportToJsonData(tfullname, tdata);
                tExp.StartExport();
            }
        }

        public void SaveFile()
        {
            if (mWorkbook == null) return;
            foreach (var curSheet in mWorkbook.Worksheets)
            {
                if(curSheet.Name.StartsWith("#")) continue;
                
                ExcelData tdata = GetContentHaveValue(curSheet);
                string tfullname = savepath + "/" + curSheet.Name + ".bytes";

                ExportToData tExp = new ExportToData(tfullname, tdata);
                tExp.StartExport();
            }
        }


        public void ExoprtCfg()
        {
            if (mWorkbook == null) return;
            foreach (var curSheet in mWorkbook.Worksheets)
            {
                if(curSheet.Name.StartsWith("#")) continue;
                
                Console.WriteLine("Export " + curSheet.Name);
                ExcelData tdata = GetContentHaveValue(curSheet);
                string tfullname = savepath + "/" + curSheet.Name + ".bytes";

                ExportToData tExp = new ExportToData(tfullname, tdata);
                tExp.StartExport();

            }
            Console.WriteLine("Complete.");
        }

        public List<string> ExportReadClass()
        {
            if (mWorkbook == null) return null;
            var ret = new List<string>();
            foreach (var curSheet in mWorkbook.Worksheets)
            {
                if(curSheet.Name.StartsWith("#")) continue;
                
                ExcelData tdata = GetContentHaveValue(curSheet);
                string tfullname = savepath + "/" + curSheet.Name + ".cs";

                ExportToCS tcs = new ExportToCS(curSheet.Name, tfullname, tdata);
                bool isSuccess = tcs.StartExport();

                if (isSuccess)
                {
                    ret.Add(curSheet.Name);
                }
               
            }

            return ret;
        }


    }

}
