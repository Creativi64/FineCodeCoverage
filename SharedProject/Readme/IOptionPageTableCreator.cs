using System.Collections.Generic;
using System.Windows.Documents;

namespace FineCodeCoverage.Readme
{
    interface IOptionPageTableCreator
    {
        Table Create();
        IReadOnlyList<ElementAndMarker> ElementAndMarkers { get; }
    }
}
