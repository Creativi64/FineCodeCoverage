using System;
using System.Windows.Documents;
using Markdig.Renderers;
using Markdig.Syntax;

namespace FineCodeCoverage.Readme
{
    public class CodeBlockRenderer : NotifyingObjectRenderer<CodeBlock>
    {
        protected override ElementAndMarker WriteAndReturn(WpfRenderer renderer, CodeBlock obj)
        {
            if (renderer == null) throw new ArgumentNullException(nameof(renderer));

            var paragraph = new Paragraph();

            renderer.Push(paragraph);
            renderer.WriteLeafRawLines(obj);
            renderer.Pop();
            return new ElementAndMarker(paragraph, MarkdownTypeMarker.CodeBlock);
        }
    }
}