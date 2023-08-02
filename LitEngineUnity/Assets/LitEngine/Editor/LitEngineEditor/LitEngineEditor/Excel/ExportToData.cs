using System;
using System.Globalization;
using System.IO;

namespace ExportTool
{
    public class ExportToData
    {
        string filename;
        string tempFile;
        ExcelData data;

        public ExportToData(string pName, ExcelData pData)
        {
            filename = pName;
            data = pData;

            tempFile = filename + ".temp";
        }

        public void StartExport()
        {
            try
            {
                DLog.Log("开始导出:" + tempFile);
                FileStream tfile = File.OpenWrite(tempFile);
                BinaryWriter twt = new BinaryWriter(tfile);
                int tline = data.r - ExcelData.sStartLine;
                twt.Write(tline);
                DLog.LogFormat("共有 {0} 行数据",tline);
                int twriteLiine = 0;
                for (int i = ExcelData.sStartLine; i < data.r; i++)
                {
                    var tfirst0 = data.objects[i, 0];
                    if (string.IsNullOrEmpty(tfirst0))
                    {
                        DLog.LogFormat($"导出截至到第{i}行。filename = {filename}");
                        break;
                    }

                    if (tfirst0.StartsWith("#"))
                    {
                        continue;
                    }


                    var tfirst = data.objects[i, data.startC];

                    if (string.IsNullOrEmpty(tfirst))
                    {
                        DLog.LogFormat($"导出截至到第{i}行。filename = {filename}");
                        break;
                    }

                    bool isHaveC = false;
                    for (int j = data.startC; j < data.c; j++)
                    {
                        if (!data.IsNeed(j)) continue;
                        isHaveC = true;
                        System.Exception terro = WriteData(twt, data.objects[ExcelData.sTypeLine, j], data.objects[i, j]);
                        if (terro != null)
                        {
                            twt.Flush();
                            twt.Close();
                            tfile.Close();
                            File.Delete(tempFile);
                            ShowError(i,j,terro.Message);
                            return;
                        }
                    }
                    if (isHaveC)
                    {
                        twriteLiine++;
                    }
                    
                }

                twt.Seek(0, SeekOrigin.Begin);
                twt.Write(twriteLiine);
                twt.Seek(0, SeekOrigin.End);

                twt.Flush();
                twt.Close();
                tfile.Close();
                if (File.Exists(filename))
                    File.Delete(filename);
                File.Move(tempFile, filename);

                DLog.Log($"导出结束。共 {twriteLiine} 行数据");
            }
            catch (Exception ex)
            {
                DLog.LogError(ex);
            }

        }

        void ShowError(int i,int j,string pErro)
        {
            if (UnityEditor.EditorUtility.DisplayDialog("Error", $"表 {filename} 生成配置出现错误第{i}行,第{j}列.erro = {pErro}", "ok"))
            {
                DLog.LogError($"表 {filename} 生成配置出现错误第{i}行,第{j}列.erro = {pErro}");
            }
        }

        public System.Exception WriteData(BinaryWriter _write, string _typestr, string _value)
        {
            try
            {
                if (_typestr.Contains("[]"))
                    WriteArray(_write, _typestr, _value);
                else
                    WriteValue(_write, _typestr, _value);
            }
            catch (System.Exception _erro)
            {
                return _erro;
            }

            return null;
        }


        protected void WriteArray(BinaryWriter _write, string _typestr, string _value)
        {
            if (!string.IsNullOrEmpty(_value))
            {
                string[] tarry = _value.Split('|');
                _write.Write(tarry.Length);

                string tctype = _typestr.Replace("[]", "");
                for (int i = 0; i < tarry.Length; i++)
                    WriteData(_write, tctype, tarry[i]);
            }
            else
            {
                _write.Write(0);
            }
        }
        protected void WriteValue(BinaryWriter _write, string _typestr, string _value)
        {
            if (string.IsNullOrEmpty(_value))
            {
                WriteNullValue(_write, _typestr, _value);
                return;
            }

            switch (_typestr)
            {
                case "int":
                    _write.Write(int.Parse(_value, CultureInfo.InvariantCulture));
                    break;
                case "float":
                    _write.Write(float.Parse(_value, CultureInfo.InvariantCulture));
                    break;
                case "string":
                    _write.Write(_value);
                    break;
                case "long":
                    _write.Write(long.Parse(_value, CultureInfo.InvariantCulture));
                    break;
                case "byte":
                    _write.Write(byte.Parse(_value, CultureInfo.InvariantCulture));
                    break;
                case "short":
                    _write.Write(short.Parse(_value, CultureInfo.InvariantCulture));
                    break;
                case "bool":
                    _write.Write(bool.Parse(_value));
                    break;
            }
        }

        protected void WriteNullValue(BinaryWriter _write, string _typestr, string _value)
        {
            switch (_typestr)
            {
                case "int":
                    _write.Write(default(int));
                    break;
                case "float":
                    _write.Write(default(float));
                    break;
                case "string":
                    _write.Write("");
                    break;
                case "long":
                    _write.Write(default(long));
                    break;
                case "byte":
                    _write.Write(default(byte));
                    break;
                case "short":
                    _write.Write(default(short));
                    break;
                case "bool":
                    _write.Write(false);
                    break;
            }
        }
    }
}
