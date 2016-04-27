using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportComments
{
    public class MoveCommentsRewriter : CSharpSyntaxRewriter
    {
        private SemanticModel _semanticModel;
        private SyntaxNode _simplifiedRoot;
        private Dictionary<string, SyntaxTriviaList> _lookup;

        public MoveCommentsRewriter(SemanticModel model, SyntaxNode simplifiedRoot, Dictionary<string, SyntaxTriviaList> lookup)
        {
            _semanticModel = model;
            _simplifiedRoot = simplifiedRoot;
            _lookup = lookup;
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            if (node == null)
                return null;
            var symbol = _semanticModel.GetDeclaredSymbol(node);
            node = (ClassDeclarationSyntax)base.VisitClassDeclaration(node);
            if (!IsPrivateOrInternal(symbol.DeclaredAccessibility))
                node = (ClassDeclarationSyntax)ApplyDocComment(node, symbol.GetDocumentationCommentId());
            return node;
        }

        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            if (node == null)
                return null;
            var symbol = _semanticModel.GetDeclaredSymbol(node);
            node = (MethodDeclarationSyntax)base.VisitMethodDeclaration(node);
            if (!IsPrivateOrInternal(symbol.DeclaredAccessibility))
                node = (MethodDeclarationSyntax)ApplyDocComment(node, symbol.GetDocumentationCommentId());
            return node;
        }

        public override SyntaxNode VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            if (node == null)
                return null;
            var symbol = _semanticModel.GetDeclaredSymbol(node);
            node = (ConstructorDeclarationSyntax)base.VisitConstructorDeclaration(node);
            if (!IsPrivateOrInternal(symbol.DeclaredAccessibility))
                node = (ConstructorDeclarationSyntax)ApplyDocComment(node, symbol.GetDocumentationCommentId());
            return node;
        }

        public override SyntaxNode VisitDelegateDeclaration(DelegateDeclarationSyntax node)
        {
            if (node == null)
                return null;
            var symbol = _semanticModel.GetDeclaredSymbol(node);
            node = (DelegateDeclarationSyntax)base.VisitDelegateDeclaration(node);
            if (!IsPrivateOrInternal(symbol.DeclaredAccessibility))
                node = (DelegateDeclarationSyntax)ApplyDocComment(node, symbol.GetDocumentationCommentId());
            return node;
        }

        public override SyntaxNode VisitConversionOperatorDeclaration(ConversionOperatorDeclarationSyntax node)
        {
            if (node == null)
                return null;
            var symbol = _semanticModel.GetDeclaredSymbol(node);
            node = (ConversionOperatorDeclarationSyntax)base.VisitConversionOperatorDeclaration(node);
            if (!IsPrivateOrInternal(symbol.DeclaredAccessibility))
                node = (ConversionOperatorDeclarationSyntax)ApplyDocComment(node, symbol.GetDocumentationCommentId());
            return node;
        }

        public override SyntaxNode VisitDestructorDeclaration(DestructorDeclarationSyntax node)
        {
            if (node == null)
                return null;
            var symbol = _semanticModel.GetDeclaredSymbol(node);
            node = (DestructorDeclarationSyntax)base.VisitDestructorDeclaration(node);
            if (!IsPrivateOrInternal(symbol.DeclaredAccessibility))
                node = (DestructorDeclarationSyntax)ApplyDocComment(node, symbol.GetDocumentationCommentId());
            return node;
        }

        public override SyntaxNode VisitEnumDeclaration(EnumDeclarationSyntax node)
        {
            if (node == null)
                return null;
            var symbol = _semanticModel.GetDeclaredSymbol(node);
            node = (EnumDeclarationSyntax)base.VisitEnumDeclaration(node);
            if (!IsPrivateOrInternal(symbol.DeclaredAccessibility))
                node = (EnumDeclarationSyntax)ApplyDocComment(node, symbol.GetDocumentationCommentId());
            return node;
        }

        public override SyntaxNode VisitEventDeclaration(EventDeclarationSyntax node)
        {
            if (node == null)
                return null;
            var symbol = _semanticModel.GetDeclaredSymbol(node);
            node = (EventDeclarationSyntax)base.VisitEventDeclaration(node);
            if (!IsPrivateOrInternal(symbol.DeclaredAccessibility))
                node = (EventDeclarationSyntax)ApplyDocComment(node, symbol.GetDocumentationCommentId());
            return node;
        }

        public override SyntaxNode VisitEventFieldDeclaration(EventFieldDeclarationSyntax node)
        {
            if (node == null)
                return null;
            //var symbol = _semanticModel.GetDeclaredSymbol(node);
            var symbol = _semanticModel.GetDeclaredSymbol(node.Declaration.Variables.First());
            node = (EventFieldDeclarationSyntax)base.VisitEventFieldDeclaration(node);
            if (!IsPrivateOrInternal(symbol.DeclaredAccessibility))
                node = (EventFieldDeclarationSyntax)ApplyDocComment(node, symbol.GetDocumentationCommentId());
            return node;
        }

        public override SyntaxNode VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            if (node == null)
                return null;
            var symbol = _semanticModel.GetDeclaredSymbol(node.Declaration.Variables.First());
            node = (FieldDeclarationSyntax)base.VisitFieldDeclaration(node);
            if (!IsPrivateOrInternal(symbol.DeclaredAccessibility))
                node = (FieldDeclarationSyntax)ApplyDocComment(node, symbol.GetDocumentationCommentId());
            return node;
        }

        public override SyntaxNode VisitIndexerDeclaration(IndexerDeclarationSyntax node)
        {
            if (node == null)
                return null;
            var symbol = _semanticModel.GetDeclaredSymbol(node);
            node = (IndexerDeclarationSyntax)base.VisitIndexerDeclaration(node);
            if (!IsPrivateOrInternal(symbol.DeclaredAccessibility))
                node = (IndexerDeclarationSyntax)ApplyDocComment(node, symbol.GetDocumentationCommentId());
            return node;
        }

        public override SyntaxNode VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
        {
            if (node == null)
                return null;
            var symbol = _semanticModel.GetDeclaredSymbol(node);
            node = (InterfaceDeclarationSyntax)base.VisitInterfaceDeclaration(node);
            if (!IsPrivateOrInternal(symbol.DeclaredAccessibility))
                node = (InterfaceDeclarationSyntax)ApplyDocComment(node, symbol.GetDocumentationCommentId());
            return node;
        }

        public override SyntaxNode VisitOperatorDeclaration(OperatorDeclarationSyntax node)
        {
            if (node == null)
                return null;
            var symbol = _semanticModel.GetDeclaredSymbol(node);
            node = (OperatorDeclarationSyntax)base.VisitOperatorDeclaration(node);
            if (!IsPrivateOrInternal(symbol.DeclaredAccessibility))
                node = (OperatorDeclarationSyntax)ApplyDocComment(node, symbol.GetDocumentationCommentId());
            return node;
        }

        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            if (node == null)
                return null;
            var symbol = _semanticModel.GetDeclaredSymbol(node);
            node = (PropertyDeclarationSyntax)base.VisitPropertyDeclaration(node);
            if (!IsPrivateOrInternal(symbol.DeclaredAccessibility))
                node = (PropertyDeclarationSyntax)ApplyDocComment(node, symbol.GetDocumentationCommentId());
            return node;
        }

        public override SyntaxNode VisitStructDeclaration(StructDeclarationSyntax node)
        {
            if (node == null)
                return null;
            var symbol = _semanticModel.GetDeclaredSymbol(node);
            node = (StructDeclarationSyntax)base.VisitStructDeclaration(node);
            if (!IsPrivateOrInternal(symbol.DeclaredAccessibility))
                node = (StructDeclarationSyntax)ApplyDocComment(node, symbol.GetDocumentationCommentId());
            return node;
        }

        public override SyntaxNode VisitEnumMemberDeclaration(EnumMemberDeclarationSyntax node)
        {
            if (node == null)
                return null;
            var symbol = _semanticModel.GetDeclaredSymbol(node);
            node = (EnumMemberDeclarationSyntax)base.VisitEnumMemberDeclaration(node);
            if (!IsPrivateOrInternal(symbol.DeclaredAccessibility))
                node = (EnumMemberDeclarationSyntax)ApplyDocComment(node, symbol.GetDocumentationCommentId());
            return node;
        }

        private SyntaxNode ApplyDocComment(SyntaxNode node, string docCommentId)
        {
            if (!_lookup.ContainsKey(docCommentId))
            {
                return node;
            }

            var simplifiedTrivia = _lookup[docCommentId];
            return node.WithLeadingTrivia(simplifiedTrivia);
        }

        private bool IsPrivateOrInternal(Accessibility enumValue)
        {
            return new[] { Accessibility.Private, Accessibility.Internal }.Contains(enumValue);
        }
    }
}
