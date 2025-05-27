using System;
using System.Windows.Documents;

namespace FineCodeCoverage.Readme
{
    public class QuoteBlockRenderer : NotifyingObjectRenderer<QuoteBlock>
    {
        protected override ElementAndMarker WriteAndReturn(WpfRenderer renderer, QuoteBlock obj)
        {
            if (renderer == null) throw new ArgumentNullException(nameof(renderer));
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            var section = new Section();

            renderer.Push(section);
            renderer.WriteChildren(obj);
            renderer.Pop();
            return new ElementAndMarker(section, MarkdownTypeMarker.QuoteBlock);
        }
    }
}
