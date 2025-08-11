using System.Collections.Generic;
using System.Windows.Documents;
using MarkdigExtended.NotifyingWpfRenderers.Base;

namespace FineCodeCoverage.Readme.Options.OptionsTable
{
    internal interface IOptionPageTableCreator
    {
        Table Create();

        IReadOnlyList<ElementAndMarker> ElementAndMarkers { get; }
    }
}
