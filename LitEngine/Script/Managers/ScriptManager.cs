using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using System.Reflection;
using ILRuntime.CLR.TypeSystem;
namespace LitEngine
{
    using IO;
    public enum UseScriptType
    {
        UseScriptType_System = 1,
        UseScriptType_LS = 2,
        UseScriptType_Unity = 3,
    }
    public class ScriptManager: ManagerInterface
    {
#if NOILRUNTIME
        public ILRuntime.Runtime.Enviorment.AppDomain Env
        {
            get;
            private set;
        }
#endif
        private UseScriptType mUseSystemAssm = UseScriptType.UseScriptType_LS;
        private CodeToolBase mCodeTool;
        public CodeToolBase CodeTool
        {
            get
            {
                if (mCodeTool == null)
                    DLog.LogError( "脚本系统还未载入任何脚本.请执行 LoadProject 系列方法.");
                return mCodeTool;
            }
        }
        public ScriptManager(UseScriptType _stype)
        {
#if !L2CPP
            mUseSystemAssm = _stype;
#else
            mUseSystemAssm = UseScriptType.UseScriptType_LS;
#endif
            switch (mUseSystemAssm)
            {
                case UseScriptType.UseScriptType_LS:
                    #if NOILRUNTIME
                    Env = new ILRuntime.Runtime.Enviorment.AppDomain();
                    mCodeTool = new CodeTool_LS(Env);
                    #endif
                    break;
                case UseScriptType.UseScriptType_Unity:
                case UseScriptType.UseScriptType_System:
                    mCodeTool = new CodeTool_SYS();
                    break;
            }
        }
#region 释放
        bool mDisposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool _disposing)
        {
            if (mDisposed)
                return;

            if (_disposing)
                DisposeNoGcCode();

            mDisposed = true;
        }

        virtual protected void DisposeNoGcCode()
        {
            mCodeTool.Dispose();
        }

        ~ScriptManager()
        {
            Dispose(false);
        }
#endregion


        public bool ProjectLoaded
        {
            get;
            private set;
        }



        public void LoadScriptFile(string _filename)
        {
            
            if(UseScriptType.UseScriptType_Unity == mUseSystemAssm)
            {
                Assembly[] tasss = System.AppDomain.CurrentDomain.GetAssemblies();

                Assembly tunityscrp = null;
                for(int i = 0;i<tasss.Length;i++)
                {
                    if (!tasss[i].GetName().Name.Equals("Assembly-CSharp")) continue;
                    tunityscrp = tasss[i];
                    break;
                }
                ((CodeTool_SYS)CodeTool).AddAssembly(tunityscrp);
            }
            else
            {

                if (string.IsNullOrEmpty(_filename)) return;
                byte[] dllbytes = LoaderManager.LoadScriptFile(_filename + ".dll");
                byte[] pdbbytes = LoaderManager.LoadScriptFile(_filename + ".pdb");

                if (dllbytes == null || pdbbytes == null)
                {
                    DLog.LogErrorFormat("LoadScriptFormFile{dllbytes = {0},pdbbytes = {1}}", dllbytes, pdbbytes);
                    return;
                }

                AESReader tdllreader = new AESReader(dllbytes);
                AESReader tpdbreader = new AESReader(pdbbytes);

                dllbytes = tdllreader.ReadAllBytes();
                pdbbytes = tpdbreader.ReadAllBytes();

                tdllreader.Dispose();
                tpdbreader.Dispose();

                LoadProjectByBytes(dllbytes, pdbbytes);
            }
            ProjectLoaded = true;
        }

        public void LoadProjectByBytes(byte[] _dll, byte[] _pdb)
        {
            try
            {
                switch (mUseSystemAssm)
                {
                    case UseScriptType.UseScriptType_LS:
                        {
                            #if NOILRUNTIME
                            System.IO.MemoryStream msDll = new System.IO.MemoryStream(_dll);
                            System.IO.MemoryStream msPdb = new System.IO.MemoryStream(_pdb);
                            Env.LoadAssembly(msDll, msPdb, new Mono.Cecil.Pdb.PdbReaderProvider());
                            #endif
                        }
                        break;
                    case UseScriptType.UseScriptType_System:
                        {
                            ((CodeTool_SYS)CodeTool).AddAssemblyType(_dll, _pdb);
                        }
                        break;
                }
                ProjectLoaded = true;
            }
            catch (Exception err)
            {
                DLog.LogError("加载脚本出现错误:" + err);
            }


        }
    }
    /*
    public void LoadProject(string _PathFile)
    {
        try
        {
            var dll = System.IO.File.ReadAllBytes(_PathFile + ".dll");
            var pdb = System.IO.File.ReadAllBytes(_PathFile + ".pdb");
            LoadProjectByBytes(dll, pdb);
        }
        catch (Exception err)
        {
            DLog.LogError("LoadProject" + err);
        }


    }

    public void LoadProjectFromPacket(string _PathFile)
    {
        try
        {
            using (AESReader treader = new AESReader(_PathFile))
            {

                int len = treader.ReadInt32();
                byte[] tbuffer = treader.ReadBytes(len);
                byte[] tdllbyts = null;
                byte[] tpdbbyts = null;
                Stream tstream = new MemoryStream(tbuffer);
#region build ProjectList

                using (ZipInputStream f = new ZipInputStream(tstream))
                {

                    while (true)
                    {
                        ZipEntry zp = f.GetNextEntry();
                        if (zp == null) break;
                        if (!zp.IsDirectory && zp.Crc != 00000000L)
                        {

                            byte[] b = new byte[f.Length];
                            int treadlen = f.Read(b, 0, b.Length);

                            //取得文件所有数据
                            if (zp.Name.Contains(".dll"))
                                tdllbyts = b;
                            else if (zp.Name.Contains(".pdb"))
                                tpdbbyts = b;
                        }
                    }
                    f.Close();
                }
#endregion
                tstream.Close();
                LoadProjectByBytes(tdllbyts, tpdbbyts);
            }
        }
        catch (Exception err)
        {
            DLog.LogError("loadpacket" + err);
        }

    }
    */
}

