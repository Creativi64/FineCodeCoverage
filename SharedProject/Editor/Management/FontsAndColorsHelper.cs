using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using FineCodeCoverage.Utilities.Extensions;
using FineCodeCoverage.VSAbstractions.OutputWindow;
using FineCodeCoverage.VSAbstractions.Threading;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Classification;
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
        private readonly IEditorFormatMapService _editorFormatMapService;
        private readonly ILogger _logger;
        private IVsFontAndColorStorage _vsFontAndColorStorage;

        [ImportingConstructor]
        public FontsAndColorsHelper(
            [Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider,
            IThreadHelper threadHelper,
            IEditorFormatMapService editorFormatMapService,
            ILogger logger)
        {
            _serviceProvider = serviceProvider;
            _threadHelper = threadHelper;
            _editorFormatMapService = editorFormatMapService;
            _logger = logger;
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

        private IFontAndColorsInfo GetInfo(string displayName, IVsFontAndColorStorage fontAndColorStorage)
        {
            var touchAreaInfo = new ColorableItemInfo[1];
#pragma warning disable VSTHRD010 // Invoke single-threaded types on Main thread
            int getItemSuccess = fontAndColorStorage.GetItem(displayName, touchAreaInfo);
#pragma warning restore VSTHRD010 // Invoke single-threaded types on Main thread

            if (getItemSuccess != VSConstants.S_OK)
            {
                _logger.LogFileAndForget($"Unable to read coverage color '{displayName}' from Fonts and Colors (0x{getItemSuccess:X8}); using the editor format map.");
                return GetEditorFormatInfo(displayName);
            }

            System.Windows.Media.Color bgColor = ParseColor(touchAreaInfo[0].crBackground);
            System.Windows.Media.Color fgColor = ParseColor(touchAreaInfo[0].crForeground);
            return new FontAndColorsInfo(new ItemCoverageColours(fgColor, bgColor), touchAreaInfo[0].dwFontFlags == (uint)FONTFLAGS.FF_BOLD);
        }

        private IFontAndColorsInfo GetEditorFormatInfo(string name)
        {
            ResourceDictionary properties = _editorFormatMapService.GetEditorFormatMap("text").GetProperties(name);
            bool TryGetColor(string colorKey, string brushKey, out Color color)
            {
                if (properties[colorKey] is Color propertyColor)
                {
                    color = propertyColor;
                    return true;
                }

                if (properties[brushKey] is SolidColorBrush brush)
                {
                    color = brush.Color;
                    return true;
                }

                color = default;
                return false;
            }

            bool hasForeground = TryGetColor(EditorFormatDefinition.ForegroundColorId, EditorFormatDefinition.ForegroundBrushId, out Color foreground);
            bool hasBackground = TryGetColor(EditorFormatDefinition.BackgroundColorId, EditorFormatDefinition.BackgroundBrushId, out Color background);
            if (!hasForeground || !hasBackground)
            {
                IItemCoverageColours defaults = CoverageEditorFormatDefinitions.GetDefaultInfo(name).ItemCoverageColours;
                _logger.LogFileAndForget($"Coverage editor format '{name}' has missing colors; using FCC defaults for the missing values.");
                foreground = hasForeground ? foreground : defaults.Foreground;
                background = hasBackground ? background : defaults.Background;
            }

            return new FontAndColorsInfo(new ItemCoverageColours(foreground, background), properties[ClassificationFormatDefinition.IsBoldId] is true);
        }

        public async Task<List<IFontAndColorsInfo>> GetInfosAsync(Guid category, IEnumerable<string> names)
        {
            var infos = new List<IFontAndColorsInfo>();
            await OpenCloseCategoryAsync(
                category,
                fontAndColorStorage
                    => infos = names.Select(name => fontAndColorStorage == null
                        ? GetEditorFormatInfo(name)
                        : GetInfo(name, fontAndColorStorage)).ToList());
            return infos;
        }

        private async Task OpenCloseCategoryAsync(Guid category, Action<IVsFontAndColorStorage> action)
        {
            IVsFontAndColorStorage fontAndColorStorage = await GetVsFontAndColorStorageAsync();
            await _threadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

#pragma warning disable VSTHRD010 // Invoke single-threaded types on Main thread
            int success = fontAndColorStorage.OpenCategory(ref category, _storeFlags);

            if (success != VSConstants.S_OK)
            {
                _logger.LogFileAndForget($"Unable to open coverage Fonts and Colors category '{category}' (0x{success:X8}); using the editor format map.");
                action(null);
                return;
            }

            try
            {
                action(fontAndColorStorage);
            }
            finally
            {
                _ = fontAndColorStorage.CloseCategory();
            }
#pragma warning restore VSTHRD010 // Invoke single-threaded types on Main thread
        }
    }
}
