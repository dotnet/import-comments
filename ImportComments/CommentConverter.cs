using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ImportComments
{
    class CommentConverter
    {

        private CSharpCompilation m_compilation;
        public CommentConverter()
        { }

        // Given a syntax tree from a compilation unit, add/convert the comments
        public CSharpCompilation ConvertCompilation(CSharpCompilation compilation)
        {
            m_compilation = compilation;
            var trees = compilation.SyntaxTrees;

            // Loop over the trees
            foreach (SyntaxTree tree in trees)
            {
                SyntaxTree newTree = ConvertTree(tree);
                compilation = compilation.AddSyntaxTrees(newTree);
            }
            return compilation;
        }

        private SyntaxTree ConvertTree(SyntaxTree tree)
        {
            SyntaxNode node = tree.GetRoot();
            node = TraverseAndConvert(node, node);
            return node.SyntaxTree;
        }

        

        // Returns the name of the declared API if the node is the type of node that should have an XML doc comment
        // otherwise returns null;
        public string GetAPIForNode(SyntaxNode node)
        {
            switch (node.Kind())
            {
                case SyntaxKind.ConstructorDeclaration:
                    {
                        ConstructorDeclarationSyntax syntax = (ConstructorDeclarationSyntax)node;
                        // assume only one field/identifier
                        string text = syntax.Identifier.Text;
                        // for now assume only one match
                        if (m_compilation.ContainsSymbolsWithName(symbolName => symbolName == text))
                        {
                            var symbols = m_compilation.GetSymbolsWithName(symbolName => symbolName == text);
                            // but which one is the right one?
                            var symbol = symbols.Single();
                            return symbol.GetDocumentationCommentId();
                        }
                        return null;
                    }
                case SyntaxKind.ConversionOperatorDeclaration:
                    {
                        ConversionOperatorDeclarationSyntax syntax = (ConversionOperatorDeclarationSyntax)node;
                        // assume only one field/identifier
                        string text = syntax.OperatorKeyword.Text;
                        // for now assume only one match
                        if (m_compilation.ContainsSymbolsWithName(symbolName => symbolName == text))
                        {
                            var symbols = m_compilation.GetSymbolsWithName(symbolName => symbolName == text);
                            // but which one is the right one?
                            var symbol = symbols.Single();
                            return symbol.GetDocumentationCommentId();
                        }
                        return null;
                    }
                case SyntaxKind.DelegateDeclaration:
                    {
                        DelegateDeclarationSyntax syntax = (DelegateDeclarationSyntax)node;
                        // assume only one field/identifier
                        string text = syntax.Identifier.Text;
                        // for now assume only one match
                        if (m_compilation.ContainsSymbolsWithName(symbolName => symbolName == text))
                        {
                            var symbols = m_compilation.GetSymbolsWithName(symbolName => symbolName == text);
                            // but which one is the right one?
                            var symbol = symbols.Single();
                            return symbol.GetDocumentationCommentId();
                        }
                        return null;
                    }
                case SyntaxKind.EnumDeclaration:
                    {
                        EnumDeclarationSyntax syntax = (EnumDeclarationSyntax)node;
                        // assume only one field/identifier
                        string text = syntax.Identifier.Text;
                        // for now assume only one match
                        if (m_compilation.ContainsSymbolsWithName(symbolName => symbolName == text))
                        {
                            var symbols = m_compilation.GetSymbolsWithName(symbolName => symbolName == text);
                            // but which one is the right one?
                            var symbol = symbols.Single();
                            return symbol.GetDocumentationCommentId();
                        }
                        return null;
                    }
                case SyntaxKind.EventDeclaration:
                    {
                       /* EventDeclarationSyntax syntax = (EventDeclarationSyntax)node;

                        string text = syntax.Identifier.Text;
                        // for now assume only one match
                        if (m_compilation.ContainsSymbolsWithName(symbolName => symbolName == text))
                        {
                            var symbols = m_compilation.GetSymbolsWithName(symbolName => symbolName == text);
                            // but which one is the right one?
                            var symbol = symbols.Single();
                            return symbol.GetDocumentationCommentId();
                        }*/
                        return null;
                    }
                case SyntaxKind.FieldDeclaration:
                    {
                        FieldDeclarationSyntax syntax = (FieldDeclarationSyntax)node;
                        // assume only one field/identifier
                        string text = syntax.Declaration.Variables.First().Identifier.Text;
                        // for now assume only one match
                        if (m_compilation.ContainsSymbolsWithName(symbolName => symbolName == text))
                        {
                            var symbols = m_compilation.GetSymbolsWithName(symbolName => symbolName == text);
                            // but which one is the right one?
                            var symbol = symbols.Single();
                            return symbol.GetDocumentationCommentId();
                        }
                        return null;
                    }
                case SyntaxKind.IndexerDeclaration:
                    {
                        IndexerDeclarationSyntax syntax = (IndexerDeclarationSyntax)node;
                        string text = syntax.ThisKeyword.Text;
                        // for now assume only one match
                        if (m_compilation.ContainsSymbolsWithName(symbolName => symbolName == text))
                        {
                            var symbols = m_compilation.GetSymbolsWithName(symbolName => symbolName == text);
                            // but which one is the right one?
                            var symbol = symbols.Single();
                            return symbol.GetDocumentationCommentId();
                        }
                        return null;
                    }
                case SyntaxKind.InterfaceDeclaration:
                    {
                        InterfaceDeclarationSyntax syntax = (InterfaceDeclarationSyntax)node;
                        string text = syntax.Identifier.Text;
                        // for now assume only one match
                        if (m_compilation.ContainsSymbolsWithName(symbolName => symbolName == text))
                        {
                            var symbols = m_compilation.GetSymbolsWithName(symbolName => symbolName == text);
                            // but which one is the right one?
                            var symbol = symbols.Single();
                            return symbol.GetDocumentationCommentId();
                        }
                        return null;
                    }
                case SyntaxKind.MethodDeclaration:
                    {
                        MethodDeclarationSyntax methodDeclarationSyntax = (MethodDeclarationSyntax)node;
                        string text = methodDeclarationSyntax.Identifier.Text;
                        var parameters = methodDeclarationSyntax.ParameterList.Parameters;
                        
                        if (m_compilation.ContainsSymbolsWithName(symbolName => symbolName == text))
                        {
                            var symbols = m_compilation.GetSymbolsWithName(symbolName => symbolName == text);

                            // select the symbol whose declaring syntax reference matches the node's syntax reference.
                            //var symbol = symbols.Where(s => s.DeclaringSyntaxReferences.Contains(node.GetReference())).Single();

                            foreach (var symbol in symbols)
                            {
                                var references = symbol.DeclaringSyntaxReferences;
                                foreach (var reference in references)
                                {
                                    SyntaxNode testNode = reference.GetSyntax();
                                    if (testNode.Equals(node))
                                    {
                                        Console.WriteLine("Matched nodes.");
                                    }
                                }
                            }

                            // find the one that corresponds to this syntax node.
                            //foreach (var symbol in symbols)
                            //{
                           //     IMethodSymbol methodsymbol = (IMethodSymbol)symbol;
                           //     symbol.DeclaringSyntaxReferences.Select(syntaxReference => syntaxReference == node.GetReference());
                            //    
                            //}
                            return symbols.First().GetDocumentationCommentId();
                        }
                        return null;
                    }
                case SyntaxKind.NamespaceDeclaration: // doesn't work
                    {
                        NamespaceDeclarationSyntax syntax = (NamespaceDeclarationSyntax)node;
                        NameSyntax nameSyntax = syntax.Name;
                        string text = nameSyntax.ToFullString();
                        // for now assume only one match
                        if (m_compilation.ContainsSymbolsWithName(symbolName => symbolName == text))
                        {
                            var symbols = m_compilation.GetSymbolsWithName(symbolName => symbolName == text);
                            // but which one is the right one?
                            var symbol = symbols.Single();
                            return symbol.GetDocumentationCommentId();
                        }
                        return null;
                    }
                case SyntaxKind.OperatorDeclaration:
                    {
                        OperatorDeclarationSyntax syntax = (OperatorDeclarationSyntax)node;
                        // this won't work, it needs to be figured out.
                        string text = syntax.OperatorKeyword.Text + syntax.OperatorToken.Text;
                        // for now assume only one match
                        if (m_compilation.ContainsSymbolsWithName(symbolName => symbolName == text))
                        {
                            var symbols = m_compilation.GetSymbolsWithName(symbolName => symbolName == text);
                            // but which one is the right one?
                            var symbol = symbols.Single();
                            return symbol.GetDocumentationCommentId();
                        }
                        return null;
                    }
                case SyntaxKind.PropertyDeclaration:
                    {
                        PropertyDeclarationSyntax syntax = (PropertyDeclarationSyntax)node;
                        string text = syntax.Identifier.Text;
                        // for now assume only one match
                        if (m_compilation.ContainsSymbolsWithName(symbolName => symbolName == text))
                        {
                            var symbols = m_compilation.GetSymbolsWithName(symbolName => symbolName == text);
                            // but which one is the right one?
                            var symbol = symbols.Single();
                            return symbol.GetDocumentationCommentId();
                        }
                        return null;
                    }
                case SyntaxKind.ClassDeclaration:
                    {
                        Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax classSyntax = (ClassDeclarationSyntax)node;
                        string text = classSyntax.Identifier.Text;
                        string valueText = classSyntax.Identifier.ValueText;
                        // for now assume only one match
                        var symbols = m_compilation.GetSymbolsWithName(symbolName => symbolName == text);
                        // but which one is the right one?
                        var symbol = symbols.Single();
                        return symbol.GetDocumentationCommentId();

                    }
                case SyntaxKind.StructDeclaration:
                    {
                        Microsoft.CodeAnalysis.CSharp.Syntax.StructDeclarationSyntax structSyntax = (StructDeclarationSyntax)node;
                        string text = structSyntax.Identifier.Text;
                        string valueText = structSyntax.Identifier.ValueText;

                        // for now assume only one match
                        var symbols = m_compilation.GetSymbolsWithName(symbolName => symbolName == text);
                        // but which one is the right one?
                        var symbol = symbols.Single();
                        return symbol.GetDocumentationCommentId();
                    }
                    
                default:
                    return null;
                }
            
                
                //var types = m_compilation.GlobalNamespace.GetTypeMembers(name);
                //m_compilation.GetTypeByMetadataName()
                //string docCommentId = classSymbol.GetDocumentationCommentId();
                return "";
            
        }

        // Given a SyntaxNode for an API node,
        // look up the doc comment and return it as a string.
        public static string GetDocCommentForId(string id)
        {
            return "/// <member name=\"" + id + "\">\n" +
                   "/// <summary> summary text </summary>\n" +
                   "/// </member>\n";

            // find the XML doc text from the doc comment id
            // This needs to come from the DDUEML or IntelliSense
            
        }

        public SyntaxNode TraverseAndConvert(SyntaxNode node, SyntaxNode newNode)
        {

            // Step 1: Handle current node
            // Find out if this node is a documentable API declaration
            // If not, skip to go to the child nodes.
            string docCommentId = GetAPIForNode(node);
            if (docCommentId != null)
            
            {
                // Look up the comment text
                string docCommentText = GetDocCommentForId(docCommentId);

                // Get the SyntaxTrivia for the comment
                SyntaxTree newTree = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(docCommentText);
                var newTrivia = newTree.GetRoot().GetLeadingTrivia();
                // Read a doc comment from a syntax tree.
                //var classNode = (ClassDeclarationSyntax)newTree.GetRoot().ChildNodes().First();
                //var newTrivia = classNode.GetLeadingTrivia();
                //var docCommentTrivia = newTrivia.Single(t => t.Kind() == SyntaxKind.SingleLineDocumentationCommentTrivia ||
                //                                       t.Kind() == SyntaxKind.MultiLineDocumentationCommentTrivia);

            // Find out if there is an existing comment or doc comment
            if (node.HasLeadingTrivia)
                {
                    SyntaxTriviaList triviaList = node.GetLeadingTrivia();
                    SyntaxTrivia firstComment = triviaList.Last();
                    foreach (var trivia in triviaList.Reverse())
                    {
                        SyntaxKind kind = trivia.Kind();
                        
                        switch (kind)
                        {
                            case SyntaxKind.SingleLineCommentTrivia:
                            case SyntaxKind.MultiLineCommentTrivia:
                                // Found existing comment
                                firstComment = trivia;
                                break;
                            case SyntaxKind.MultiLineDocumentationCommentTrivia:
                            case SyntaxKind.SingleLineDocumentationCommentTrivia:
                                // Found existing XML doc comment
                                firstComment = trivia;
                                break;
                            default:
                                break;
                        }

                    }


                    // Append the doc comment
                    newNode = node.InsertTriviaBefore(firstComment, newTrivia);
                }
                else // no leading trivia
                {
                    newNode = node.WithLeadingTrivia(newTrivia);
                }
            }
            else // not an API node
            {
                newNode = node;
            }

            if (node.ChildNodes().Count() > 0)
            {
                newNode = newNode.ReplaceNodes(newNode.ChildNodes(), TraverseAndConvert);
            }
            return newNode;
        }

    }
}
