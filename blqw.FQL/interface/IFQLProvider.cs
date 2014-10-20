using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace blqw
{
    /// <summary> 提供用于检索控制格式化Sql语句的对象的机制
    /// </summary>
    public interface IFQLProvider
    {
        /// <summary> sql指令参数名称的前缀 例如 sqlserver中的@ Oracle中的:
        /// </summary>
        string ParameterPrefix { get; }
        /// <summary> 根据Value值创建DbParameter对象
        /// </summary>
        DbParameter CreateDbParameter(object value);
        /// <summary> 根据DbType类型创建DbParameter对象
        /// </summary>
        DbParameter CreateDbParameter(DbType dbType, Type type);
    }
}
