using System;
using System.Data.Common;
namespace blqw.Data
{
    public interface IFQLResult
    {
        /// <summary> 导入返回参数
        /// </summary>
        void ImportOutParameter();
        /// <summary> sql指令文本
        /// </summary>
        string CommandText { get; }
        /// <summary> sql指令参数
        /// </summary>
        DbParameter[] DbParameters { get; }

        IFQLConcat AsConcat(string firstConnector);
    }
}
