using System;
using System.ComponentModel.Composition;
using System.Reflection;
using Microsoft.VisualStudio.Shell;

namespace FineCodeCoverage.Core.Utilities
{
    [Export(typeof(IVsColorTheme))]
    public class VsColorTheme : IVsColorTheme
    {
        private object _currentTheme;
        private readonly object _colorThemeService;

        private PropertyInfo _indexer;
        private Type _colorNameType;

        [ImportingConstructor]
        public VsColorTheme(
        [Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider
        ) => this._colorThemeService = serviceProvider.GetService(typeof(SVsColorThemeService));

        private object CurrentTheme
        {
            get
            {
                if (this._currentTheme == null)
                {
                    this.SetCurrentTheme();
                }

                return this._currentTheme;
            }
        }
        public string CurrentThemeName => ((dynamic)this.CurrentTheme).Name;

#pragma warning disable IDE1006 // Naming Styles
        private event EventHandler themeChanged;
#pragma warning restore IDE1006 // Naming Styles

        public event EventHandler ThemeChanged
        {
            add
            {
                themeChanged = value;
                Microsoft.VisualStudio.PlatformUI.VSColorTheme.ThemeChanged += (_) =>
                {
                    this.SetCurrentTheme();
                    themeChanged?.Invoke(this, EventArgs.Empty);

                };
            }

            remove => themeChanged = null;
        }

        private void SetCurrentTheme() => this._currentTheme = ((dynamic)this._colorThemeService).CurrentTheme;

        public VsColorEntry GetColorEntry(ColorName colorName)
        {
            if (this._indexer == null)
            {
                this._indexer = this.CurrentTheme.GetType().GetProperty("Item");
                this._colorNameType = this._indexer.GetIndexParameters()[0].ParameterType;
            }

            object vsColorName = Activator.CreateInstance(this._colorNameType, true);
            ((dynamic)vsColorName).Category = colorName.Category;
            ((dynamic)vsColorName).Name = colorName.Name;

            object colorEntry = this._indexer.GetValue(this.CurrentTheme, new object[] { vsColorName });
            return colorEntry == null ? null : new VsColorEntry(colorEntry, colorName);
        }
    }
}
