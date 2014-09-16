using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading;

namespace blqw.Data
{
    /// <summary> FQL.Format 方法的返回值
    /// </summary>
    public struct FQLResult
    {
        internal FQLResult(string commandText, DbParameter[] parameters, ThreadStart callback)
        {
            CommandText = commandText;
            DbParameters = parameters;
            Callback = callback;
        }
        /// <summary> sql指令文本
        /// </summary>
        public readonly string CommandText;
        /// <summary> sql指令参数
        /// </summary>
        public readonly DbParameter[] DbParameters;
        /// <summary> 设置返回值的回调函数
        /// </summary>
        internal readonly ThreadStart Callback;
        /// <summary> 导入返回参数
        /// </summary>
        public void ImportOutParameter()
        {
            var callback = Callback;
            if (callback != null)
            {
                callback();
            }
        }
    }
}
