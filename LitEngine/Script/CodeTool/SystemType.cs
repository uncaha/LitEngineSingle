using System;
using System.Collections.Generic;
using ILRuntime.CLR.Method;
using System.Reflection;
namespace ILRuntime.CLR.TypeSystem
{
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

        public ILRuntime.Runtime.Enviorment.AppDomain AppDomain { get; set; }

        /// <summary>
        /// Get a specified Method in this type
        /// </summary>
        /// <param name="name">Name of the Type</param>
        /// <param name="paramCount">Parameter count</param>
        /// <param name="declaredOnly">True to search the methods decleared in this type only, false to search base types.</param>
        /// <returns></returns>
        public IMethod GetMethod(string name, int paramCount, bool declaredOnly = false)
        {
            return null;
        }


        public IType MakeArrayType(int rank)
        {
            return new SystemType(clrType.MakeArrayType());
        }

        /// <summary>
        ///  Get a specified Method in this type
        /// </summary>
        /// <param name="name">Name of the Type</param>
        /// <param name="param">List of parameter's types</param>
        /// <param name="genericArguments">List of Generic Arguments</param>
        /// <param name="returnType">Return Type</param>
        /// <param name="declaredOnly">True to search the methods decleared in this type only, false to search base types.</param>
        /// <returns></returns>
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

        public int GetFieldIndex(object token)
        {
            return 0;
        }

        public IMethod GetConstructor(List<IType> param)
        {
            return null;
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
