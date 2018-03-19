using System.Collections.Generic;
using System;
using ILRuntime.CLR.TypeSystem;
namespace LitEngine
{
    namespace ProtoCSLS
    {
        public class ProtoBufferReaderBuilderCSLS
        {
            static public object GetCSLEObject(CodeToolBase _codetool,byte[] _buffer, int _len, string _classname)
            {
                if (string.IsNullOrEmpty(_classname)) return null;
                ProtoBufferReaderBuilderCSLS tcslspro = new ProtoBufferReaderBuilderCSLS(_codetool,_buffer, _len, _classname);
                return tcslspro.Value;
            }

            public Dictionary<int, BuilderObjectBase> Members
            {
                get;
                protected set;
            }

            private string mClassName = "";

            private IType mScriptType;
            private object mScriptObject;

            protected CodeToolBase mCodeTool;

            private ProtobufferReaderCSLS mReader;
            public object Value
            {
                get
                {
                    return mScriptObject;
                }

            }

            public ProtoBufferReaderBuilderCSLS(CodeToolBase _codetool,byte[] _buffer, int _len, string _classname)
            {
                mCodeTool = _codetool;
                mReader = new ProtobufferReaderCSLS(_buffer, _len);
                mClassName = _classname;
                InitType();
                mScriptObject = mCodeTool.GetCSLEObjectParmasByType(mScriptType);
                BuildMember();
            }
            private void InitType()
            {
                mScriptType = mCodeTool.GetLType(mClassName);
            }

            public void AddMember(BuilderObjectBase _object)
            {
                if (_object == null) return;
                if (Members == null)
                    Members = new Dictionary<int, BuilderObjectBase>();
                if (!Members.ContainsKey(_object.FieldNumber))
                    Members.Add(_object.FieldNumber, _object);
                else
                    throw new InvalidOperationException("重复设置member 字段:" + _object.FieldNumber);
            }

            private void BuildMember()
            {
                IType[] ttypes = mCodeTool.GetFieldTypes(mScriptType);
                int tindex = 1;
                for (int i = 0;i< ttypes.Length;i++)
                {
                    BuilderObjectBase tobj = BuilderObjectBase.GetMember(mCodeTool,ttypes[i], i, tindex++, mScriptObject, mScriptType);
                    AddMember(tobj);
                }

                int tfieldnumber = mReader.ReadFieldHeader();
                while (tfieldnumber > 0)
                {
                    if (!Members.ContainsKey(tfieldnumber))
                    {
                        throw new InvalidOperationException("ProtoReaderMemberObject 未能从builder中找到对应的字段 fieldnumber:" + tfieldnumber);
                    }
                    BuilderObjectBase tprb = Members[tfieldnumber];
                    tprb.ReadMember(mReader);
                    tfieldnumber = mReader.ReadFieldHeader();
                }

            }

        }
    }
}



