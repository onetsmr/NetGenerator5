using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using NetGenerator5.Generator.Dependency;

namespace NetGenerator5.Generator
{
    [Generator]
    public class SourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
//#if DEBUG
//            if (!System.Diagnostics.Debugger.IsAttached)
//            {
//                System.Diagnostics.Debugger.Launch();
//            }
//#endif
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.MSBuildProjectDirectory", out var projectDirectory) == false)
            {
                throw new ArgumentException("MSBuildProjectDirectory");
            }

            var configFile = context.AdditionalFiles.First(e => e.Path.EndsWith("generatorsettings.json")).GetText(context.CancellationToken);
            var config = Newtonsoft.Json.Linq.JObject.Parse(configFile.ToString());

            var projectPath = Path.Combine(projectDirectory, config["OutputPath"].ToString().Replace("/", "\\"));
            var modelNamespaceName = config["ModelNamespace"].ToString();
            var controllerNamespaceName = config["ControllerNamespace"].ToString();

            var modelTypes = FindTypes(context, modelNamespaceName, "ModelAttribute");
            if (modelTypes == null)
                return;

            var controllerTypes = FindTypes(context, controllerNamespaceName, "ApiControllerAttribute");
            if (controllerTypes == null)
                return;

            var generatedFilePath = Path.Combine(projectPath, "generated.js");
            var generatedFileContent = new DependencyClass().HelloWorld();

            File.WriteAllText(Path.Combine(projectPath, "generated.js"), $"// {generatedFileContent}");

            context.AddSource("generated", $"// {generatedFileContent}");
        }

        private ITypeSymbol[] FindTypes(GeneratorExecutionContext context, string namespaceName,
            string attributeFilter = null)
        {
            var @namespace = FindNamespace(context, namespaceName);
            if (@namespace == null)
                return null;

            return @namespace.GetMembers()
                .Where(e => e.GetAttributes().Any(a => a.AttributeClass.ToString().EndsWith(attributeFilter)))
                .OfType<ITypeSymbol>()
                .ToArray();
        }

        private INamespaceSymbol FindNamespace(GeneratorExecutionContext context, string namespaceName)
        {
            var namespaceNameParts = namespaceName.Split('.');

            INamespaceSymbol @namespace = context.Compilation.GlobalNamespace;

            while (@namespace.ToString() != namespaceName)
            {
                foreach (var innerNamespace in @namespace.GetNamespaceMembers())
                {
                    if (namespaceNameParts.Contains(innerNamespace.Name))
                    {
                        @namespace = innerNamespace;
                        break;
                    }
                }
            }

            return @namespace;
        }
    }
}
