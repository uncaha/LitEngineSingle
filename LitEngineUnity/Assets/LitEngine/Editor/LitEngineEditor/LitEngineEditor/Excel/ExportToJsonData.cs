using System;
using System.IO;

namespace ExportTool
{
    public class ExportToJsonData
    {
        string filename;
        string tempFile;
        ExcelData data;

        public ExportToJsonData(string pFullPath, ExcelData pData)
        {
            filename = pFullPath;
            data = pData;

            tempFile = filename + ".temp";
        }

        public void StartExport()
        {
            try
            {
                FileStream tfile = File.OpenWrite(tempFile);
                StreamWriter twt = new StreamWriter(tfile);
                twt.Write("{");
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


                    if (i > ExcelData.sStartLine)
                    {
                        twt.Write(",");
                    }

                    twt.Write($"\"{data.objects[i, data.startC]}\":{{");
                    for (int j = data.startC + 1; j < data.c; j++)
                    {
                        if (!data.IsNeed(j)) continue;
                        if (j > 1)
                        {
                            twt.Write(",");
                        }
                        System.Exception terro = WriteData(twt, data.objects[ExcelData.sTypeLine, j], data.objects[ExcelData.sFieldNameLine, j],data.objects[i, j]);
                        if (terro != null)
                        {
                            twt.Flush();
                            twt.Close();
                            tfile.Close();
                            File.Delete(tempFile);
                            if (UnityEditor.EditorUtility.DisplayDialog("Error", $"表 {filename} 生成配置出现错误第{i}行,第{j}列.erro = {terro.ToString()}", "ok"))
                            {
                                DLog.LogError($"表 {filename} 生成配置出现错误第{i}行,第{j}列.erro = {terro.ToString()}");
                            }
                            return;
                        }
                    }
                    twt.Write("}");
                }
                twt.Write("}");
                twt.Flush();
                twt.Close();
                tfile.Close();
                if (File.Exists(filename))
                    File.Delete(filename);
                File.Move(tempFile, filename);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }

        public System.Exception WriteData(StreamWriter _write,string _typestr, string pName, string _value)
        {
            _write.Write($"\"{pName}\":");
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


        protected void WriteArray(StreamWriter _write, string _typestr, string _value)
        {
            if (!string.IsNullOrEmpty(_value))
            {
                _write.Write("[");
                string[] tarry = _value.Split(',');
                string tctype = _typestr.Replace("[]", "");
                for (int i = 0; i < tarry.Length; i++)
                {
                    WriteValue(_write, tctype, tarry[i]);
                    if(i < tarry.Length - 1)
                    {
                        _write.Write(",");
                    }
                }

                _write.Write("]");

            }
        }
        protected void WriteValue(StreamWriter _write, string _typestr, string _value)
        {
            switch (_typestr)
            {
                case "string":
                    _write.Write($"\"{_value}\"");
                    break;
                default:
                    _write.Write(_value);
                    break;
            }
        }
    }
}
