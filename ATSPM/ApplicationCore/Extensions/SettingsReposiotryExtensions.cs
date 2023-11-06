using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Extensions
{
    /// <summary>
    /// Extensions for <see cref="ISettingsRepository"/>
    /// </summary>
    public static class SettingsReposiotryExtensions
    {
        /// <summary>
        /// returns a string value by setting key
        /// </summary>
        /// <param name="repo"></param>
        /// <param name="setting">Unique setting key</param>
        /// <returns></returns>
        public static string LookupSetting(this ISettingsRepository repo, string setting)
        {
            var s = repo.GetList().FirstOrDefault(f => f.Setting == setting);

            if (s == null)
                return null;

            return s.Value;
        }

        /// <summary>
        /// Returns the settings as a dictionary
        /// </summary>
        /// <param name="repo"></param>
        /// <returns></returns>
        public static IDictionary<string, string> SettingsDictionary(this ISettingsRepository repo) 
        {
            return repo.GetList().ToDictionary(k => k.Setting, k => k.Value);
        }
    }
}
