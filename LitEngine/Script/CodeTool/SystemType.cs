using System;
using System.Collections.Generic;
#if NOILRUNTIME
using ILRuntime.CLR.Method;
#endif
using System.Reflection;
namespace ILRuntime.CLR.TypeSystem
{
#if NOILRUNTIME

#else
    public interface IType
    {
        bool IsGenericInstance { get; }
        KeyValuePair<string, IType>[] GenericArguments { get; }
        Type TypeForCLR { get; }
        Type ReflectionType { get; }

        IType BaseType { get; }

        IType[] Implements { get; }

        IType ByRefType { get; }

        IType ArrayType { get; }

        string FullName { get; }

        string Name { get; }

        bool IsArray { get; }

        int ArrayRank { get; }

        bool IsValueType { get; }

        bool IsDelegate { get; }

        bool IsPrimitive { get; }

        bool IsByRef { get; }

        bool IsInterface { get; }

        IType ElementType { get; }

        bool HasGenericParameter { get; }

        bool IsGenericParameter { get; }


        int GetFieldIndex(object token);


        bool CanAssignTo(IType type);

        IType MakeGenericInstance(KeyValuePair<string, IType>[] genericArguments);

        IType MakeByRefType();

        IType MakeArrayType(int rank);
        IType FindGenericArgument(string key);

        IType ResolveGenericType(IType contextType);
    }
#endif
    public class SystemType : IType
    {
        public SystemType(Type _clrtype)
        {
            clrType = _clrtype; 
        }
        Type clrType;
        IType[] mFieldTypes = null;
        public IType[] FieldTypes
        {
            get {
                if (mFieldTypes == null)
                {
                    FieldInfo[] tpis = clrType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
                    mFieldTypes = new IType[tpis.Length];
                    for (int i = 0; i < tpis.Length; i++)
                    {
                        mFieldTypes[i] = new SystemType(tpis[i].FieldType);
                    }
                }
                return mFieldTypes;
            }
        }
        public bool IsGenericInstance {
            get;
            set;
        }
        public KeyValuePair<string, IType>[] GenericArguments { get; set; }
        public Type TypeForCLR
        {
            get
            {
                return clrType;
            }
        }
        public Type ReflectionType { get; set; }
        public int ArrayRank { get; set; }
        public IType BaseType { get; set; }

        public IType[] Implements { get; set; }

        public IType ByRefType { get; set; }

        public IType ArrayType { get; set; }

        public string FullName { get; set; }

        public string Name { get; set; }

        public bool IsArray { get; set; }

        public bool IsValueType { get; set; }

        public bool IsDelegate { get; set; }

        public bool IsPrimitive { get; set; }

        public bool HasGenericParameter { get; set; }

        public bool IsByRef { get; set; }

        public bool IsInterface { get; set; }

        public IType ElementType { get; set; }

        public bool IsGenericParameter { get; set; }
#if NOILRUNTIME
        public ILRuntime.Runtime.Enviorment.AppDomain AppDomain { get; set; }
        public IMethod GetMethod(string name, List<IType> param, IType[] genericArguments, IType returnType = null, bool declaredOnly = false)
        {
            return null;
        }
        public IMethod GetVirtualMethod(IMethod method)
        {
            return null;
        }

        public List<IMethod> GetMethods()
        {
            return null;
        }
       public IMethod GetMethod(string name, int paramCount, bool declaredOnly = false)
        {
            return null;
        }
        public IMethod GetConstructor(List<IType> param)
        {
            return null;
        }


#endif

        public IType MakeArrayType(int rank)
        {
            return new SystemType(clrType.MakeArrayType());
        }

        public int GetFieldIndex(object token)
        {
            return 0;
        }

        public bool CanAssignTo(IType type)
        {
            return false;
        }

        public IType MakeGenericInstance(KeyValuePair<string, IType>[] genericArguments)
        {
            return null;
        }

        public IType MakeByRefType()
        {
            return null;
        }

        public IType MakeArrayType()
        {
            return new SystemType(clrType.MakeArrayType());
        }
        public IType FindGenericArgument(string key)
        {
            return null;
        }

        public IType ResolveGenericType(IType contextType)
        {
            return null;
        }
    }
}
