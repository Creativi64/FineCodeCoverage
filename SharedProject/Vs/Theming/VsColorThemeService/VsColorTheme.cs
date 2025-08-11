using System;
using System.ComponentModel.Composition;
using System.Reflection;
using Microsoft.VisualStudio.Shell;

namespace FineCodeCoverage.Vs.Theming.VsColorThemeService
{
    [Export(typeof(IVsColorTheme))]
    public class VsColorTheme : IVsColorTheme
    {
        private readonly object _colorThemeService;
        private object _currentTheme;
        private PropertyInfo _indexer;
        private Type _colorNameType;

        [ImportingConstructor]
        public VsColorTheme(
        [Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
            => _colorThemeService = serviceProvider.GetService(typeof(SVsColorThemeService));

        private object CurrentTheme
        {
            get
            {
                if (_currentTheme == null)
                {
                    SetCurrentTheme();
                }

                return _currentTheme;
            }
        }

        public string CurrentThemeName => ((dynamic)CurrentTheme).Name;

        private event EventHandler ThemeChangedHandler;

        public event EventHandler ThemeChanged
        {
            add
            {
                ThemeChangedHandler = value;
                Microsoft.VisualStudio.PlatformUI.VSColorTheme.ThemeChanged += (_) =>
                {
                    SetCurrentTheme();
                    ThemeChangedHandler?.Invoke(this, EventArgs.Empty);
                };
            }

            remove => ThemeChangedHandler = null;
        }

        private void SetCurrentTheme() => _currentTheme = ((dynamic)_colorThemeService).CurrentTheme;

        public VsColorEntry GetColorEntry(ColorName colorName)
        {
            if (_indexer == null)
            {
                _indexer = CurrentTheme.GetType().GetProperty("Item");
                _colorNameType = _indexer.GetIndexParameters()[0].ParameterType;
            }

            object vsColorName = Activator.CreateInstance(_colorNameType, true);
            ((dynamic)vsColorName).Category = colorName.Category;
            ((dynamic)vsColorName).Name = colorName.Name;

            object colorEntry = _indexer.GetValue(CurrentTheme, new object[] { vsColorName });
            return colorEntry == null ? null : new VsColorEntry(colorEntry, colorName);
        }
    }
}
