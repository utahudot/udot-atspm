#region license
// Copyright 2024 Utah Departement of Transportation
// for DomainCore - ATSPM.Domain.Extensions/ServiceProviderExtensions.cs
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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;

namespace ATSPM.Domain.Extensions
{
    /// <summary>
    /// Extensions for <see cref="IServiceProvider"></see> and <see cref="IServiceCollection"/>
    /// </summary>
    public static class ServiceProviderExtensions
    {
        /// <summary>
        /// Finds all implementations of <typeparamref name="Tinterface"/> from assembly
        /// and registers them with <paramref name="services"/> using the <paramref name="lifetime"/>
        /// </summary>
        /// <typeparam name="Tinterface"></typeparam>
        /// <param name="services"></param>
        /// <param name="lifetime"></param>
        /// <returns></returns>
        public static IServiceCollection RegisterServicesByInterface<Tinterface>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(m => m.GetTypes().Where(w => w.GetInterfaces().Contains(typeof(Tinterface))))
                .Where(w => !w.IsInterface && !w.IsAbstract)
                .ToList();

            foreach (var t in types)
            {
                services.Add(new ServiceDescriptor(typeof(Tinterface), t, lifetime));
            }

            return services;
        }

        /// <summary>
        /// Finds all implementations of <typeparamref name="Tinterface"/> from assembly
        /// and registers them with <paramref name="services"/> using the <paramref name="lifetime"/>.
        /// Also binds the <typeparamref name="Tconfiguration"/> to the service using the <paramref name="host"/>
        /// </summary>
        /// <typeparam name="Tinterface"></typeparam>
        /// <typeparam name="Tconfiguration"></typeparam>
        /// <param name="services"></param>
        /// <param name="host"></param>
        /// <param name="lifetime"></param>
        /// <returns></returns>
        public static IServiceCollection RegisterServicesByInterfaceAndConfiguration<Tinterface, Tconfiguration>(this IServiceCollection services, HostBuilderContext host, ServiceLifetime lifetime = ServiceLifetime.Transient) where Tconfiguration : class
        {
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(m => m.GetTypes().Where(w => w.GetInterfaces().Contains(typeof(Tinterface))))
                .Where(w => !w.IsInterface && !w.IsAbstract)
                .ToList();

            foreach (var t in types)
            {
                services.Add(new ServiceDescriptor(typeof(Tinterface), t, lifetime));

                if (host.Configuration.GetSection($"{typeof(Tconfiguration).Name}:{t.Name}").Exists())
                {
                    services.Configure<Tconfiguration>(name: t.Name, host.Configuration.GetSection($"{typeof(Tconfiguration).Name}:{t.Name}"));
                }
            }

            return services;
        }
    }
}
