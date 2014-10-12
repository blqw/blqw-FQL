using System;
using System.Collections.Generic;
using System.Text;

namespace blqw.Data
{
    public interface IFQLConcat : IFQLResult
    {
        string FirstConnector { get; }
        void Append(string connector, string sqlformat, params object[] args);
    }
}
