using System;
using System.Timers;

namespace Protoplasm.Utils
{
    public static class ConsoleExtender
    {
        public static bool Active => Environment.UserInteractive;

        public class Holder : IDisposable
        {
            private static readonly bool NoConsole = !Active;

            private static readonly object Locker = new object();

            private readonly Timer _timer;
            private string _value;
            private bool _changed;

            private Holder(string message, params object[] args)
            {
                Console.Write(message, args);

                try
                {
                    if (NoConsole)
                        return;

                    Left = Console.CursorLeft;
                    Top = Console.CursorTop;
                    ForeColor = Console.ForegroundColor;
                    BackColor = Console.BackgroundColor;

                    _timer = new Timer {/*AutoReset = false, */Enabled = false };
                    _timer.Start();
                    _timer.Elapsed += (sender, e)
                        =>
                    {
                        if (!_changed)
                            return;

                        _changed = false;
                        Write(Value);
                    };
                }
                finally
                {
                    Console.WriteLine();
                }

            }

            public ConsoleColor ForeColor { get; set; }
            public ConsoleColor BackColor { get; set; }
            public int Left { get; set; }
            public int Top { get; set; }
            public int Width { get; set; }

            public string Value
            {
                get { return _value; }
                private set
                {
                    _changed |= _value != value;
                    _value = value;
                }
            }

            public void SetValue(string pattern, params object[] args)
            {
                Value = string.Format(pattern, args);
            }

            void Do(Action action)
            {
                lock (Locker)
                {
                    var left = Console.CursorLeft;
                    var top = Console.CursorTop;
                    var foreColor = Console.ForegroundColor;
                    var backColor = Console.BackgroundColor;

                    Console.SetCursorPosition(Left, Top);
                    Console.ForegroundColor = ForeColor;
                    Console.BackgroundColor = BackColor;

                    action();


                    Console.SetCursorPosition(left, top);
                    Console.ForegroundColor = foreColor;
                    Console.BackgroundColor = backColor;
                }
            }

            // ReSharper disable once MemberHidesStaticFromOuterClass
            private void Write(string pattern, params object[] args)
            {
                Do(() => Console.Write(pattern, args));
            }

            // ReSharper disable once MemberHidesStaticFromOuterClass
            // ReSharper disable once UnusedMember.Local
            private void WriteLine(string pattern, params object[] args)
            {
                Do(() => Console.Write(pattern, args));
            }

            public static Holder Get(string message, params object[] args)
            {
                lock (Locker)
                {
                    return new Holder(message, args);
                }
            }

            public void Dispose()
            {
                if (NoConsole)
                    return;

                Write(Value);
                _timer.Stop();
                _timer.Dispose();
            }
        }

        public static void Write(this string patternt, ConsoleColor color, params object[] args)
        {
            if(!Active)
                return;

            var foregroundColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Write(patternt, args);
            Console.ForegroundColor = foregroundColor;
        }

        public static void Write(this string patternt, params object[] args)
        {
            if (!Active)
                return;

            Console.Write(patternt, args);
        }

        public static void Write<T>(this T value)
        {
            Write($"{value}");
        }

        public static void Write<T>(this T value, ConsoleColor color)
        {
            Write($"{value}", color);
        }
        public static void WriteLine(this string patternt, ConsoleColor color, params object[] args)
        {
            if (!Active)
                return;

            var foregroundColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            WriteLine(patternt, args);
            Console.ForegroundColor = foregroundColor;
        }

        public static void WriteLine<T>(this T value)
        {
            WriteLine($"{value}");
        }

        public static void WriteLine<T>(this T value, ConsoleColor color)
        {
            WriteLine($"{value}", color);
        }


        public static void WriteLine(this string patternt, params object[] args)
        {
            if (!Active)
                return;

            Console.WriteLine(patternt, args);
        }


    }
}