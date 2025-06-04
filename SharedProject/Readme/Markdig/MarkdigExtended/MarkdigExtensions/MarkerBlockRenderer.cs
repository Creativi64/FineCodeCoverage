using System;
using System.Collections.Generic;
using System.Windows.Documents;
using Markdig.Renderers;
using Markdig.Renderers.Wpf;

namespace FineCodeCoverage.Readme
{
    public class MarkerBlockRenderer : WpfObjectRenderer<MarkerBlock>
    {
        private readonly IDictionary<string, Func<IEnumerable<Block>>> _blockCreators;

        public MarkerBlockRenderer(IDictionary<string, Func<IEnumerable<Block>>> blockCreators)
            => this._blockCreators = blockCreators;

        public MarkerBlockRenderer(string marker, Func<IEnumerable<Block>> creator)
            : this(new Dictionary<string, Func<IEnumerable<Block>>> {
                    { marker, creator }
                }
            )
        { }

        protected override void Write(WpfRenderer renderer, MarkerBlock markerBlock)
        {
            if (this._blockCreators.TryGetValue(markerBlock.Marker, out Func<IEnumerable<Block>> creator))
            {
                IEnumerable<Block> blocks = creator();
                if (blocks != null)
                {
                    foreach (Block block in blocks)
                    {
                        renderer.WriteBlock(block);
                    }
                }
            }
        }
    }
}