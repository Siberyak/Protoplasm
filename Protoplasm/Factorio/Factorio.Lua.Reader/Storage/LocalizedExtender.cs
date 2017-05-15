using System.Linq;
using Newtonsoft.Json.Linq;

namespace Factorio.Lua.Reader
{
    public static class LocalizedExtender
    {
        public static string LocalisedName(this ILocalized localized)
        {
            if (localized == null)
                return null;

            string result = null;
            var args = localized._LocalisedName ?? new object[0];
            if (args.Length > 0)
            {
                var pattern = (string)args[0];
                var parts = pattern.Split('.');
                var category = parts[0];
                var value = parts[1];

                if (args.Length > 1)
                {
                    var array = (JArray)args[1];
                    args = array
                        .Select(x => Extensions.Value<string>(x).Split('.'))
                        .Select(x => localized.Storage.Localization(x[0], x[1]))
                        .ToArray();
                }

                result = localized.Storage.Localization(category, value, args);
            }
            
            result = result ?? localized.Storage.Localization(localized.Category, localized.Name);

            return result;
        }
    }
}