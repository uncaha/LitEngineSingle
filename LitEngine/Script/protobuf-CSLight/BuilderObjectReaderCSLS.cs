using System;
using System.Collections.Generic;
using System.Reflection;
using ILRuntime.CLR.TypeSystem;
namespace LitEngine
{
    namespace ProtoCSLS
    {
        public class BuilderObjectBase
        {
            protected CodeToolBase mCodeTool;

            protected object mParentObject;
            protected IType mFunType;
            protected IType mParentType;

            protected int mFieldIndex;
            protected object mSelfObject;

            protected int mFieldNumber;
            public object Value
            {
                get
                {
                    return mSelfObject;
                }
                set
                {
                    mSelfObject = value;
                }
            }
            public Dictionary<int, BuilderObjectBase> Members
            {
                get;
                protected set;
            }
            public BuilderObjectBase BuildParent
            {
                get;
                set;
            }

            public int FieldNumber
            {
                get
                {
                    return mFieldNumber;
                }
            }
            public BuilderObjectBase(CodeToolBase _codetool,int _fieldnumber, int _fieldindex, object _parent, IType _type, IType _parenttype)
            {
                mFieldNumber = _fieldnumber;
                mFieldIndex = _fieldindex;
                mParentObject = _parent;
                mFunType = _type;
                mParentType = _parenttype;
                mCodeTool = _codetool;
            }

            virtual public void SetValueToCSLEObject()
            {
                if (Members == null) return;
                foreach (BuilderObjectBase tobj in Members.Values)
                {
                    tobj.SetValueToCSLEObject();
                }
            }
            virtual public void ReadMember(ProtobufferReaderCSLS _reader)
            {
            }
            virtual public void AddMember(BuilderObjectBase _object)
            {
                if (Members == null)
                    Members = new Dictionary<int, BuilderObjectBase>();
                if (!Members.ContainsKey(_object.FieldNumber))
                {
                    Members.Add(_object.FieldNumber, _object);
                    _object.BuildParent = this;
                }

                else
                    throw new InvalidOperationException("重复设置member 字段:" + _object.FieldNumber);

            }

            static public BuilderObjectBase GetMember(CodeToolBase _codetool,IType _fieldtype, int _fieldindex, int _index, object _parent, IType _parenttype)
            {
  
                if (_codetool.IsLSType(_fieldtype.TypeForCLR))
                {
                    return new BuilderObjectCELSClass(_codetool,_index, _fieldindex, _parent, _fieldtype, _parenttype);
                }
                else
                {
                    string ttypestring = _fieldtype.TypeForCLR.ToString();
                    if (ttypestring.Contains("List"))
                    {
                        IType tatyp = _codetool.GetListChildType(_fieldtype);//包含类型
                        if (tatyp == null)
                            DLog.LogError("获取数组基础类型失败(" + _fieldtype + "|" + ttypestring + "|" + tatyp + ")");
                        return new BuilderObjectCELSArray(_codetool, _index, _fieldindex, _parent, _fieldtype, _parenttype, tatyp);
                    }
                    else
                    {
                        return new BuilderObjectDefault(_codetool, _index, _fieldindex, _parent, _fieldtype, _parenttype);
                    }

                }
            }
        }

        public class BuilderObjectCELSClass : BuilderObjectBase
        {
            public BuilderObjectCELSClass(CodeToolBase _codetool, int _fieldnumber, int _fieldindex, object _parent, IType _type, IType _parenttype) : base(_codetool, _fieldnumber, _fieldindex, _parent, _type, _parenttype)
            {
                mSelfObject = mCodeTool.GetCSLEObjectParmasByType(mFunType);
                if (mSelfObject == null)
                    throw new InvalidOperationException("未能取得对象 " + ":" + mFunType.TypeForCLR.Name);
                if(mParentType != null && mParentObject != null)
                    mCodeTool.SetMember(mParentType, mFieldIndex, mSelfObject, mParentObject);

                IType[] ttypes = mCodeTool.GetFieldTypes(mFunType);
                int tindex = 1;
                for (int i = 0; i < ttypes.Length; i++)
                {
                    BuilderObjectBase tobj = BuilderObjectBase.GetMember(_codetool, ttypes[i], i, tindex++, mSelfObject, mFunType);
                    AddMember(tobj);
                }

            }

            override public void ReadMember(ProtobufferReaderCSLS _reader)
            {
                int bytes = (int)_reader.ReadUInt32Variant(false);
                byte[] tbytearry = _reader.ReadBytes(bytes);
                if (tbytearry != null)
                {
                    ProtobufferReaderCSLS treader = new ProtobufferReaderCSLS(tbytearry, bytes);
                    int tfieldnumber = treader.ReadFieldHeader();
                    while (tfieldnumber > 0)
                    {
                        if (!Members.ContainsKey(tfieldnumber))
                        {
                            throw new InvalidOperationException("ProtoReaderMemberObject 未能从builder中找到对应的字段 fieldnumber:" + tfieldnumber);
                        }
                        BuilderObjectBase tprb = Members[tfieldnumber];
                        tprb.ReadMember(treader);
                        tfieldnumber = treader.ReadFieldHeader();
                    }
                }
            }

        }

        public class BuilderObjectDefault : BuilderObjectBase
        {
            private Type mType;
            public BuilderObjectDefault(CodeToolBase _codetool, int _fieldnumber, int _fieldindex, object _parent, IType _type, IType _parenttype) : base(_codetool, _fieldnumber, _fieldindex, _parent, _type, _parenttype)
            {
                mType = _type.TypeForCLR;
            }
            override public void AddMember(BuilderObjectBase _object)
            {

            }
            override public void SetValueToCSLEObject()
            {
                // UnityEngine.Debug.Log(mSelfObject.ToString());
                if(mParentType != null && mParentObject != null)
                    mCodeTool.SetMember(mParentType, mFieldIndex, mSelfObject, mParentObject);
            }
            override public void ReadMember(ProtobufferReaderCSLS _reader)
            {
                Value = _reader.ReadByData(mType, _reader.WType);
                SetValueToCSLEObject();
            }
        }

        public class BuilderObjectCELSArray : BuilderObjectBase
        {
            private IType mChildType;
            private int mIndex = 0;
            public BuilderObjectCELSArray(CodeToolBase _codetool, int _fieldnumber, int _fieldindex, object _parent, IType _type, IType _parenttype, IType _ChildType) : base(_codetool, _fieldnumber, _fieldindex, _parent, _type, _parenttype)
            {
                mChildType = _ChildType;
                mSelfObject = mCodeTool.GetMemberByIndex(mParentType, mFieldIndex, _parent);

                if (mSelfObject == null)
                {
                    mSelfObject = Activator.CreateInstance(mFunType.TypeForCLR, true);
                    mCodeTool.SetMember(mParentType, mFieldIndex, mSelfObject, mParentObject);
                }
                if (mSelfObject == null)
                    throw new InvalidOperationException("数组类变量为空,初始化对象失败,可尝试在定义时直接初始化: " + mFieldIndex);

            }

            override public void ReadMember(ProtobufferReaderCSLS _reader)
            {
                BuilderObjectBase tchild = null;

                if (mCodeTool.IsLSType(mChildType.TypeForCLR))
                {
                    tchild = new BuilderObjectCELSClass(mCodeTool, mIndex, 0, null, mChildType, null);
                }
                else
                {
                    tchild = new BuilderObjectDefault(mCodeTool, mIndex, 0, null, mChildType, null);
                }
                AddMember(tchild);
                tchild.ReadMember(_reader);

                object tmet = mCodeTool.GetLMethod(mFunType, "Add",1);
                mCodeTool.CallMethod(tmet, mSelfObject, tchild.Value);
                mIndex++;
            }
        }
    }
}

