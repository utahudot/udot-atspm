#region license
// Copyright 2024 Utah Departement of Transportation
// for DomainCore - ATSPM.Domain.Common/GenericLogger.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ATSPM.Domain.Common
{
    // TODO: remove events since they can't be accessed. Instead add an action callback
    
    public class GenericLogger<T1, T2> : GenericLogger<T1>, ILogger<T2>
    {
        public GenericLogger(T1 logWriter, LoggerExternalScopeProvider scopeProvider) : base(logWriter, scopeProvider, typeof(T2).Name) { }
    }

    public class GenericLogger<T1> : ILogger
    {
        public delegate void LogEventHandler(object sender, LogEventArg e);
        public event LogEventHandler OnLogged;
        
        private readonly T1 _logWriter;
        private readonly string _categoryName;
        private readonly LoggerExternalScopeProvider _scopeProvider;

        public static ILogger CreateLogger(T1 logWriter) => new GenericLogger<T1>(logWriter, new LoggerExternalScopeProvider(), "");
        public static ILogger<T2> CreateLogger<T2>(T1 logWriter) => new GenericLogger<T1, T2>(logWriter, new LoggerExternalScopeProvider());

        public GenericLogger(T1 logWriter, LoggerExternalScopeProvider scopeProvider, string categoryName)
        {
            _logWriter = logWriter;
            _scopeProvider = scopeProvider;
            _categoryName = categoryName;
        }

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public IDisposable BeginScope<TState>(TState state) => _scopeProvider.Push(state);

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            OnLogged?.Invoke(this, new LogEventArg(logLevel, eventId, (IReadOnlyList<KeyValuePair<string, object>>)state, exception));
            
            var sb = new StringBuilder();
            sb.Append(logLevel.ToString().ToUpper())
              .Append(": [").Append(_categoryName).Append("] ")
              .Append(formatter(state, exception));

            if (exception != null)
            {
                sb.Append('\n').Append(exception);
            }

            // Append scopes
            _scopeProvider.ForEachScope((scope, state) =>
            {
                state.Append("\n => ");
                state.Append(scope);
            }, sb);

            if (_logWriter.GetType().GetMethods().Any(a => a.Name == "WriteLine"))
            {
                dynamic writer = _logWriter;
                writer.WriteLine(sb.ToString());
            }
        }
    }

    public class GenericLoggerProvider<T> : ILoggerProvider
    {
        private readonly T _logWriter;
        private readonly LoggerExternalScopeProvider _scopeProvider = new LoggerExternalScopeProvider();

        public GenericLoggerProvider(T logWriter)
        {
            _logWriter = logWriter;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new GenericLogger<T>(_logWriter, _scopeProvider, categoryName);
        }

        public void Dispose()
        {
        }
    }

    public class LogEventArg : EventArgs
    {
        public LogEventArg(LogLevel logLevel, EventId eventId, IReadOnlyList<KeyValuePair<string, object>> state, Exception exception) => (LogLevel, EventId, State, Exception) = (logLevel, eventId, state, exception);
        
        public LogLevel LogLevel { get; }

        public EventId EventId { get; }

        public IReadOnlyList<KeyValuePair<string, object>> State { get; }

        public Exception Exception { get; }
    }
}
