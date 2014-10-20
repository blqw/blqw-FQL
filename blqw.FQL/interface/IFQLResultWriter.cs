﻿using System;
using System.Collections.Generic;
using System.Text;

namespace blqw
{
    /// <summary> 返回值可写模式
    /// </summary>
    public interface IFQLResultWriter : IFQLResult
    {
        /// <summary> 第一次连接语句时使用的符号
        /// </summary>
        string FirstConnector { get; }
        /// <summary> 连接新的sql语句,如果是第一次,则使用 FirstConnector 作为连接符号,否则使用参数 connector
        /// </summary>
        /// <param name="connector">连接sql语句时使用的连接符号</param>
        /// <param name="sqlformat">需要格式化的sql语句</param>
        /// <param name="args">sql参数</param>
        void Append(string connector, string sqlformat, params object[] args);

        /// <summary> 使用And符号连接sql,如果是第一次,则使用 WHERE 作为连接符号,否则使用 AND
        /// </summary>
        /// <param name="sqlformat">需要格式化的sql语句</param>
        /// <param name="args">sql参数</param>
        void And(string sqlformat, params object[] args);
        /// <summary> 使用Or符号连接sql,如果是第一次,则使用 WHERE 作为连接符号,否则使用 OR
        /// </summary>
        /// <param name="sqlformat">需要格式化的sql语句</param>
        /// <param name="args">sql参数</param>
        void Or(string sqlformat, params object[] args);
        /// <summary> 使用逗号连接sql,如果是第一次,则使用 WHERE 作为连接符号,否则使用 OR
        /// </summary>
        /// <param name="sqlformat">需要格式化的sql语句</param>
        /// <param name="args">sql参数</param>
        void Comma(string sqlformat, params object[] args);
    }
}
