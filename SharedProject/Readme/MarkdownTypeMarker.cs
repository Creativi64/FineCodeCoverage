namespace FineCodeCoverage.Readme
{
    public enum MarkdownTypeMarker
    {
        FlowDocument,

        QuoteBlock, // Section

        CodeInline, // Run
        CodeBlock, // Paragraph
        Paragraph, // Paragraph
        HeadingBlock1, // Paragraph
        HeadingBlock2, // Paragraph
        HeadingBlock3, // Paragraph
        HeadingBlock4, // Paragraph
        HeadingBlock5, // Paragraph
        HeadingBlock6, // Paragraph

        ThematicBreakBlock, // Line

        EmphasisInlineStrikeThrough, // Span
        EmphasisInlineSubscript, // Span
        EmphasisInlineSuperscript, // Span
        EmphasisInlineInserted, // Span
        EmphasisInlineMarked, // Span

        AutolinkInline, // Hyperlink

        LinkInlineHyperlink, // Hyperlink
        LinkInlineImage,// Image factory

        TaskList,

        Table, // Table
        TableHeader, // TableRow
        TableCell // TableCell
    }
}
