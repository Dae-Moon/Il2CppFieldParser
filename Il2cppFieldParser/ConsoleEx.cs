using System;

namespace Il2CppFieldParser
{
    public static class ConsoleEx
    {
        public static int padding = 0;


        public static void Write(params object[] param)
        {
            bool startPadding = false;
            ConsoleColor foregroundColor = Console.ForegroundColor;
            foreach (object obj in param)
            {
                bool flag2 = obj is ConsoleColor;
                if (flag2)
                    Console.ForegroundColor = (ConsoleColor)obj;
                else
                {
                    if (startPadding)
                    {
                        for (int j = 0; j < padding; j++)
                        {
                            Console.Write(' ');
                        }
                    }
                    else
                        startPadding = true;

                    Console.Write(obj);
                }
            }
            Console.ForegroundColor = foregroundColor;
        }
        
        public static void WriteLine(params object[] param)
        {
            Write(param);
            Console.Write('\n');
        }
    }
}
