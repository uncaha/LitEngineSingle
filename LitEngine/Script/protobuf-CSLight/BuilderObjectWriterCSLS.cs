using System;
using System.Collections;
using System.Collections.Generic;
using ILRuntime.CLR.TypeSystem;
namespace LitEngine
{
    namespace ProtoCSLS
    {
        #region Builder
        public class ProtoBufferWriterBuilderCSLS
        {
            private CodeToolBase mCodeTool;
            //CLRSharp_Instance
            private ProtoBufferWriterCSLS mWriter;
            protected object mObject;
            protected IType mType;
            protected BuilderObjectWriterBaseCSLS mBuilder;
            public ProtoBufferWriterBuilderCSLS(CodeToolBase _codetool, object _object)
            {
                mCodeTool = _codetool;
                mObject = _object;
                mType = mCodeTool.GetObjectType(_object);
                mWriter = new ProtoBufferWriterCSLS();
                BuildMember();
            }
            public byte[] GetBuffer()
            {
                if (mWriter.Length == 0) return null;
                byte[] ret = new byte[mWriter.Length];
                Array.Copy(mWriter.Data, 0, ret, 0, mWriter.Length);
                return ret;
            }

           
            private void BuildMember()
            {
                if (mObject == null) return;
                IType[] ttypes = mCodeTool.GetFieldTypes(mType);
                for(int i = 0;i<ttypes.Length;i++)
                {
                    object tfield = mCodeTool.GetMemberByIndex(mType, i, mObject);
                    BuilderObjectWriterBaseCSLS.BuildChild(mCodeTool, tfield, mWriter);
                }
            }
        }
        #endregion
        #region builder object
        #region base object
        public class BuilderObjectWriterBaseCSLS
        {
            protected CodeToolBase mCodeTool;
            protected object mObject;
            public BuilderObjectWriterBaseCSLS(CodeToolBase _codetool, object _object)
            {
                mCodeTool = _codetool;
                mObject = _object;
            }
            virtual public void WriteMember(ProtoBufferWriterCSLS _writer)
            {

            }

            public static void BuildChild(CodeToolBase _codetool, object _fieldobj, ProtoBufferWriterCSLS _writer)
            {
                if (_fieldobj == null)
                {
                    _writer.FieldNumberForward();
                    return;
                }
                BuilderObjectWriterBaseCSLS tobj = null;
                if (_codetool.IsLSType(_fieldobj.GetType()))
                {
                    tobj = new BuilderObjectWriterObjectCSLS(_codetool,_fieldobj);         
                }
                else
                {
                    Type ttype = _fieldobj.GetType();
                    if (ttype.ToString().Contains("List"))
                        tobj = new BuilderObjectWriterArrayCSLS(_codetool,_fieldobj);
                    else
                        tobj = new BuilderObjectWriterDefaultCSLS(_codetool,_fieldobj);
                }
                tobj.WriteMember(_writer);
            }
        }
        #endregion
        #region CSLight-object
        public class BuilderObjectWriterObjectCSLS : BuilderObjectWriterBaseCSLS
        {
            protected IType mType;
            public BuilderObjectWriterObjectCSLS(CodeToolBase _codetool, object _object) : base(_codetool, _object)
            {
                mType = mCodeTool.GetObjectType(mObject);
            }
            override public void WriteMember(ProtoBufferWriterCSLS _writer)
            {
                if (mObject == null)
                {
                    _writer.FieldNumberForward();
                    return;
                }
                ProtoBufferWriterCSLS twriterchild = new ProtoBufferWriterCSLS();
                IType[] ttypes = mCodeTool.GetFieldTypes(mType);
                for (int i = 0; i < ttypes.Length; i++)
                {
                    object tfield = mCodeTool.GetMemberByIndex(mType, i, mObject);
                    
                    BuildChild(mCodeTool,tfield, twriterchild);
                }

                if (twriterchild.Length > 0)
                {
                    _writer.WriteFieldHeaderAddFieldNumber(WireType.String);
                    _writer.WriteBytes(twriterchild.Data, 0, twriterchild.Length);
                }
                else
                    _writer.FieldNumberForward();
            }
        }
        #endregion
        #region Default-object
        public class BuilderObjectWriterDefaultCSLS : BuilderObjectWriterBaseCSLS
        {
            public BuilderObjectWriterDefaultCSLS(CodeToolBase _codetool, object _object) : base(_codetool, _object)
            {
            }
            override public void WriteMember(ProtoBufferWriterCSLS _writer)
            {
                _writer.Write(mObject);
            }
        }
        #endregion
        #region Array-object
        public class BuilderObjectWriterArrayCSLS : BuilderObjectWriterBaseCSLS
        {
            protected ArrayList mList;
            public BuilderObjectWriterArrayCSLS(CodeToolBase _codetool, object _object) : base(_codetool,_object)
            {
                if (mObject != null)
                    mList = new ArrayList((ICollection)mObject);
            }
            override public void WriteMember(ProtoBufferWriterCSLS _writer)
            {
                if (mList == null || mList.Count == 0)
                {
                    _writer.FieldNumberForward();
                    return;
                }
                foreach (object obj in mList)
                {
                    BuildChild(mCodeTool,obj, _writer);
                    _writer.FieldNumberBack();
                }
                _writer.FieldNumberForward();
            }
        }
        #endregion
        #endregion
    }
}

