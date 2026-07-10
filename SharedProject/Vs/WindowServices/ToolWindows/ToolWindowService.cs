using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using FineCodeCoverage.VSAbstractions.Store;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace FineCodeCoverage.Vs.WindowServices.ToolWindows
{
    [Export(typeof(IToolWindowService))]
    [Export(typeof(IToolWindowServiceInit))]
    internal sealed class ToolWindowService : IToolWindowService, IToolWindowServiceInit
    {
        private sealed class PositionedToolWindow
        {
            public PositionedToolWindow(ProvideToolWindowAttribute provideToolWindowAttribute)
            {
                Rect = new Rect(
                    provideToolWindowAttribute.PositionX,
                    provideToolWindowAttribute.PositionY,
                    provideToolWindowAttribute.Width,
                    provideToolWindowAttribute.Height);
                ToolWindowType = provideToolWindowAttribute.ToolType;
            }

            public Type ToolWindowType { get; }

            public Rect Rect { get; }

            public bool HasPositioned { get; set; }
        }

        private const string PositionedToolWindowSettingsCollectionName = "FCCPositionedToolWindows";
        private readonly IWritableUserSettingsStoreProvider _writableUserSettingsStoreProvider;
        private readonly bool _alwaysPosition;
        private AsyncPackage _package;
        private IWritableSettingsStore _userSettingsStore;
        private List<PositionedToolWindow> _positionedToolWindows;

        [ImportingConstructor]
        public ToolWindowService(
            IWritableUserSettingsStoreProvider writeableUserSettingsStoreProvider,
            IShouldAlwaysPositionToolWindows shouldAlwaysPositionToolWindows)
        {
            _writableUserSettingsStoreProvider = writeableUserSettingsStoreProvider;
            _alwaysPosition = shouldAlwaysPositionToolWindows.AlwaysPosition;
        }

        public async Task<IToolWindowService> InitializeAsync(AsyncPackage package)
        {
            _package = package;
            _userSettingsStore = await _writableUserSettingsStoreProvider.ProvideAsync();
            SetPositionedToolWindows();
            return this;
        }

        private void SetPositionedToolWindows()
            => _positionedToolWindows = _package.GetType().GetCustomAttributes<ProvideToolWindowAttribute>()
                .Select(provideToolWindowAttribute => new PositionedToolWindow(provideToolWindowAttribute))
                .Where(ptw => ptw.Rect.Size != new Size(0, 0)).ToList();

        public async Task<ToolWindowPane> ShowToolWindowAsync(Type toolWindowType, int id, bool create, CancellationToken cancellationToken)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            PositionedToolWindow positionedToolWindow = _positionedToolWindows.FirstOrDefault(ptw => ptw.ToolWindowType == toolWindowType);
            if (positionedToolWindow != null)
            {
                await PossiblyPositionAsync(positionedToolWindow, toolWindowType, id, create, cancellationToken);
            }

            return await _package.ShowToolWindowAsync(toolWindowType, id, create, cancellationToken);
        }

        private async Task PossiblyPositionAsync(PositionedToolWindow positionedToolWindow, Type toolWindowType, int id, bool create, CancellationToken cancellationToken)
        {
            if (_alwaysPosition)
            {
                // always set to make designing easier
                await PositionAsync(positionedToolWindow, toolWindowType, id, create, cancellationToken);
            }
            else if (!positionedToolWindow.HasPositioned)
            {
                await PositionIfFirstTimeAsync(positionedToolWindow, toolWindowType, id, create, cancellationToken);
            }
        }

        private async Task PositionIfFirstTimeAsync(PositionedToolWindow positionedToolWindow, Type toolWindowType, int id, bool create, CancellationToken cancellationToken)
        {
            bool hasPositioned = false;
            if (_userSettingsStore.CollectionExists(PositionedToolWindowSettingsCollectionName))
            {
                hasPositioned = _userSettingsStore.GetBoolean(PositionedToolWindowSettingsCollectionName, toolWindowType.FullName, false);
            }
            else
            {
                _userSettingsStore.CreateCollection(PositionedToolWindowSettingsCollectionName);
            }

            if (hasPositioned)
            {
                return;
            }

            await PositionAsync(positionedToolWindow, toolWindowType, id, create, cancellationToken);
            _userSettingsStore.SetBoolean(PositionedToolWindowSettingsCollectionName, toolWindowType.FullName, true);
        }

        /*
          this allows ProvideToolWindowAttribute Width and Height to work properly
          https://developercommunity.visualstudio.com/t/ProvideToolWindowAttribute-Width-and-Hei/10941671
        */
        private async Task PositionAsync(PositionedToolWindow positionedToolWindow, Type toolWindowType, int id, bool create, CancellationToken cancellationToken)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            ToolWindowPane toolWindow = await _package.FindToolWindowAsync(toolWindowType, id, create, cancellationToken);
            var frame = toolWindow.Frame as IVsWindowFrame;
            Guid empty = Guid.Empty;
            _ = frame.SetFramePos(
                VSSETFRAMEPOS.SFP_fSize | VSSETFRAMEPOS.SFP_fMove,
                ref empty,
                (int)positionedToolWindow.Rect.Left,
                (int)positionedToolWindow.Rect.Top,
                (int)positionedToolWindow.Rect.Width,
                (int)positionedToolWindow.Rect.Height);

            positionedToolWindow.HasPositioned = true;
        }

        public Task<ToolWindowPane> ShowToolWindowAsync(Type toolWindowType, int id, bool create)
            => ShowToolWindowAsync(toolWindowType, id, create, _package.DisposalToken);
    }
}
