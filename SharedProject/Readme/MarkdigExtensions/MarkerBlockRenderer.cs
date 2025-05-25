using Markdig.Renderers.Wpf;
using Markdig.Renderers;
using System;
using System.Collections.Generic;

namespace FineCodeCoverage.Readme
{
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
