using System;
using System.Collections.Generic;
using System.Windows.Documents;
using Markdig.Renderers;
using Markdig.Renderers.Wpf;

namespace FineCodeCoverage.Readme
{
    public class MarkerBlockRenderer : WpfObjectRenderer<MarkerBlock>
    {
        private readonly IDictionary<string, Func<Block>> blockCreators;

        public MarkerBlockRenderer(IDictionary<string, Func<Block>> blockCreators)
        {
            this.blockCreators = blockCreators;
        }
        public MarkerBlockRenderer(string marker, Func<Block> creator) :
            this(
                new Dictionary<string, Func<Block>> {
                    { marker, creator }
                }
            )
        {
        }

        protected override void Write(WpfRenderer renderer, MarkerBlock block)
        {
            if (blockCreators.TryGetValue(block.Marker, out var creator))
            {
                var flowBlock = creator();
                if (flowBlock != null)
                {
                    renderer.WriteBlock(flowBlock);
                }
            }
        }
    }
}
