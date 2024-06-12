#region license
// Copyright 2024 Utah Departement of Transportation
// for Infrastructure - ATSPM.Infrastructure.Logging/ControllerEventLogger.cs
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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Infrastructure.Logging
{
    public class ColorConsoleLoggerConfiguration
    {
        public int EventId { get; set; }

        public Dictionary<LogLevel, ConsoleColor> LogLevels { get; set; } = new Dictionary<LogLevel, ConsoleColor>()
        {
            [LogLevel.Information] = ConsoleColor.Green
        };
    }

    public class ControllerEventLoggerProvider : ILoggerProvider
    {
        private readonly IDisposable _onChangeToken;
        private ColorConsoleLoggerConfiguration _currentConfig;
        private readonly ConcurrentDictionary<string, ControllerEventLogger> _loggers = new ConcurrentDictionary<string, ControllerEventLogger>();

        public ControllerEventLoggerProvider(IOptionsMonitor<ColorConsoleLoggerConfiguration> config)
        {
            _currentConfig = config.CurrentValue;
            _onChangeToken = config.OnChange(updatedConfig => _currentConfig = updatedConfig);
        }

        public ILogger CreateLogger(string categoryName)
        {
            _loggers.GetOrAdd(categoryName, name => new ControllerEventLogger(name, GetCurrentConfig));

            return new ControllerEventLogger(categoryName);
        }

        private ColorConsoleLoggerConfiguration GetCurrentConfig() => _currentConfig;

        public void Dispose()
        {
            _loggers.Clear();
            _onChangeToken.Dispose();
        }
    }

    //https://docs.microsoft.com/en-us/dotnet/core/extensions/custom-logging-provider
    public class ControllerEventLogger : ILogger
    {
        private readonly string _name;
        private readonly Func<ColorConsoleLoggerConfiguration> _getCurrentConfig;

        public ControllerEventLogger(string name, Func<ColorConsoleLoggerConfiguration> getCurrentConfig) => (_name, _getCurrentConfig) = (name, getCurrentConfig);

        public ControllerEventLogger(string name) => _name = name;

        public IDisposable BeginScope<TState>(TState state) => default;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            //check if valid log leven
            if (!IsEnabled(logLevel))
            {
                return;
            }

            // public int EventCode { get; set; }
            //public int EventParam { get; set; }
            if (state is IReadOnlyList<KeyValuePair<string, object>> parameters)
            {
                ControllerEventLog logEvent = new ControllerEventLog()
                {
                    SignalIdentifier = parameters.Where(k => k.Key == "locationId")?.Select(v => v.Value)?.FirstOrDefault()?.ToString() ?? string.Empty,
                    Timestamp = DateTime.Now,
                    EventCode = eventId.Id,
                    EventParam = Convert.ToInt32(parameters.Where(k => k.Key == "EventParam")?.Select(v => v.Value)?.FirstOrDefault() ?? 0)
                };

                //using (MOEEntities db = new MOEEntities())
                //{
                //    db.ApplicationEvents.Add(applicationEvent);
                //    db.SaveChanges();
                //}
            }
        }
    }

    public static class ColorConsoleLoggerExtensions
    {
        public static ILoggingBuilder AddControllerEventLogger(this ILoggingBuilder builder)
        {
            builder.AddConfiguration();

            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, ControllerEventLoggerProvider>());

            LoggerProviderOptions.RegisterProviderOptions<ColorConsoleLoggerConfiguration, ControllerEventLoggerProvider>(builder.Services);

            return builder;
        }

        public static ILoggingBuilder AddControllerEventLogger(this ILoggingBuilder builder, Action<ColorConsoleLoggerConfiguration> configure)
        {
            builder.AddControllerEventLogger();
            builder.Services.Configure(configure);

            return builder;
        }
    }
}
