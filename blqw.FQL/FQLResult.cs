using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading;

namespace blqw.Data
{
    /// <summary> FQL.Format 方法的返回值
    /// </summary>
    public struct FQLResult : IFQLResult
    {
        internal FQLResult(IFQLProvider provider, string commandText, DbParameter[] parameters, ThreadStart callback, int argumentCount)
        {
            _provider = provider;
            CommandText = commandText;
            DbParameters = parameters;
            _callback = callback;
            _argumentCount = argumentCount;
        }
        /// <summary> sql指令文本
        /// </summary>
        public readonly string CommandText;
        /// <summary> sql指令参数
        /// </summary>
        public readonly DbParameter[] DbParameters;
        /// <summary> 设置返回值的回调函数
        /// </summary>
        internal readonly ThreadStart _callback;

        private IFQLProvider _provider;
        private int _argumentCount;

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


        string IFQLResult.CommandText
        {
            get { return CommandText; }
        }

        DbParameter[] IFQLResult.DbParameters
        {
            get { return DbParameters; }
        }

        public IFQLConcat AsConcat(string firstConnector)
        {
            return new FQLConcat(firstConnector, _provider, CommandText, DbParameters, _callback, _argumentCount);
        }
    }
}
