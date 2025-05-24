using Markdig;
using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Renderers.Wpf;
using Markdig.Syntax;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Documents;

namespace FineCodeCoverage.Readme
{
    public class MarkerBlockParser : BlockParser
    {
        public MarkerBlockParser()
        {
            OpeningCharacters = new[] { '{' }; // trigger on `{`
        }

        private string GetMarker(string text)
        {
            // Extract the marker from the text, e.g., "{{marker}}" -> "marker"
            if (text.StartsWith("{{") && text.EndsWith("}}"))
            {
                return text.Substring(2, text.Length - 4).Trim();
            }
            return null;
        }

        public override BlockState TryOpen(BlockProcessor processor)
        {
            var line = processor.Line;
            var text = line.ToString();
            var marker = GetMarker(text);
            if (marker != null)
            {
                var block = new MarkerBlock(marker);
                processor.NewBlocks.Push(block);
                return BlockState.BreakDiscard;
            }

            return BlockState.None;
        }
    }
    public class MarkerBlock : LeafBlock
    {
        public MarkerBlock(string marker) : base(null) {
            Marker = marker;
        }
        public string Marker { get; }
    }

    public class MarkerBlockRenderer : WpfObjectRenderer<MarkerBlock>
    {
        private readonly IDictionary<string, Func<System.Windows.Documents.Block>> blockCreators;

        public MarkerBlockRenderer(IDictionary<string, Func<System.Windows.Documents.Block>> blockCreators)
        {
            this.blockCreators = blockCreators;
        }
        public MarkerBlockRenderer(string marker, Func<System.Windows.Documents.Block> creator) :
            this(
                new Dictionary<string, Func<System.Windows.Documents.Block>> {
                    { marker, creator }
                }
            ){
        }

        protected override void Write(WpfRenderer renderer, MarkerBlock block)
        {
            if(blockCreators.TryGetValue(block.Marker, out var creator)){

                var flowBlock = creator();
                if (flowBlock != null)
                {
                    renderer.WriteBlock(flowBlock);
                }
            }
            
        }
    }
    
    class ReadmeMarkerTableReplacerMarkdownExtension : IMarkdownExtension
    {
        private readonly string marker;
        private readonly Func<Table> tableCreator;

        public ReadmeMarkerTableReplacerMarkdownExtension(string marker,Func<Table> tableCreator)
        {
            this.marker = marker;
            this.tableCreator = tableCreator;
        }
        public void Setup(MarkdownPipelineBuilder pipeline)
        {
            pipeline.BlockParsers.Add(new MarkerBlockParser());
        }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
            renderer.ObjectRenderers.Add(new MarkerBlockRenderer(marker, tableCreator));
        }
    }

    //class IgnoreRemainingExtension : IMarkdownExtension
    //{
    //    public void Setup(MarkdownPipelineBuilder pipeline)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    internal class ReadMePipeLineProvider
    {
        // still need the fixup links
        public MarkdownPipeline Provide(string marker,Func<Table> tableCreator)
        {
            // do I need advanced now that replacing with a Table ?
            var readmeMarkerTableReplacerMarkdownExtension =
                new ReadmeMarkerTableReplacerMarkdownExtension(marker, tableCreator);
            return new MarkdownPipelineBuilder().UseAdvancedExtensions().Use(readmeMarkerTableReplacerMarkdownExtension)
                .Build();
        }
    }
}
