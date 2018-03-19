/*

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using LitEngine;
public class CreatCLRBindingFile
{
    [UnityEditor.MenuItem("GenerateBinding/AutoBinding")]
    public static void CreatAutoGen()
    {
  
        UnityEditor.AssetDatabase.Refresh();
        DLog.Log("开始导出CLR绑定类.");
        string _output = Application.dataPath + "\\AutoGenerate";
        List<Type> _types = new List<Type>();

        _types.Add(typeof(int));
        _types.Add(typeof(float));
        _types.Add(typeof(long));
        _types.Add(typeof(object));
        _types.Add(typeof(string));
        _types.Add(typeof(byte));

        _types.Add(typeof(System.Math));
        _types.Add(typeof(System.IO.File));
        _types.Add(typeof(System.IO.FileStream));
        _types.Add(typeof(System.IO.StringReader));
        _types.Add(typeof(System.IO.StringWriter));
        _types.Add(typeof(System.IO.StreamReader));
        _types.Add(typeof(System.IO.StreamWriter));
        _types.Add(typeof(System.IO.MemoryStream));
        _types.Add(typeof(ValueType));
        _types.Add(typeof(Array));
        _types.Add(typeof(List<int>));
        _types.Add(typeof(List<float>));
        _types.Add(typeof(List<string>));
        _types.Add(typeof(List<UnityEngine.Object>));
        _types.Add(typeof(List<UnityEngine.GameObject>));
        _types.Add(typeof(List<UnityEngine.Transform>));
        _types.Add(typeof(List<ILRuntime.Runtime.Intepreter.ILTypeInstance>));

        _types.Add(typeof(LinkedList<int>));
        _types.Add(typeof(LinkedList<float>));
        _types.Add(typeof(LinkedList<string>));
        _types.Add(typeof(LinkedList<System.Object>));
        _types.Add(typeof(LinkedList<ILRuntime.Runtime.Intepreter.ILTypeInstance>));

        _types.Add(typeof(Dictionary<string, ILRuntime.Runtime.Intepreter.ILTypeInstance>));
        _types.Add(typeof(Dictionary<int, ILRuntime.Runtime.Intepreter.ILTypeInstance>));
        _types.Add(typeof(Dictionary<ILRuntime.Runtime.Intepreter.ILTypeInstance, ILRuntime.Runtime.Intepreter.ILTypeInstance>));

        _types.Add(typeof(GameObject));
        _types.Add(typeof(Transform));
        _types.Add(typeof(UnityEngine.Debug));
        _types.Add(typeof(UnityEngine.Component));
        _types.Add(typeof(UnityEngine.Object));
        _types.Add(typeof(UnityEngine.Animation));
        _types.Add(typeof(UnityEngine.Animator));
        _types.Add(typeof(UnityEngine.AudioClip));
        _types.Add(typeof(UnityEngine.Time));
        _types.Add(typeof(UnityEngine.Mathf));
        _types.Add(typeof(UnityEngine.Vector2));
        _types.Add(typeof(UnityEngine.Vector3));
        _types.Add(typeof(UnityEngine.Vector4));
        _types.Add(typeof(UnityEngine.Quaternion));
        _types.Add(typeof(UnityEngine.SceneManagement.Scene));
        _types.Add(typeof(UnityEngine.SceneManagement.SceneManager));

        _types.Add(typeof(LitEngine.ScriptInterface.UIInterface));
        _types.Add(typeof(LitEngine.ScriptInterface.BehaviourInterfaceBase));
        _types.Add(typeof(LitEngine.ScriptInterface.ScriptInterfaceApplication));
        _types.Add(typeof(LitEngine.ScriptInterface.ScriptInterfaceMouse));
        _types.Add(typeof(LitEngine.ScriptInterface.ScriptInterfaceTrigger));
        _types.Add(typeof(LitEngine.ScriptInterface.ScriptInterfaceCollision));

        _types.Add(typeof(AppCore));
        _types.Add(typeof(GameCore));

        _types.Add(typeof(LitEngine.PublicUpdateManager));
        _types.Add(typeof(GameUpdateManager));
        _types.Add(typeof(LitEngine.ScriptManager));
        _types.Add(typeof(LitEngine.ScriptTool));
        _types.Add(typeof(LitEngine.CodeTool_LS));
        _types.Add(typeof(LitEngine.DownLoad.DownLoadTask));
        _types.Add(typeof(LitEngine.Loader.LoaderManager));

        _types.Add(typeof(LitEngine.IO.AesStreamBase));
        _types.Add(typeof(LitEngine.IO.AESReader));
        _types.Add(typeof(LitEngine.IO.AESWriter));

        _types.Add(typeof(LitEngine.NetTool.TCPNet));
        _types.Add(typeof(LitEngine.NetTool.SendData));
        _types.Add(typeof(LitEngine.NetTool.ReceiveData));
        _types.Add(typeof(LitEngine.NetTool.MSG_RECALL_DATA));
        _types.Add(typeof(LitEngine.NetTool.BufferBase));
        _types.Add(typeof(LitEngine.NetTool.HttpNet));
        _types.Add(typeof(LitEngine.NetTool.HttpData));

        _types.Add(typeof(LitEngine.SafeList<ILRuntime.Runtime.Intepreter.ILTypeInstance>));
        _types.Add(typeof(LitEngine.SafeMap<string, ILRuntime.Runtime.Intepreter.ILTypeInstance>));
        _types.Add(typeof(LitEngine.SafeMap<int, ILRuntime.Runtime.Intepreter.ILTypeInstance>));
        _types.Add(typeof(LitEngine.SafeMap<ILRuntime.Runtime.Intepreter.ILTypeInstance, ILRuntime.Runtime.Intepreter.ILTypeInstance>));
        _types.Add(typeof(LitEngine.SafeQueue<ILRuntime.Runtime.Intepreter.ILTypeInstance>));
        _types.Add(typeof(LitEngine.ScriptInterface.BehaviourInterfaceBase));
        _types.Add(typeof(LitEngine.UnZip.UnZipTask));
        _types.Add(typeof(LitEngine.UpdateSpace.UpdateILObject));
        _types.Add(typeof(LitEngine.UpdateSpace.UpdateObject));
        _types.Add(typeof(LitEngine.UpdateSpace.UpdateObjectVector));
        _types.Add(typeof(LitEngine.UpdateSpace.UpdateNeedDisObject));
        _types.Add(typeof(LitEngine.UpdateSpace.UpdateGroup));
        _types.Add(typeof(LitEngine.XmlLoad.SmallXmlParser));
        _types.Add(typeof(DLog));


        _types.Add(typeof(Google.Protobuf.ByteString));
        _types.Add(typeof(Google.Protobuf.CodedInputStream));
        _types.Add(typeof(Google.Protobuf.CodedOutputStream));
        _types.Add(typeof(Google.Protobuf.WireFormat));

        ILRuntime.Runtime.CLRBinding.BindingCodeGenerator.GenerateBindingCode(_types, _output);

        DLog.Log("导出完成.");

        UnityEditor.AssetDatabase.Refresh();
    }

}


*/