using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Options;
using System.IO;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Simplification;

namespace ImportComments
{
    class Program
    {
        static void Main(string[] args)
        {

            if (args.Length != 3)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("{0} <IntelliSenseDirectory> <SourceDirectory> <SolutionFilePath>", AppDomain.CurrentDomain.FriendlyName);
                Console.WriteLine("Press ENTER to exit;");
                Console.ReadLine();
                return;
            }

            ParseIntelliSense p = new ParseIntelliSense(args[0]);

            //Check parameters
            if (!Directory.Exists(p.IntelliSenseDirectory))
            {
                Console.WriteLine($"Directory not found: {p.IntelliSenseDirectory}");
                Console.WriteLine("Press ENTER to exit;");
                Console.ReadLine();
                return;
            }            
            if (!Directory.Exists(args[1]))
            {
                Console.WriteLine($"Directory not found: {args[1]}");
                Console.WriteLine("Press ENTER to exit;");
                Console.ReadLine();
                return;
            }
            if (!File.Exists(args[2]))
            {
                Console.WriteLine($"Solution file not found: {args[2]}");
                Console.WriteLine("Press ENTER to exit;");
                Console.ReadLine();
                return;
            }
            p.ParseIntelliSenseFiles();

            var workspace = MSBuildWorkspace.Create();

            var solution = workspace.OpenSolutionAsync(args[2]).Result;

            var projects = solution.Projects.ToList();

            var metadataReferences = projects.Where(proj => !proj.Name.Contains("test")) // Filter out test projects I guess?
                                             .SelectMany(proj => proj.MetadataReferences)
                                             .Distinct(); // Does it matter if they're distinct or not?

            var project = projects.SingleOrDefault(proj => proj.FilePath.Contains(args[1]));
            if (project == null)
            {
                var path = GetPathToProject(args[1]);
                project = workspace.OpenProjectAsync(path).Result;
            }

            foreach (var document in project.Documents)
            {
                // Reads the source code from the file
                SourceText text;
                using (var stream = File.OpenRead(document.FilePath))
                {
                    text = SourceText.From(stream);
                }

                SyntaxTree tree = (SyntaxTree)CSharpSyntaxTree.ParseText(text);

                var compilation = CSharpCompilation.Create("test", syntaxTrees: new[] { tree }, 
                    references: metadataReferences);
                var rewriter = new Rewriter(compilation.GetSemanticModel(tree), p.MembersDictionary);
                var newTreeRootNode = rewriter.Visit(tree.GetRoot());
                var newTree = newTreeRootNode.SyntaxTree;

                //Checks to see if the source code was changed
                if (tree != newTree)
                {

                    var simplifiedDoc = Simplifier.ReduceAsync(document.WithSyntaxRoot(newTree.GetRoot())).Result;

                    var options = SetOptions(workspace.Options);

                    //SyntaxNode formattedNode = Formatter.Format(newTree.GetRoot(), workspace, options);
                    SyntaxNode formattedNode = Formatter.Format(simplifiedDoc.GetSyntaxRootAsync().Result, workspace, options);

                    Console.WriteLine($"Saving file: {document.FilePath}");
                    SourceText newText = formattedNode.GetText();
                    using (var writer = new StreamWriter(document.FilePath, append: false, encoding: text.Encoding))
                    {
                        newText.Write(writer);
                    }
                }

            }

            Console.WriteLine("Press ENTER to exit;");
            Console.ReadLine();
        }

        private static string GetPathToProject(string refPath)
        {
            var split = refPath.Split('\\');
            var libraryName = split[split.Length - 2];
            return $"{refPath}/{libraryName}.csproj";
        }

        private static OptionSet SetOptions(OptionSet options)
        {
            options = options.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInAccessors, true);
            options = options.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInAnonymousMethods, true);
            options = options.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInAnonymousTypes, true);
            options = options.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInControlBlocks, true);
            options = options.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInLambdaExpressionBody, false);
            options = options.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInMethods, true);
            options = options.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInObjectCollectionArrayInitializers, false);
            options = options.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInProperties, true);
            options = options.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInTypes, true);

            return options;
        }

        private static IEnumerable<string> EnumerateSourceFiles(string path) =>
            Directory.EnumerateFiles(path, "*.cs", SearchOption.AllDirectories);

    }

}