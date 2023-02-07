using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
//using Microsoft.CodeAnalysis.CSharp.Workspaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SimplifyWorkingTree
{
    public static class RoslynExtensions
    {

        //arpit comment. 
        public static Solution GetSolution(this SyntaxNodeAnalysisContext context)
        {
            var workspace = context.Options.GetPrivatePropertyValue<object>("Workspace");
            return workspace.GetPrivatePropertyValue<Solution>("CurrentSolution");
        }

        public static T GetPrivatePropertyValue<T>(this object obj, string propName)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            var pi = obj.GetType().GetRuntimeProperty(propName);

            if (pi == null)
            {
                throw new ArgumentOutOfRangeException(nameof(propName), $"Property {propName} was not found in Type {obj.GetType().FullName}");
            }

            return (T)pi.GetValue(obj, null);
        }
    }

    class Program
    {
        public async Task<List<Location>> GetUnusedLocalVariables(Solution solution)
        {
            List<Location> locations = new List<Location>();
            foreach (var pjt in solution.Projects)
            {
                var compilation = await pjt.GetCompilationAsync();
                foreach (var tree in compilation.SyntaxTrees)
                {
                    var methods = tree.GetRoot().DescendantNodesAndSelf().OfType<MethodDeclarationSyntax>().Where(x => x.IsKind(SyntaxKind.MethodDeclaration));
                    foreach (var method in methods)
                    {
                        if (method.Body == null) continue;
                        var dataFlow = compilation.GetSemanticModel(tree).AnalyzeDataFlow(method.Body);
                        var variablesDeclared = dataFlow.VariablesDeclared.Where(x => x.Kind.ToString() == "Local");
                        var variablesRead = dataFlow.ReadInside.Union(dataFlow.ReadOutside);
                        var unused = variablesDeclared.Except(variablesRead);
                        if (unused.Any())
                        {
                            foreach (var unusedVar in unused)
                            {
                                var foreachStatements = method.DescendantNodes().OfType<ForEachStatementSyntax>().Where(x => x.Identifier.Text == unusedVar.Name).ToList();
                                if (foreachStatements?.Count > 0) continue;
                                locations.Add(unusedVar.Locations.First());
                            }
                        }
                    }
                }
            }
            return locations;
        }
        //public async Task<List<Location>> GetUnusedLocalVariables(string filePath)
        //{
        //    List<Location> locations = new List<Location>();

        //    string sFilePath = Path.GetFullPath(filePath);
        //    string text = File.ReadAllText(sFilePath);
        //    SyntaxTree tree = CSharpSyntaxTree.ParseText(text);

        //    Compilation compilation = 

        //    var methods = tree.GetRoot().DescendantNodesAndSelf().OfType<MethodDeclarationSyntax>().Where(x => x.IsKind(SyntaxKind.MethodDeclaration));
        //    foreach (var method in methods)
        //    {
        //        if (method.Body == null) continue;
        //        var dataFlow = compilation.GetSemanticModel(tree).AnalyzeDataFlow(method.Body);
        //        var variablesDeclared = dataFlow.VariablesDeclared.Where(x => x.Kind.ToString() == "Local");
        //        var variablesRead = dataFlow.ReadInside.Union(dataFlow.ReadOutside);
        //        var unused = variablesDeclared.Except(variablesRead);
        //        if (unused.Any())
        //        {
        //            foreach (var unusedVar in unused)
        //            {
        //                var foreachStatements = method.DescendantNodes().OfType<ForEachStatementSyntax>().Where(x => x.Identifier.Text == unusedVar.Name).ToList();
        //                if (foreachStatements?.Count > 0) continue;
        //                locations.Add(unusedVar.Locations.First());
        //            }
        //        }
        //    }
        //    return locations;
        //}

        static void Main(string[] args)
        {
            string sCurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;

            string sFile = System.IO.Path.Combine(sCurrentDirectory, "../../../../ReduceTests/ReduceTests.cs");

            string sFilePath = Path.GetFullPath(sFile);
            string text = File.ReadAllText(sFilePath);
            SyntaxTree tree = CSharpSyntaxTree.ParseText(text);

            var cu = tree.GetCompilationUnitRoot();
            CompilationUnitSyntax input = tree.GetCompilationUnitRoot();
            var nameSpaceOriginal = ((NamespaceDeclarationSyntax)input.Members[0]);
            var classOriginal = (ClassDeclarationSyntax)nameSpaceOriginal.Members[0];
            var methodOriginal = (MethodDeclarationSyntax)classOriginal.Members[0];
            var blockX = (BlockSyntax)methodOriginal.Body;
            var tempMethod = methodOriginal;
            var x = tempMethod.Body.RemoveNode(tempMethod.Body.Statements[0], SyntaxRemoveOptions.KeepNoTrivia);


            //var dataFlow = compilation.GetSemanticModel(tree).AnalyzeDataFlow(tempMethod.Body);
            //var variablesDeclared = dataFlow.VariablesDeclared.Where(x => x.Kind.ToString() == "Local");
            //var variablesRead = dataFlow.ReadInside.Union(dataFlow.ReadOutside);
            //var unused = variablesDeclared.Except(variablesRead);

            //foreach (int nodeId in tempMethod.Body.Statements.Count())
            //{
            //    tempMethod.Body.Statements[nodeId];
            //}


            //Solution solution = new Solution()

            //GetUnusedLocalVariables




            tempMethod = methodOriginal.WithBody(x);
            var newClass = classOriginal.ReplaceNode(methodOriginal, tempMethod);
            var output = input.ReplaceNode(classOriginal, newClass);
            System.Diagnostics.Debug.WriteLine(output.ToString());
            System.Console.WriteLine(output.ToString());
            System.Console.ReadLine();
        }
    }
}
