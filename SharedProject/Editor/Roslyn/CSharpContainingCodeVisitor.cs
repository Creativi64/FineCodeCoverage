using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace FineCodeCoverage.Editor.Roslyn
{
    [Export(typeof(ICSharpCodeCoverageNodeVisitor))]
    internal class CSharpContainingCodeVisitor : CSharpSyntaxVisitor, ILanguageContainingCodeVisitor, ICSharpCodeCoverageNodeVisitor
    {
        private readonly List<SyntaxNode> _nodes = new List<SyntaxNode>();
        public List<TextSpan> GetSpans(SyntaxNode rootNode)
            => GetNodes(rootNode).ConvertAll(node => node.Span);

        public List<SyntaxNode> GetNodes(SyntaxNode rootNode)
        {
            _nodes.Clear();
            Visit(rootNode);
            return _nodes;
        }

#if VS2022
        public override void VisitFileScopedNamespaceDeclaration(FileScopedNamespaceDeclarationSyntax node)
            => VisitMembers(node.Members);
#endif
        public override void VisitCompilationUnit(CompilationUnitSyntax node) => VisitMembers(node.Members);

        public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node) => VisitMembers(node.Members);

        public override void VisitClassDeclaration(ClassDeclarationSyntax node) => VisitMembers(node.Members);

        public override void VisitStructDeclaration(StructDeclarationSyntax node) => VisitMembers(node.Members);

        public override void VisitRecordDeclaration(RecordDeclarationSyntax node) => VisitMembers(node.Members);

        public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node) => VisitMembers(node.Members);

        public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node) => AddIfHasBody(node);

        public override void VisitConversionOperatorDeclaration(ConversionOperatorDeclarationSyntax node) => AddIfHasBody(node);

        public override void VisitDestructorDeclaration(DestructorDeclarationSyntax node) => AddIfHasBody(node);

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node) => AddIfHasBody(node);

        public override void VisitOperatorDeclaration(OperatorDeclarationSyntax node) => AddIfHasBody(node);

        private static bool HasBody(BaseMethodDeclarationSyntax node)
            => node.Body != null || node.ExpressionBody != null;
        private void AddIfHasBody(BaseMethodDeclarationSyntax node)
        {
            if (!HasBody(node))
            {
                return;
            }

            AddNode(node);
        }

        public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node) => VisitBasePropertyDeclaration(node);

        public override void VisitEventDeclaration(EventDeclarationSyntax node) => VisitBasePropertyDeclaration(node);

        public override void VisitIndexerDeclaration(IndexerDeclarationSyntax node) => VisitBasePropertyDeclaration(node);

        private void VisitBasePropertyDeclaration(BasePropertyDeclarationSyntax node)
        {
            if (IsAbstract(node.Modifiers))
            {
                return;
            }

            VisitNonAbstractBasePropertyDeclaration(node);
        }

        private void AddIfPropertyDeclaration(BasePropertyDeclarationSyntax node)
        {
            if (!(node is PropertyDeclarationSyntax propertyDeclarationSyntax))
            {
                return;
            }

            AddNode(propertyDeclarationSyntax);
        }

        private void VisitNonAbstractBasePropertyDeclaration(BasePropertyDeclarationSyntax node)
        {
            if (node.AccessorList == null)
            {
                AddIfPropertyDeclaration(node);
            }
            else
            {
                AddAccessors(node.AccessorList.Accessors, node.Parent is InterfaceDeclarationSyntax);
            }
        }

        private void AddAccessors(SyntaxList<AccessorDeclarationSyntax> accessors, bool typeIsInterface)
            => accessors.Where(accessor => !typeIsInterface || AccessorHasBody(accessor)).ToList().ForEach(AddNode);

        private static bool AccessorHasBody(AccessorDeclarationSyntax accessor)
            => accessor.Body != null || accessor.ExpressionBody != null;

        private void VisitMembers(SyntaxList<MemberDeclarationSyntax> members)
        {
            foreach (MemberDeclarationSyntax member in members)
            {
                Visit(member);
            }
        }

        private static bool IsAbstract(SyntaxTokenList modifiers)
            => modifiers.Any(modifier => modifier.IsKind(SyntaxKind.AbstractKeyword));

        private void AddNode(SyntaxNode node) => _nodes.Add(node);
    }
}
