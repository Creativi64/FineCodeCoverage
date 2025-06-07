using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace FineCodeCoverage.Editor.Roslyn
{
    internal sealed class VBContainingCodeVisitor : VisualBasicSyntaxVisitor, ILanguageContainingCodeVisitor
    {
        private readonly List<TextSpan> _spans = new List<TextSpan>();

        public List<TextSpan> GetSpans(SyntaxNode rootNode)
        {
            Visit(rootNode);
            return _spans;
        }

        public override void VisitCompilationUnit(CompilationUnitSyntax node) => VisitMembers(node.Members);

        public override void VisitNamespaceBlock(NamespaceBlockSyntax node) => VisitMembers(node.Members);

        private void VisitMembers(SyntaxList<StatementSyntax> members)
        {
            foreach (StatementSyntax member in members)
            {
                Visit(member);
            }
        }

        public override void VisitClassBlock(ClassBlockSyntax node) => VisitMembers(node.Members);

        public override void VisitStructureBlock(StructureBlockSyntax node) => VisitMembers(node.Members);

        public override void VisitModuleBlock(ModuleBlockSyntax node) => VisitMembers(node.Members);

        public override void VisitConstructorBlock(ConstructorBlockSyntax node) => AddNode(node);

        public override void VisitMethodBlock(MethodBlockSyntax node)
        {
            if (IsPartial(node.SubOrFunctionStatement.Modifiers))
            {
                return;
            }

            AddNode(node);
        }

        private static bool IsPartial(SyntaxTokenList modifiers)
            => modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PartialKeyword));

        private static bool IsAbstract(SyntaxTokenList modifiers)
            => modifiers.Any(modifier => modifier.IsKind(SyntaxKind.MustOverrideKeyword));

        public override void VisitOperatorBlock(OperatorBlockSyntax node) => AddNode(node);

        public override void VisitPropertyBlock(PropertyBlockSyntax node) => VisitAccessors(node.Accessors);

        // Coverlet instruments C# auto properties but not VB.  May be able to remove this
        public override void VisitPropertyStatement(PropertyStatementSyntax node)
        {
            if (IsAbstract(node.Modifiers))
            {
                return;
            }

            AddNode(node);
        }

        public override void VisitEventBlock(EventBlockSyntax node) => VisitAccessors(node.Accessors);

        private void VisitAccessors(SyntaxList<AccessorBlockSyntax> accessors)
        {
            foreach (AccessorBlockSyntax accessor in accessors)
            {
                Visit(accessor);
            }
        }

        public override void VisitAccessorBlock(AccessorBlockSyntax node) => AddNode(node);

        private void AddNode(SyntaxNode node) => _spans.Add(node.Span);
    }
}
