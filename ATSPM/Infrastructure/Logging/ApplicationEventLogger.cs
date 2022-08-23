using ATSPM.Data.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Extensions.Logging
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
