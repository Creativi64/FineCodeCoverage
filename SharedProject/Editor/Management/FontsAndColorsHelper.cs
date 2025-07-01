using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Core.Utilities.VsThreading;
using FineCodeCoverage.Utilities.Extensions;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Task = System.Threading.Tasks.Task;

namespace FineCodeCoverage.Editor.Management
{
    [Export(typeof(IFontsAndColorsHelper))]
    internal sealed class FontsAndColorsHelper : IFontsAndColorsHelper
    {
        private readonly uint _storeFlags = (uint)(__FCSTORAGEFLAGS.FCSF_READONLY | __FCSTORAGEFLAGS.FCSF_LOADDEFAULTS | __FCSTORAGEFLAGS.FCSF_NOAUTOCOLORS | __FCSTORAGEFLAGS.FCSF_PROPAGATECHANGES);
        private readonly IServiceProvider _serviceProvider;
        private readonly IThreadHelper _threadHelper;
        private IVsFontAndColorStorage _vsFontAndColorStorage;

        [ImportingConstructor]
        public FontsAndColorsHelper(
            [Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider,
            IThreadHelper threadHelper)
        {
            _serviceProvider = serviceProvider;
            _threadHelper = threadHelper;
        }

        private static System.Windows.Media.Color ParseColor(uint color)
            => System.Drawing.ColorTranslator.FromOle(Convert.ToInt32(color)).ToMediaColor();

        private async Task<IVsFontAndColorStorage> GetVsFontAndColorStorageAsync()
        {
            if (_vsFontAndColorStorage == null)
            {
                await _threadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                _vsFontAndColorStorage = _serviceProvider.GetService<IVsFontAndColorStorage>();
            }

            return _vsFontAndColorStorage;
        }

        private static IFontAndColorsInfo GetInfo(string displayName, IVsFontAndColorStorage fontAndColorStorage)
        {
            var touchAreaInfo = new ColorableItemInfo[1];
#pragma warning disable VSTHRD010 // Invoke single-threaded types on Main thread
            int getItemSuccess = fontAndColorStorage.GetItem(displayName, touchAreaInfo);
#pragma warning restore VSTHRD010 // Invoke single-threaded types on Main thread

            if (getItemSuccess != VSConstants.S_OK)
            {
                return null;
            }

            System.Windows.Media.Color bgColor = ParseColor(touchAreaInfo[0].crBackground);
            System.Windows.Media.Color fgColor = ParseColor(touchAreaInfo[0].crForeground);
            return new FontAndColorsInfo(new ItemCoverageColours(fgColor, bgColor), touchAreaInfo[0].dwFontFlags == (uint)FONTFLAGS.FF_BOLD);
        }

        public async Task<List<IFontAndColorsInfo>> GetInfosAsync(Guid category, IEnumerable<string> names)
        {
            var infos = new List<IFontAndColorsInfo>();
            await OpenCloseCategoryAsync(
                category,
                fontAndColorStorage
                    => infos = names.Select(name => GetInfo(name, fontAndColorStorage))
                                .Where(color => color != null).ToList());
            return infos;
        }

        private async Task OpenCloseCategoryAsync(Guid category, Action<IVsFontAndColorStorage> action)
        {
            IVsFontAndColorStorage fontAndColorStorage = await GetVsFontAndColorStorageAsync();
            await _threadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

#pragma warning disable VSTHRD010 // Invoke single-threaded types on Main thread
            int success = fontAndColorStorage.OpenCategory(ref category, _storeFlags);

            if (success == VSConstants.S_OK)
            {
                action(fontAndColorStorage);
            }

            _ = fontAndColorStorage.CloseCategory();
#pragma warning restore VSTHRD010 // Invoke single-threaded types on Main thread
        }
    }
}
