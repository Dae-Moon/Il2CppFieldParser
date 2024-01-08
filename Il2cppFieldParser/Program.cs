using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using dnlib.DotNet;

namespace Il2CppFieldParser
{
    internal class Program
    {
        private const string fileName = "Fields.cs";
        private const string separatorText = "_";

        private const bool sortNamespaces = true;
        private const bool sortClasses = true;
        private const bool sortFields = true;

        private const uint limitFiles = 0U;
        private const uint spacing = 0U;

        private const string deniedSymbols = "\\/:*?\"<>|+";

        private static void Close(int exitCode = 0)
        {
            Console.Write("\nPress Enter for exit...");
            if (Console.ReadKey(true).Key == ConsoleKey.Enter)
                Environment.Exit(exitCode);
        }
        
        private static void Main()
        {
            ConsoleEx.padding = 1;
            for (;;)
            {
                try
                {
                    if (File.Exists("Fields.cs"))
                        File.Delete("Fields.cs");

                    Console.Clear();
                    Console.Write("Enter path to directory with assemblies: ");

                    ModuleContext context = ModuleDef.CreateModuleContext();
                    ModuleDefMD moduleDefMD = ModuleDefMD.Load(Console.ReadLine(), context);

                    if (moduleDefMD == null)
                    {
                        ConsoleEx.WriteLine(ConsoleColor.Red, "\nERROR: The module was not loaded due to an unknown reason.");
                        Close(0);
                    }
                    else
                    {
                        if (!moduleDefMD.IsILOnly)
                        {
                            ConsoleEx.WriteLine(ConsoleColor.Red, "\nERROR: The module is native.");
                            Close(0);
                        }
                        else
                        {
                            int num = 0;
                            StringBuilder stringBuilder = new StringBuilder().AppendLine("public static class FieldOffsets\n{");
                            foreach (TypeDef typeDef in moduleDefMD.Types)
                            {
                                if (typeDef.IsClass)
                                {
                                    num++;
                                    List<FieldDef> list = new List<FieldDef>(typeDef.Fields);
                                    list.Sort((FieldDef left, FieldDef right) => string.Compare(left.Name, right.Name));
                                    foreach (FieldDef fieldDef in list)
                                    {
                                        if (!(!fieldDef.HasCustomAttributes || fieldDef.HasConstant))
                                        {
                                            CANamedArgument offset = null;
                                            if (fieldDef.CustomAttributes.Any(delegate (CustomAttribute t) { offset = t.GetField("Offset"); return offset != null; }))
                                            {
                                                if (!typeDef.FullName.Any((char t) => "\\/:*?\"<>|+".Contains(t)))
                                                {
                                                    stringBuilder.AppendLine(string.Concat(new object[]
                                                    {
                                                        "    public const int ",
                                                        typeDef.FullName.Replace(".", "_"),
                                                        "_",
                                                        fieldDef.Name,
                                                        " = ",
                                                        offset.Value,
                                                        ";"
                                                    }));

                                                    for (int i = 0; i < spacing; i++)
                                                        stringBuilder.AppendLine();
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            stringBuilder.Append('}');
                            File.WriteAllText("Fields.cs", stringBuilder.ToString());
                            ConsoleEx.WriteLine(ConsoleColor.Green, "The fields were successfully parsed!");
                            Close(0);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ConsoleEx.WriteLine(ConsoleColor.Red, "\nERROR: " + ex.ToString());
                    Close(0);
                }
            }
        }
    }
}
