using System;
using System.Collections.Generic;
using System.Text;

namespace Il2CppFieldParser
{
    public class TypeBuilder : IDisposable
    {
        private string typeName;
        private string namespaceName;
        private List<(string Type, string Name, string AccessModifier, string defaultValue)> fields;
        private List<(string Type, string Name, string AccessModifier, string GetterCode, string SetterCode)> properties;
        private List<(string ReturnType, string Name, List<(string Type, string Name)> Parameters, string AccessModifier, string MethodCode)> methods;
        private string baseClass;
        private List<string> interfaces;
        private string typeKeyword;
        private string description;
        private List<(List<(string Type, string Name)> Parameters, string ConstructorCode)> constructors;
        private List<string> usings;
        private bool disposed = false;

        public TypeBuilder(string typeName, string typeKeyword = "class", string description = "")
        {
            this.typeName = typeName;
            this.typeKeyword = typeKeyword;
            this.description = description;
            fields = new List<(string Type, string Name, string AccessModifier, string defaultValue)>();
            properties = new List<(string Type, string Name, string AccessModifier, string GetterCode, string SetterCode)>();
            methods = new List<(string ReturnType, string Name, List<(string Type, string Name)> Parameters, string AccessModifier, string MethodCode)>();
            interfaces = new List<string>();
            constructors = new List<(List<(string Type, string Name)> Parameters, string ConstructorCode)>();
            usings = new List<string>();
        }

        public void SetNamespace(string namespaceName)
        {
            this.namespaceName = namespaceName;
        }

        public void AddUsing(string usingDirective)
        {
            if (usingDirective != namespaceName)
                usings.Add(usingDirective);
        }

        public void SetBaseClass(string baseClass)
        {
            this.baseClass = baseClass;
        }

        public void AddInterface(string interfaceName)
        {
            interfaces.Add(interfaceName);
        }

        public void AddField(string type, string name, string accessModifier = "private", string defaultValue = "")
        {
            fields.Add((type, name, accessModifier, defaultValue));
        }

        public void AddProperty(string type, string name, string accessModifier = "public", string getterCode = null, string setterCode = null)
        {
            properties.Add((type, name, accessModifier, getterCode, setterCode));
        }

        public void AddMethod(string returnType, string methodName, List<(string Type, string Name)> parameters = null, string accessModifier = "public", string methodCode = "// Method logic here")
        {
            methods.Add((returnType, methodName, parameters ?? new List<(string Type, string Name)>(), accessModifier, methodCode));
        }

        public void AddConstructor(List<(string Type, string Name)> parameters = null, string constructorCode = "// Constructor logic here")
        {
            constructors.Add((parameters ?? new List<(string Type, string Name)>(), constructorCode));
        }

        public StringBuilder Build()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var usingDirective in usings)
            {
                sb.AppendLine($"using {usingDirective};");
            }

            if (!string.IsNullOrWhiteSpace(namespaceName))
            {
                sb.AppendLine();
                sb.AppendLine($"namespace {namespaceName}");
                sb.AppendLine("{");
            }

            if (!string.IsNullOrWhiteSpace(description))
            {
                sb.AppendLine($"/// <summary>");
                sb.AppendLine($"/// {description}");
                sb.AppendLine($"/// </summary>");
            }

            sb.Append($"{typeKeyword} {typeName}");
            if (!string.IsNullOrEmpty(baseClass) || interfaces.Count > 0)
            {
                sb.Append(" : ");
                if (!string.IsNullOrEmpty(baseClass))
                {
                    sb.Append(baseClass);
                    if (interfaces.Count > 0) sb.Append(", ");
                }
                sb.Append(string.Join(", ", interfaces));
            }
            sb.AppendLine();
            sb.AppendLine("{");

            foreach (var field in fields)
            {
                if (string.IsNullOrWhiteSpace(field.defaultValue))
                    sb.AppendLine($"    {field.AccessModifier} {field.Type} {field.Name};");
                else
                    sb.AppendLine($"    {field.AccessModifier} {field.Type} {field.Name} = {field.defaultValue};");
            }

            foreach (var prop in properties)
            {
                sb.AppendLine($"    {prop.AccessModifier} {prop.Type} {prop.Name}");
                sb.AppendLine("    {");
                if (prop.GetterCode != null)
                {
                    sb.AppendLine("        get");
                    sb.AppendLine("        {");
                    sb.AppendLine($"            {prop.GetterCode}");
                    sb.AppendLine("        }");
                }
                if (prop.SetterCode != null)
                {
                    sb.AppendLine("        set");
                    sb.AppendLine("        {");
                    sb.AppendLine($"            {prop.SetterCode}");
                    sb.AppendLine("        }");
                }
                sb.AppendLine("    }");
            }

            foreach (var constructor in constructors)
            {
                sb.Append($"    public {typeName}(");
                sb.Append(string.Join(", ", constructor.Parameters.ConvertAll(p => $"{p.Type} {p.Name}")));
                sb.AppendLine(")");
                sb.AppendLine("    {");
                sb.AppendLine($"        {constructor.ConstructorCode}");
                sb.AppendLine("    }");
            }

            foreach (var method in methods)
            {
                sb.Append($"    {method.AccessModifier} {method.ReturnType} {method.Name}(");
                sb.Append(string.Join(", ", method.Parameters.ConvertAll(p => $"{p.Type} {p.Name}")));
                sb.AppendLine(")");
                sb.AppendLine("    {");
                sb.AppendLine($"        {method.MethodCode}");
                sb.AppendLine("    }");
            }

            sb.AppendLine("}");

            if (!string.IsNullOrEmpty(namespaceName))
            {
                sb.AppendLine("}");
            }

            return sb;
        }

        public override string ToString()
        {
            return Build().ToString();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    fields.Clear();
                    properties.Clear();
                    methods.Clear();
                    constructors.Clear();
                    interfaces.Clear();
                    usings.Clear();
                }

                disposed = true;
            }
        }

        ~TypeBuilder()
        {
            Dispose(false);
        }
    }
}
