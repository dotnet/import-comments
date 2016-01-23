using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Options;
using System.IO;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Formatting;

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
                SyntaxTree tree = (SyntaxTree)CSharpSyntaxTree.ParseText(
                   File.ReadAllText(file));

                var compilation = CSharpCompilation.Create("test", syntaxTrees: new[] { tree });
                var rewriter = new Rewriter(compilation.GetSemanticModel(tree), p.MembersDictionary);
                var newTreeRootNode = rewriter.Visit(tree.GetRoot());
                var newTree = newTreeRootNode.SyntaxTree;

                //Checks to see if the source code was changed
                if (tree != newTree)
                {
                    Workspace workspace = MSBuildWorkspace.Create();
                    OptionSet options = workspace.Options;
                    options = options.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInAccessors, false);
                    options = options.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInAnonymousMethods, false);
                    options = options.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInAnonymousTypes, false);
                    options = options.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInControlBlocks, false);
                    options = options.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInLambdaExpressionBody, false);
                    options = options.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInMethods, false);
                    options = options.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInObjectCollectionArrayInitializers, false);
                    options = options.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInProperties, false);
                    options = options.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInTypes, false);

                    SyntaxNode formattedNode = Formatter.Format(newTree.GetRoot(), workspace, options);

                    // TODO: It should just overwrite the old file. Writing to a new one to test it for now
                    Console.WriteLine($"Saving file: {file}");
                    File.WriteAllText(file.Replace(".cs", "_new.cs"), formattedNode.ToFullString());
                }

            }

            Console.WriteLine("Press ENTER to exit;");
            Console.ReadLine();

        }

        private static IEnumerable<string> EnumerateSourceFiles(string path) =>
            Directory.EnumerateFiles(path, "*.cs", SearchOption.AllDirectories);

    }

}