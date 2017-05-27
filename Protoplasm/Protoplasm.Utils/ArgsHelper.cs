using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Protoplasm.Utils
{
    public static class ArgsHelper<T>
        where T : new()
    {
        public delegate void ProcessArgumentDelegate(T holder, string argumentValue);

        static readonly Dictionary<string, ProcessArgumentDelegate> _processes = new Dictionary<string, ProcessArgumentDelegate>();
        static readonly OrderedDictionary _helps = new OrderedDictionary();


        public static void Register(string pattern, ProcessArgumentDelegate processArgValue, string help = null)
        {
            _processes.Add(pattern, processArgValue);
            _helps.Add(pattern, help);
        }

        public static void ViewHelp()
        { }

        public static T ProcessArgs(params string[] args)
        {
            var result = new T();

            foreach (var arg in args)
            {
                var key = _processes.Keys.FirstOrDefault(x => arg.StartsWith(x));
                if (key == null)
                    continue;

                var value = new string(arg.Skip(key.Length).ToArray());
                _processes[key](result, value);
            }

            return result;
        }
    }

    public static class ArgsHelper
    {
        static readonly Dictionary<string, Action<string>> _processes = new Dictionary<string, Action<string>>();
        static readonly OrderedDictionary _helps = new OrderedDictionary();


        public static void Register(string pattern, Action<string> processArgValue, string help = null)
        {
            _processes.Add(pattern, processArgValue);
            _helps.Add(pattern, help);
        }

        public static void ViewHelp()
        { }

        public static void ProcessArgs(params string[] args)
        {
            foreach (var arg in args)
            {
                var key = _processes.Keys.FirstOrDefault(x => arg.StartsWith(x));
                if (key == null)
                    continue;

                var value = new string(arg.Skip(key.Length).ToArray());
                _processes[key](value);
            }
        }
    }
}