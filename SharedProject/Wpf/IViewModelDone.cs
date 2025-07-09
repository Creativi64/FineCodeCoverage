using System;

namespace FineCodeCoverage.Wpf
{
    public interface IViewModelDone
    {
        event EventHandler<bool> Done;
    }
}
