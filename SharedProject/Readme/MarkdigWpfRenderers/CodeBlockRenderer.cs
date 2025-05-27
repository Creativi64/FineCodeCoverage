using System;
using System.Windows.Documents;

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
