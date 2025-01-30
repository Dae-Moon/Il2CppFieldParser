using System;

namespace Il2CppFieldParser
{
    internal static class ConsoleEx
    {
        public static void Write(ConsoleColor color, string text)
        {
            var backup = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ForegroundColor = backup;
        }
        
        public static void Write(params ValueTuple<string, ConsoleColor>[] args)
        {
            foreach (var arg in args)
            {
                Write(arg.Item2, arg.Item1);
            }
        }

        public static void WriteLine(ConsoleColor color, string text)
        {
            Write(color, text);
            Console.Write(Environment.NewLine);
        }
        
        public static void WriteLine(params ValueTuple<string, ConsoleColor>[] args)
        {
            Write(args);
            Console.Write(Environment.NewLine);
        }

        public static void Success(string text) => WriteLine(ConsoleColor.DarkGreen, $"[SUCCESS] {text}");
        public static void Info(string text) => WriteLine(ConsoleColor.Gray, $"[INFO] {text}");
        public static void Warning(string text) => WriteLine(ConsoleColor.DarkYellow, $"[WARNING] {text}");
        public static void Error(string text) => WriteLine(ConsoleColor.DarkRed, $"[ERROR] {text}");
    }
}
