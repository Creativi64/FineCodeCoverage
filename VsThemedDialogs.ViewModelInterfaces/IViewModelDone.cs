using System;

namespace VsThemedDialogs
{
    public interface IViewModelDone
    {
        event EventHandler<bool> Done;
    }
}
