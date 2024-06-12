#region license
// Copyright 2024 Utah Departement of Transportation
// for Infrastructure - ATSPM.Infrastructure.Logging/ApplicationEventLogger.cs
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
using ATSPM.Data.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ATSPM.Infrastructure.Logging
{
    public class ApplicationEventLogger : ILogger
    {
        //private readonly MOEEntities db = new MOEEntities();

        private string _applicationName;
        public ApplicationEventLogger(string applicationName)
        {
            _applicationName = applicationName;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            //Console.WriteLine($"I'm writing the message: {logLevel} - {state}");

            if (state is IReadOnlyList<KeyValuePair<string, object>> parameters)
            {
                //ApplicationEvent applicationEvent = new ApplicationEvent()
                //{
                //    Timestamp = DateTime.Now,
                //    ApplicationName = _applicationName,
                //    Description = formatter(state, exception),
                //    SeverityLevel = (int)logLevel,
                //    Class = parameters.Where(k => k.Key == "Class").Select(v => v.Value).FirstOrDefault().ToString(),
                //    Function = parameters.Where(k => k.Key == "Function").Select(v => v.Value).FirstOrDefault().ToString()
                //};

                //TODO: took this out for testing
                //using (MOEContext db = new MOEContext())
                //{
                //    db.ApplicationEvents.Add(applicationEvent);
                //    db.SaveChanges();
                //}
            }
        }
    }

    public class Logger : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            return new ApplicationEventLogger(categoryName);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
