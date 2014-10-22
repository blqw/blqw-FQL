using System;
using System.Data.Common;
namespace blqw
{
    /// <summary> FQL.Format 方法的返回值定义
    /// </summary>
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
        /// <summary> 将结果转为sql连接模式
        /// </summary>
        IFQLBuilder AsBuilder();

        /// <summary> 将结果转为sql连接模式
        /// </summary>
        IFQLBuilder AsBuilder(string firstConnector);

    }
}
