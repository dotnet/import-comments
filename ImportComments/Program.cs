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
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
                SourceText text;
                using (var stream = File.OpenRead(document.FilePath))
                {
                    text = SourceText.From(stream);
                }

                SyntaxTree initialTree = (SyntaxTree)CSharpSyntaxTree.ParseText(text);

                var compilation = CSharpCompilation.Create(
                    "test", syntaxTrees: new[] { initialTree }, references: metadataReferences);

                var rewriter = new Rewriter(compilation.GetSemanticModel(initialTree), p.MembersDictionary);
                
                var treeWithFormattedTrivia = rewriter.Visit(initialTree.GetRoot()).SyntaxTree;

                var options = SetOptions(workspace.Options);

                var simplifiedTree = Simplifier.ReduceAsync(document.WithSyntaxRoot(treeWithFormattedTrivia.GetRoot()), options).Result
                                    .GetSyntaxTreeAsync().Result;

                var simplifiedTriviaLookup = GetSimplifiedCommentLookup(simplifiedTree, metadataReferences);

                var rw = new MoveCommentsRewriter(compilation.GetSemanticModel(initialTree), simplifiedTree.GetRoot(), simplifiedTriviaLookup);

                var finalTree = rw.Visit(initialTree.GetRoot()).SyntaxTree;

                if (initialTree != finalTree)
                {
                    // Need to call format here because comments are inserted at the 0th column when rewriting the syntax tree.
                    var formattedRootNode = Formatter.Format(finalTree.GetRoot(), workspace, options);

                    Console.WriteLine($"Saving file: {document.FilePath}");
                    SourceText newText = formattedRootNode.GetText();
                    using (var writer = new StreamWriter(document.FilePath, append: false, encoding: text.Encoding))
                    {
                        newText.Write(writer);
                    }
                }
            }

            Console.WriteLine("Press ENTER to exit;");
            Console.ReadLine();
        }

        private static Dictionary<string, SyntaxTriviaList> GetSimplifiedCommentLookup(SyntaxTree simplifiedTree, IEnumerable<MetadataReference> metadataReferences)
        {
            var compilation = CSharpCompilation.Create(
                assemblyName: "simplifiedTest", syntaxTrees: new[] { simplifiedTree }, references: metadataReferences);

            var model = compilation.GetSemanticModel(simplifiedTree);

            return GetSimplifiedCommentLookupImpl(simplifiedTree.GetRoot(), model, new Dictionary<string, SyntaxTriviaList>());
        }

        private static Dictionary<string, SyntaxTriviaList> GetSimplifiedCommentLookupImpl(SyntaxNode node, SemanticModel model, Dictionary<string, SyntaxTriviaList> lookup)
        {
            Func<SyntaxNode, bool> isCorrectSyntaxType = n =>
            {
                return n is ClassDeclarationSyntax ||
                       n is MethodDeclarationSyntax ||
                       n is ConstructorDeclarationSyntax ||
                       n is DelegateDeclarationSyntax ||
                       n is ConversionOperatorDeclarationSyntax ||
                       n is DestructorDeclarationSyntax ||
                       n is EnumDeclarationSyntax ||
                       n is EventDeclarationSyntax ||
                       n is EventFieldDeclarationSyntax ||
                       n is FieldDeclarationSyntax ||
                       n is IndexerDeclarationSyntax ||
                       n is InterfaceDeclarationSyntax ||
                       n is OperatorDeclarationSyntax ||
                       n is PropertyDeclarationSyntax ||
                       n is StructDeclarationSyntax ||
                       n is EnumMemberDeclarationSyntax;
            };

            foreach (var child in node.ChildNodes())
            {
                if (child.HasLeadingTrivia)
                {
                    lookup = GetSimplifiedCommentLookupImpl(child, model, lookup);

                    if (!isCorrectSyntaxType(child))
                    {
                        continue;
                    }

                    // A pattern matching expression would be nice here.
                    ISymbol symbol = null;
                    if (child is EventFieldDeclarationSyntax)
                    {
                        var c = child as EventFieldDeclarationSyntax;
                        symbol = model.GetDeclaredSymbol(c.Declaration.Variables.First());
                    }
                    else if (child is FieldDeclarationSyntax)
                    {
                        var c = child as FieldDeclarationSyntax;
                        symbol = model.GetDeclaredSymbol(c.Declaration.Variables.First());
                    }
                    else
                    {
                        symbol = model.GetDeclaredSymbol(child);
                    }

                    var commentId = symbol.GetDocumentationCommentId();

                    lookup.Add(commentId, child.GetLeadingTrivia());
                }
            }

            return lookup;
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

            options = options.WithChangedOption(SimplificationOptions.PreferAliasToQualification, false);
            options = options.WithChangedOption(SimplificationOptions.PreferOmittingModuleNamesInQualification, false);
            options = options.WithChangedOption(SimplificationOptions.PreferImplicitTypeInference, false);
            options = options.WithChangedOption(SimplificationOptions.PreferImplicitTypeInLocalDeclaration, false);
            options = options.WithChangedOption(SimplificationOptions.AllowSimplificationToGenericType, true);
            options = options.WithChangedOption(SimplificationOptions.AllowSimplificationToBaseType, false);
            options = options.WithChangedOption(SimplificationOptions.QualifyMemberAccessWithThisOrMe, LanguageNames.CSharp, true);
            options = options.WithChangedOption(SimplificationOptions.PreferIntrinsicPredefinedTypeKeywordInDeclaration, LanguageNames.CSharp, false);
            options = options.WithChangedOption(SimplificationOptions.PreferIntrinsicPredefinedTypeKeywordInMemberAccess, LanguageNames.CSharp, false);

            return options;
        }

        private static IEnumerable<string> EnumerateSourceFiles(string path) =>
            Directory.EnumerateFiles(path, "*.cs", SearchOption.AllDirectories);

    }

}