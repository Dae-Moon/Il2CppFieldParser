using System;
using System.IO;
using System.Linq;
using dnlib.DotNet;

namespace Il2CppFieldParser
{
    internal class Program
    {
        private const string deniedSymbols = "\\/:*?\"<>|+";

        private static void Main(string[] args)
        {
            try
            {
                var path = GetPathAssemblies(args);
                if (string.IsNullOrWhiteSpace(path))
                {
                    ConsoleEx.Error("Failed to get the directory path.");
                    return;
                }

                foreach (var fileName in Directory.GetFiles(path))
                {
                    var module = ModuleDefMD.Load(fileName);
                    if (module != null && module.IsILOnly)
                        ParseModule(module);
                }

            }
            catch (Exception ex)
            {
                ConsoleEx.Error(ex.Message);
            }

            Pause();
        }

        public static void Pause()
        {
            Console.Write("Press any key to continue ...");
            Console.ReadKey(true);
        }

        private static void ParseModule(ModuleDefMD module)
        {
            foreach (var type in module.Types)
            {
                if (type.IsClass && !type.FullName.Any((char t) => deniedSymbols.Contains(t)))
                {
                    var typeBuilder = new TypeBuilder(type.Name);
                    typeBuilder.SetNamespace(type.Namespace);
                    bool isEmpty = true;
                    
                    foreach (var field in type.Fields)
                    {
                        if (!field.HasConstant && field.HasCustomAttributes && !field.Name.String.Any((char t) => deniedSymbols.Contains(t)))
                        {
                            CANamedArgument offset = null;
                            if (field.CustomAttributes.Any(delegate (CustomAttribute t) { offset = t.GetField("Offset"); return offset != null; }))
                            {
                                typeBuilder.AddField("int", field.Name, "public const", offset.Value.ToString());
                                isEmpty = false;
                            }
                        }
                    }

                    if (!isEmpty)
                    {
                        var path = Path.Combine(Environment.CurrentDirectory, "Output", module.Name.Remove(module.Name.LastIndexOf('.')), type.Namespace.Replace('.', '\\'));
                        Directory.CreateDirectory(path);
                        path = Path.Combine(path, type.Name);
                        path += ".cs";
                        File.WriteAllText(path, typeBuilder.ToString());
                    }

                    typeBuilder.Dispose();
                }
            }
        }

        private static string GetPathAssemblies(string[] args)
        {
            if (args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                    if (Directory.Exists(args[i]))
                        return args[i];
            }
            else
            {
                Console.Write("Enter path to directory with assemblies: ");
                var path = Console.ReadLine();

                if (Directory.Exists(path))
                    return path;
            }

            return string.Empty;
        }
    }
}
