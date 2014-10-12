using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading;

namespace blqw.Data
{
    public struct FQLConcat : IFQLConcat
    {
        /// <summary> 设置返回值的回调函数
        /// </summary>
        private ThreadStart _callback;
        private IFQLProvider _provider;
        private List<string> _commandTexts;
        private List<DbParameter> _parameters;
        private int _commandTextCount;
        private int _parameterCount;
        private int _argumentCount;
        private string _firstConnector;
        private bool _first;
        private int _generations;

        public FQLConcat(string firstConnector, IFQLProvider provider, string commandText, DbParameter[] parameters, ThreadStart callback, int argumentCount)
        {
            _generations = 0;
            _first = true;
            _firstConnector = firstConnector;
            _provider = provider;
            _callback = callback;
            _commandTexts = new List<string> { commandText };
            _parameters = new List<DbParameter>(parameters);
            _commandTextCount = 1;
            _parameterCount = parameters.Length;
            _argumentCount = argumentCount;
        }

        public void ImportOutParameter()
        {
            var callback = _callback;
            if (callback != null)
            {
                callback();
            }
        }

        public string CommandText
        {
            get
            {
                if (_commandTextCount == 1)
                {
                    return _commandTexts[0];
                }
                if (_commandTextCount == _commandTexts.Count)
                {
                    return string.Join(" ", _commandTexts.ToArray());
                }
                var strArr = new string[_commandTextCount];
                _commandTexts.CopyTo(0, strArr, 0, _commandTextCount);
                return string.Join(" ", strArr);
            }
        }

        public DbParameter[] DbParameters
        {
            get
            {
                if (_parameterCount == _parameters.Count)
                {
                    return _parameters.ToArray();
                }
                var arr = new DbParameter[_parameterCount];
                _parameters.CopyTo(0, arr, 0, _parameterCount);
                return arr;
            }
        }

        public IFQLConcat AsConcat(string firstConnector)
        {
            var a = _first;
            var b = _firstConnector;
            try
            {
                _first = true;
                _firstConnector = firstConnector;
                return this;
            }
            finally
            {
                _first = a;
                _firstConnector = b;
            }
        }

        string IFQLConcat.FirstConnector
        {
            get { return _firstConnector; }
        }

        void IFQLConcat.Append(string connector, string sqlformat, params object[] args)
        {
            var r = FQL.Format(_provider, _argumentCount, sqlformat, args);
            _argumentCount += args.Length;
            if (_first)
            {
                _first = false;
                _commandTexts.Add(_firstConnector);
            }
            else
            {
                _commandTexts.Add(connector);
            }
            _commandTexts.Add(r.CommandText);
            _commandTextCount += 2;
            _parameters.AddRange(r.DbParameters);
            _parameterCount += r.DbParameters.Length;
            if (r._callback != null)
            {
                _callback += r._callback;
            }
        }

    }
}
