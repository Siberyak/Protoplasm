using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace KG.SE2.Utils
{
    public static class ConfigExtender
    {
        public static readonly Func<string, string[]> ByMasterKeyAndMachineName =
            key => new[]
                       {
                           $"{MasterKey}.{MachineName}.{key}",
                           $"{MachineName}.{key}",
                           $"{MasterKey}.{key}",
                           $"{key}",
                       };

        public static readonly Func<string, string[]> ByMachineName =
            key => new[]
                       {
                           $"{MachineName}.{key}",
                           key
                       };

        public static string MasterKey => "MASTER-KEY".AppSetting(ByMachineName);
        public static string MachineName => Environment.MachineName.ToUpper();

        public static string AppSetting(this string key, Func<string, string[]> getLocalKey = null)
        {
            if (!ConfigurationManager.AppSettings.HasKeys())
                return null;


            if (getLocalKey != null)
            {
                string[] localKeys = getLocalKey(key);
                foreach (string localKey in localKeys)
                {
                    if (ConfigurationManager.AppSettings.AllKeys.Contains(localKey))
                        return ConfigurationManager.AppSettings[localKey];
                }
            }

            if (ConfigurationManager.AppSettings.AllKeys.Contains(key))
                return ConfigurationManager.AppSettings[key];

            return null;
        }

        public static int AppSettingAsInt(this string key, int defaultValue, Func<string, string[]> getLocalKey = null)
        {
            string value = key.AppSetting(getLocalKey);
            int intValue;
            return string.IsNullOrWhiteSpace(value)
                       ? defaultValue
                       : int.TryParse(value, out intValue) ? intValue : defaultValue;
        }

        public static bool AppSettingAsBool(this string key, Func<string, string[]> getLocalKey = null)
        {
            bool value;
            return bool.TryParse(key.AppSetting(getLocalKey), out value) && value;
        }

        public static ConnectionStringSettings ConnectionStrings(this string key, Func<string, string[]> getLocalKey = null)
        {
            ConnectionStringSettings[] collection = ConfigurationManager.ConnectionStrings.OfType<ConnectionStringSettings>().ToArray();
            if (!collection.Any())
                return null;

            string[] localKeys = getLocalKey?.Invoke(key) ?? new [] { key };

            foreach (string localKey in localKeys)
            {
                var value = collection.FirstOrDefault(x => x.Name == localKey);
                if (value != null)
                    return value;
            }

            return null;
        }
    }
}