using System;
using System.ComponentModel.Composition;
using System.Reflection;
using Microsoft.VisualStudio.Shell;

namespace FineCodeCoverage.Core.Utilities
{
    [Export(typeof(IVsColorTheme))]
    public class VsColorTheme : IVsColorTheme
    {
        private object currentTheme;
        private readonly object colorThemeService;

        private PropertyInfo indexer;
        private Type colorNameType;

        [ImportingConstructor]
        public VsColorTheme(
        [Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider
        ) => this.colorThemeService = serviceProvider.GetService(typeof(SVsColorThemeService));

        private object CurrentTheme
        {
            get
            {
                if (this.currentTheme == null)
                {
                    this.SetCurrentTheme();
                }

                return this.currentTheme;
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

        private void SetCurrentTheme() => this.currentTheme = ((dynamic)this.colorThemeService).CurrentTheme;

        public VsColorEntry GetColorEntry(ColorName colorName)
        {
            if (this.indexer == null)
            {
                this.indexer = this.CurrentTheme.GetType().GetProperty("Item");
                this.colorNameType = this.indexer.GetIndexParameters()[0].ParameterType;
            }

            object vsColorName = Activator.CreateInstance(this.colorNameType, true);
            ((dynamic)vsColorName).Category = colorName.Category;
            ((dynamic)vsColorName).Name = colorName.Name;

            object colorEntry = this.indexer.GetValue(this.CurrentTheme, new object[] { vsColorName });
            return colorEntry == null ? null : new VsColorEntry(colorEntry, colorName);
        }
    }
}
