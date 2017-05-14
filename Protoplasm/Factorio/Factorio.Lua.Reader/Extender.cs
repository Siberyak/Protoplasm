using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json.Linq;
using NLua;
using Protoplasm.Utils;

namespace Factorio.Lua.Reader
{
    public static class Extender
    {
        public static IDictionary<object, object> AsDictionary(this LuaTable table)
        {
            return table.AsDictionary(x => x);
        }

        public static IDictionary<object, object> AsDictionary(this LuaTable table, Func<object, object> valueTranslater)
        {
            return table.Keys.OfType<object>().ToDictionary(x => x, x => valueTranslater(table[x]));
        }

        public static IEnumerable<KeyValuePair<object, object>> AsEnumerable(this LuaTable table, Func<object, object> valueTranslater)
        {
            return table.Keys.OfType<object>().Select(x => new KeyValuePair<object, object>(x, valueTranslater(table[x])));
        }

        public static IEnumerable<KeyValuePair<object, object>> AsEnumerable(this LuaTable table)
        {
            return table.AsEnumerable(x => x);
        }

        public static KeyValuePair<object, object>[] ToArray(this LuaTable table, Func<object, object> valueTranslater = null)
        {
            return table.AsEnumerable(valueTranslater ?? (o => o)).ToArray();
        }

        public static void Write(this JTokenWriter writer, object value)
        {
            if (value is LuaTable)
            {
                var table = (LuaTable)value;
                var data = table.ToArray();

                if (PreocessData(writer, data))
                    return;

                writer.WriteStartObject();
                writer.WritePropertyName("items");
                var array = data.Where(x => x.Key is double).ToArray();
                if (!ProcessArray(writer, array))
                    throw new Exception();


                var properties = data.Where(x => !(x.Key is double)).ToArray();
                if (!ProcessProperties(writer, properties, false))
                    throw new Exception();

                writer.WriteEndObject();
                return;

                throw new NotSupportedException();
            }

            writer.WriteValue(value);
        }

        private static bool PreocessData(JTokenWriter writer, KeyValuePair<object, object>[] data, bool startObject = true)
        {
            var isArray = data.All(x => x.Key is double);
            return isArray
                ? ProcessArray(writer, data)
                : ProcessProperties(writer, data, startObject);
        }

        private static bool ProcessProperties(JTokenWriter writer, KeyValuePair<object, object>[] data, bool startObject)
        {
            var isProperties = data.All(x => x.Key is string);
            if (!isProperties)
                return false;

            if (startObject)
                writer.WriteStartObject();
            foreach (var pair in data.OrderBy(x => x.Key).ToArray())
            {
                writer.WritePropertyName((string)pair.Key);
                writer.Write(pair.Value);
            }
            if (startObject)
                writer.WriteEndObject();
            return true;
        }

        private static bool ProcessArray(JTokenWriter writer, KeyValuePair<object, object>[] data)
        {
            writer.WriteStartArray();
            foreach (var item in data.OrderBy(x => x.Key).Select(x => x.Value).ToArray())
            {
                writer.Write(item);
            }
            writer.WriteEndArray();
            return true;
        }

        public static string[] Keys(this JObject obj)
        {
            return obj?.Properties().Select(x => x.Name).OrderBy(x => x).ToArray() ?? new string[0];
        }
    }

    public class RecipePartInfo
    {
        IRecipePart _part;
        public string LocalizedName => _part.LocalizedName;

        public string Name => _part.Name;


    }

    public class PartsList : List<IRecipePart>, ITypedList
    {
        public PartsList(IEnumerable<IRecipePart> collection) : base(collection)
        {
        }

        public string GetListName(PropertyDescriptor[] listAccessors)
        {
            return $"{GetType().TypeName()}";
        }

        public PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors)
        {
            return TypeDescriptor.GetProperties(typeof (IRecipePart));
        }
    }
}