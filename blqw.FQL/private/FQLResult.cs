﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading;

namespace blqw
{
    /// <summary> FQL.Format 方法的返回值
    /// </summary>
    struct FQLResult : IFQLResult
    {
        /// <summary> sql指令文本
        /// </summary>
        public readonly string CommandText;
        /// <summary> sql指令参数
        /// </summary>
        public readonly DbParameter[] DbParameters;
        /// <summary> 设置返回值的回调函数
        /// </summary>
        public readonly ThreadStart _callback;
        /// <summary> 格式化机制
        /// </summary>
        private IFQLProvider _provider;
        /// <summary> 参数个数
        /// </summary>
        private int _argumentCount;
        public FQLResult(IFQLProvider provider, string commandText, DbParameter[] parameters, ThreadStart callback, int argumentCount)
        {
            _provider = provider;
            CommandText = commandText;
            DbParameters = parameters;
            _callback = callback;
            _argumentCount = argumentCount;
        }

        /// <summary> 导入返回参数
        /// </summary>
        public void ImportOutParameter()
        {
            var callback = _callback;
            if (callback != null)
            {
                callback();
            }
        }

        public IFQLResultWriter AsWriter()
        {
            return new FQLResultWriter(null, _provider, CommandText, DbParameters, _callback, _argumentCount);
        }

        public IFQLResultWriter AsWriter(string firstConnector)
        {
            return new FQLResultWriter(firstConnector, _provider, CommandText, DbParameters, _callback, _argumentCount);
        }

        /// <summary> sql指令文本
        /// </summary>
        string IFQLResult.CommandText
        {
            get { return CommandText; }
        }
        /// <summary> sql指令参数
        /// </summary>
        DbParameter[] IFQLResult.DbParameters
        {
            get { return DbParameters; }
        }

    }
}
