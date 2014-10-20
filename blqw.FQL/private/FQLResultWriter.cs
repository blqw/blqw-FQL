﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading;

namespace blqw
{
    struct FQLResultWriter : IFQLResultWriter
    {
        private ThreadStart _callback; //返回值的回调
        private IFQLProvider _provider; //格式化机制
        private List<string> _commandTexts; //sql语句集合
        private List<DbParameter> _parameters; //参数集合
        private int _commandTextsLimit; //sql语句数量限制
        private int _parametersLimit; //参数数量限制
        private int _argumentStart; //参数化名称的起始值
        private string _firstConnector; //首次连接语句时使用的符号
        private bool _first;

        public FQLResultWriter(string firstConnector, IFQLProvider provider, string commandText, DbParameter[] parameters, ThreadStart callback, int argumentCount)
        {
            _first = true;
            _firstConnector = firstConnector;
            _provider = provider;
            _callback = callback;
            _commandTexts = new List<string> { commandText };
            _parameters = new List<DbParameter>(parameters);
            _commandTextsLimit = 1;
            _parametersLimit = parameters.Length;
            _argumentStart = argumentCount;
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
                if (_commandTextsLimit == 1)
                {
                    return _commandTexts[0];
                }
                if (_commandTextsLimit == _commandTexts.Count)
                {
                    return string.Join(" ", _commandTexts.ToArray());
                }
                var strArr = new string[_commandTextsLimit];
                _commandTexts.CopyTo(0, strArr, 0, _commandTextsLimit);
                return string.Join(" ", strArr);
            }
        }

        public DbParameter[] DbParameters
        {
            get
            {
                if (_parametersLimit == _parameters.Count)
                {
                    return _parameters.ToArray();
                }
                var arr = new DbParameter[_parametersLimit];
                _parameters.CopyTo(0, arr, 0, _parametersLimit);
                return arr;
            }
        }

        public string FirstConnector
        {
            get { return _firstConnector; }
        }

        public void Append(string connector, string sqlformat, params object[] args)
        {
            var r = (FQLResult)FQL.Format(_provider, _argumentStart, sqlformat, args);
            _argumentStart += args.Length;
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
            _commandTextsLimit += 2;
            _parameters.AddRange(r.DbParameters);
            _parametersLimit += r.DbParameters.Length;
            if (r._callback != null)
            {
                _callback += r._callback;
            }
        }

        void IFQLResultWriter.And(string sqlformat, params object[] args)
        {
            if (_first && _firstConnector == null)
            {
                _first = false;
                Append("WHERE", sqlformat, args);
            }
            else
            {
                Append("AND", sqlformat, args);
            }
        }

        void IFQLResultWriter.Or(string sqlformat, params object[] args)
        {
            if (_first && _firstConnector == null)
            {
                _first = false;
                Append("WHERE", sqlformat, args);
            }
            else
            {
                Append("OR", sqlformat, args);
            }
        }

        void IFQLResultWriter.Comma(string sqlformat, params object[] args)
        {
            Append(",", sqlformat, args);
        }

        public IFQLResultWriter AsWriter()
        {
            return new FQLResultWriter {
                _callback = _callback,
                _provider = _provider,
                _commandTexts = new List<string>(_commandTexts),
                _parameters = new List<DbParameter>(_parameters),
                _commandTextsLimit = _commandTextsLimit,
                _parametersLimit = _parametersLimit,
                _argumentStart = _argumentStart,
                _first = true,
            };
        }

        public IFQLResultWriter AsWriter(string firstConnector)
        {
            return new FQLResultWriter {
                _callback = _callback,
                _provider = _provider,
                _commandTexts = new List<string>(_commandTexts),
                _parameters = new List<DbParameter>(_parameters),
                _commandTextsLimit = _commandTextsLimit,
                _parametersLimit = _parametersLimit,
                _argumentStart = _argumentStart,
                _firstConnector = firstConnector,
                _first = true,
            };
        }
    }
}
