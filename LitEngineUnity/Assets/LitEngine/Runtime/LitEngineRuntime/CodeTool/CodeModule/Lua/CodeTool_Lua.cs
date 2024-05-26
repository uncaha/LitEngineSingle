//using LuaInterface;
//using LitEngine.Method;
//using LitEngine.UpdateSpace;
//namespace LitEngine.CodeTool
//{
//    public class CodeTool_Lua : CodeToolBase
//    {
//        public LuaState LuaRoot { get; private set; }
//        UpdateObject updateObject;
//        public CodeTool_Lua() : base()
//        {
//            new LuaResLoader();
//            LuaRoot = new LuaState();
//            LuaRoot.LogGC = true;
//            LuaRoot.Start();
//            LuaBinder.Bind(LuaRoot);

//            updateObject = new UpdateObject("codeTool_Lua", new Method_Action(Update), this);
//            GameUpdateManager.RegUpdate(updateObject);
//        }
//        override protected void DisposeNoGcCode()
//        {
//            updateObject.UnRegToOwner();
//            LuaRoot.Dispose();
//            LuaRoot = null;
//        }

//        private void Update()
//        {
//            if (LuaRoot != null)
//            {
//                LuaRoot.Collect();
//                LuaRoot.CheckTop();
//            }
//        }
//        #region 类型判断
//        override public IBaseType GetLType(string _name)
//        {
//            return new LuaType(_name);
//        }

//        override public IBaseType GetObjectType(object _obj)
//        {
//            return null;
//        }
//        #endregion
//        #region 方法
//        override public LitEngine.Method.MethodBase GetLMethod(IBaseType _type, object pTar, string _funname, int _pamcount)
//        {
//            LuaTable ttable = (LuaTable)pTar;
//            LuaFunction tfun = ttable.GetLuaFunction(_funname);
//            return tfun != null ? new Method_Lua(pTar, tfun) : null;
//        }
//        #endregion
//        #region 对象获取
//        override public object GetObject(string _classname, params object[] _parmas)
//        {
//            if (_classname == null || _classname.Length == 0) return null;
//            string[] tarrys = _classname.Split(',');
//            if (tarrys.Length < 3) return null;
//            LuaRoot.Require(tarrys[0]);
//            LuaTable ret = LuaRoot.GetTable(tarrys[1]);
//            if (ret == null) return null;
//            LuaFunction objnew = ret.GetLuaFunction(tarrys[2]);
//            if (objnew == null) return null;
//            objnew.BeginPCall();
//            objnew.PushArgs(_parmas);
//            objnew.PCall();
//            ret = objnew.CheckLuaTable();
//            objnew.EndPCall();
//            return ret;
//        }

//        #endregion

//        override public UpdateBase GetUpdateObjectAction(string _Function, string _classname, object _target)
//        {
//            MethodBase tmethod = GetLMethod(null, _target, _classname, 0);
//            if (tmethod == null) return null;
//            return new UpdateObject(string.Format("{0}->{1}", _classname, _Function), tmethod, _target);
//        }
//    }
//}