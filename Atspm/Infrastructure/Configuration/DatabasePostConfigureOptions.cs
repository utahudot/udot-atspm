#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Configuration/DatabasePostConfigureOptions.cs
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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Reflection;
using Utah.Udot.Atspm.Infrastructure.Attributes;

namespace Utah.Udot.Atspm.Infrastructure.Configuration
{
    /// <summary>
    /// Applies database-stored <see cref="UserSetting"/> overrides onto <typeparamref name="TOptions"/> after external configuration binding.
    /// </summary>
    /// <typeparam name="TOptions">The configuration options class marked with <see cref="UserConfigurableAttribute"/>.</typeparam>
    public class DatabasePostConfigureOptions<TOptions>(
        IServiceScopeFactory scopeFactory,
        IReadOnlyList<UserSettingDescriptor> descriptors,
        ILogger<DatabasePostConfigureOptions<TOptions>> logger) : IPostConfigureOptions<TOptions>
        where TOptions : class
    {
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        private readonly IReadOnlyList<UserSettingDescriptor> _descriptors = descriptors;
        private readonly ILogger<DatabasePostConfigureOptions<TOptions>> _logger = logger;

        /// <inheritdoc/>
        public void PostConfigure(string? name, TOptions options)
        {
            var attr = typeof(TOptions).GetCustomAttribute<UserConfigurableAttribute>();
            var sectionName = attr?.SectionName ?? typeof(TOptions).Name;

            var sectionDescriptors = _descriptors
                .Where(d => string.Equals(d.Section, sectionName, StringComparison.OrdinalIgnoreCase))
                .ToDictionary(d => d.Key, StringComparer.OrdinalIgnoreCase);

            if (sectionDescriptors.Count == 0)
            {
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IUserSettingRepository>();

            var prefix = sectionName + ":";
            var overrides = repo.GetList(s => s.Key.StartsWith(prefix));

            foreach (var setting in overrides)
            {
                if (sectionDescriptors.TryGetValue(setting.Key, out var descriptor) && setting.Value is not null)
                {
                    try
                    {
                        var targetType = Nullable.GetUnderlyingType(descriptor.PropertyInfo.PropertyType)
                            ?? descriptor.PropertyInfo.PropertyType;
                        var convertedValue = Convert.ChangeType(setting.Value, targetType, CultureInfo.InvariantCulture);
                        descriptor.PropertyInfo.SetValue(options, convertedValue);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to apply database UserSetting override for key '{Key}' with value '{Value}'.", setting.Key, setting.Value);
                    }
                }
            }
        }
    }
}
