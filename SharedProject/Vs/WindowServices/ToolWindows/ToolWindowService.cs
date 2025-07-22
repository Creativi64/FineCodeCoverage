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

namespace FineCodeCoverage.Output
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
        private AsyncPackage _package;
        private IWritableSettingsStore _userSettingsStore;
        private List<PositionedToolWindow> _positionedToolWindows;

        [ImportingConstructor]
        public ToolWindowService(IWritableUserSettingsStoreProvider writeableUserSettingsStoreProvider)
            => _writableUserSettingsStoreProvider = writeableUserSettingsStoreProvider;

        public async Task<IToolWindowService> InitializeAsync(AsyncPackage package)
        {
            _package = package;
            _userSettingsStore = await _writableUserSettingsStoreProvider.ProvideAsync();
            GetPositionedToolWindows();
            return this;
        }

        private void GetPositionedToolWindows() => _positionedToolWindows = _package.GetType().GetCustomAttributes<ProvideToolWindowAttribute>()
                .Select(provideToolWindowAttribute => new PositionedToolWindow(provideToolWindowAttribute))
                .Where(ptw => ptw.Rect.Size != new Size(0, 0)).ToList();

        public async Task<ToolWindowPane> ShowToolWindowAsync(Type toolWindowType, int id, bool create, CancellationToken cancellationToken)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            PositionedToolWindow positionedToolWindow = _positionedToolWindows.FirstOrDefault(ptw => ptw.ToolWindowType == toolWindowType);
            if (positionedToolWindow?.HasPositioned == false)
            {
                bool hasPositioned = false;
                if (_userSettingsStore.CollectionExists(PositionedToolWindowSettingsCollectionName))
                {
                    hasPositioned = _userSettingsStore.GetBoolean(PositionedToolWindowSettingsCollectionName, toolWindowType.FullName,false);
                }
                else
                {
                    _userSettingsStore.CreateCollection(PositionedToolWindowSettingsCollectionName);
                }

                if (!hasPositioned)
                {
                    _userSettingsStore.SetBoolean(PositionedToolWindowSettingsCollectionName, toolWindowType.FullName, true);
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
                }

                positionedToolWindow.HasPositioned = true;
            }

            return await _package.ShowToolWindowAsync(toolWindowType, id, create, cancellationToken);
        }

        public Task<ToolWindowPane> ShowToolWindowAsync(Type toolWindowType, int id, bool create)
            => ShowToolWindowAsync(toolWindowType, id, create, _package.DisposalToken);
    }
}
