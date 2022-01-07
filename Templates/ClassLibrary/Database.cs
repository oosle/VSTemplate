using System;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Reflection;
using Dapper;

namespace $projectname$
{
    #region DataAccessLayer exception handling class
    
    [Serializable]
    public class DALException : System.Exception
    {
        private static string baseMessage = "DALException";

        public DALException()
            : base()
        { }

        public DALException(string message)
            : base(baseMessage + ": " + message)
        { }

        public DALException(string message, Exception innerException)
            : base(baseMessage + ": " + message, innerException)
        { }

        protected DALException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
    
    #endregion

    #region Dapper mapping classes

    public class ColumnAttributeTypeMapper<T> : FallbackTypeMapper
    {
        public ColumnAttributeTypeMapper() : base(new SqlMapper.ITypeMap[] {
            new CustomPropertyTypeMap(
                typeof(T),
                    (type, columnName) =>
                    type.GetProperties().FirstOrDefault(prop =>
                    prop.GetCustomAttributes(false)
                        .OfType<ColumnAttribute>()
                        .Any(attr => attr.Name == columnName)
                )
            ),
            new DefaultTypeMap(typeof(T))
        }) { }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class ColumnAttribute : Attribute
    {
        public string Name { get; set; }
    }

    public class FallbackTypeMapper : SqlMapper.ITypeMap
    {
        private readonly System.Collections.Generic.IEnumerable<SqlMapper.ITypeMap> _mappers;

        public FallbackTypeMapper(IEnumerable<SqlMapper.ITypeMap> mappers)
        {
            _mappers = mappers;
        }

        public ConstructorInfo FindConstructor(string[] names, Type[] types)
        {
            foreach (var mapper in _mappers)
            {
                try
                {
                    ConstructorInfo result = mapper.FindConstructor(names, types);
                    if (result != null)
                    {
                        return result;
                    }
                }
                catch (NotImplementedException)
                {
                }
            }
            
            return (null);
        }

        public SqlMapper.IMemberMap GetConstructorParameter(ConstructorInfo constructor, string columnName)
        {
            foreach (var mapper in _mappers)
            {
                try
                {
                    var result = mapper.GetConstructorParameter(constructor, columnName);
                    if (result != null)
                    {
                        return result;
                    }
                }
                catch (NotImplementedException)
                {
                }
            }
            
            return (null);
        }

        public SqlMapper.IMemberMap GetMember(string columnName)
        {
            foreach (var mapper in _mappers)
            {
                try
                {
                    var result = mapper.GetMember(columnName);
                    if (result != null)
                    {
                        return result;
                    }
                }
                catch (NotImplementedException)
                {
                }
            }
            
            return (null);
        }

        public ConstructorInfo FindExplicitConstructor()
        {
            return (_mappers
                .Select(mapper => mapper.FindExplicitConstructor())
                .FirstOrDefault(result => result != null));
        }
    }

    #endregion

    #region Static database utility class for bulk SQL operations, daisy chain SQL together

    public static class Utils
    {
        public static DbType GetDbType(string typ, out string name)
        {
            string typName = name = typ;

            if (typ.Contains(" "))
            {
                var split = typ.Split(' ');
                name = split[split.Length - 1];
                split = split[0].Split('.');
                typName = split[split.Length - 1].ToLower();

                // [] comes from any Nullable type defs, can shaft SQL type definition so remove
                if (typName.IndexOf('[') > -1 || typName.IndexOf(']') > -1)
                    typName = typName.Trim('[', ']');
            }

            switch (typName)
            {
                case ("int16"): return (DbType.Int16);
                case ("int32"): return (DbType.Int32);
                case ("int64"): return (DbType.Int64);
                case ("uint16"): return (DbType.UInt16);
                case ("uint32"): return (DbType.UInt32);
                case ("uint64"): return (DbType.UInt64);
                case ("short"): return (DbType.Int16);
                case ("int"): return (DbType.Int32);
                case ("decimal"): return (DbType.Decimal);
                case ("single"): return (DbType.Double);
                case ("double"): return (DbType.Double);
                case ("datetime"): return (DbType.DateTime);
                case ("bool"): return (DbType.Boolean);
                case ("boolean"): return (DbType.Boolean);
                case ("guid"): return (DbType.Guid);
                default: return (DbType.String);
            }
        }

        public static string Replace<T>(T entity, string sql)
        {
            string newsql = sql;
            Type type = entity.GetType();

            foreach (var p in type.GetProperties())
            {
                string pName = p.ToString();
                DbType pType = GetDbType(p.ToString(), out pName);
                object pValue = p.GetValue(entity);
                pName = "@" + pName;

                if (pValue != null)
                {
                    if (pType == DbType.String || pType == DbType.Guid)
                    {
                        string value = (string)pValue;
                        value = value.Replace("'", "`");
                        newsql = newsql.Replace(pName, $"'{value}'");
                    }
                    else if (pType == DbType.DateTime)
                    {
                        DateTime date = (DateTime)pValue;
                        string value = date.ToString("yyyy/MM/dd HH:mm:ss.fff");
                        newsql = newsql.Replace(pName, $"CONVERT(DATETIME, '{value}')");
                    }
                    else if (pType == DbType.Boolean)
                        newsql = newsql.Replace(pName, (bool)pValue == true ? "1" : "0");
                    else
                        newsql = newsql.Replace(pName, $"{pValue}");
                }
                else
                {
                    newsql = newsql.Replace(pName, $"NULL");
                }
            }

            return (newsql);
        }
    }

    #endregion
}
