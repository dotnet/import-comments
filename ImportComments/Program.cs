using Microsoft.CodeAnalysis.CSharp;
using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using System.IO;
using System.Collections.Generic;

namespace ImportComments
{
    class Program
    {
        static void Main(string[] args)
        {

            if (args.Length != 2)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("{0} <IntelliSenseDirectory> <SourceDirectory>", AppDomain.CurrentDomain.FriendlyName);
                Console.ReadLine();
                return;
            }

            ParseIntelliSense p = new ParseIntelliSense(args[0]);

            //Check parameters
            if (!Directory.Exists(p.IntelliSenseDirectory))
            {
                Console.WriteLine($"Directory not found: {p.IntelliSenseDirectory}");
                Console.ReadLine();
                return;
            }            
            if (!Directory.Exists(args[1]))
            {
                Console.WriteLine($"Directory not found: {args[1]}");
                Console.ReadLine();
                return;
            }
            p.ParseIntelliSenseFiles();

            foreach (var file in EnumerateSourceFiles(args[1]))
            {
                Console.WriteLine(file);

                SyntaxTree tree = (SyntaxTree)CSharpSyntaxTree.ParseText(
                   File.ReadAllText(file));

                var compilation = Microsoft.CodeAnalysis.CSharp.CSharpCompilation.Create("test", syntaxTrees: new[] { tree });
                var rewriter = new Rewriter(compilation.GetSemanticModel(tree), p.MembersDictionary);
                var newTreeRootNode = rewriter.Visit(tree.GetRoot());
                var newTree = newTreeRootNode.SyntaxTree;

                Workspace workspace = MSBuildWorkspace.Create();
                SyntaxNode formattedNode = Microsoft.CodeAnalysis.Formatting.Formatter.Format(newTree.GetRoot(), workspace);

                // TO-DO: It should just overwrite the old file. Writing to a new one to test it for now
                File.WriteAllText(file.Replace(".cs","_new.cs"), formattedNode.ToFullString());
            }

            Console.WriteLine("Press ENTER to exit;");
            Console.ReadLine();

        }

        private static IEnumerable<string> EnumerateSourceFiles(string path) =>
            Directory.EnumerateFiles(path, "*.cs", SearchOption.AllDirectories);

    }

}