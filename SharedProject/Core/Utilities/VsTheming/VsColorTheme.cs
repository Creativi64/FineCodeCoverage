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
        )
        {
            colorThemeService = serviceProvider.GetService(typeof(SVsColorThemeService));
        }

        private object CurrentTheme
        {
            get
            {
                if (currentTheme == null)
                {
                    SetCurrentTheme();
                }
                return currentTheme;
            }
        }
        public string CurrentThemeName
        {
            get => ((dynamic)CurrentTheme).Name;
        }

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
                    SetCurrentTheme();
                    themeChanged?.Invoke(this, EventArgs.Empty);

                };
            }
            remove
            {
                themeChanged = null;
            }
        }

        private void SetCurrentTheme()
        {
            this.currentTheme = ((dynamic)colorThemeService).CurrentTheme;
        }

        public VsColorEntry GetColorEntry(ColorName colorName)
        {
            if (indexer == null)
            {
                indexer = CurrentTheme.GetType().GetProperty("Item");
                colorNameType = indexer.GetIndexParameters()[0].ParameterType;
            }

            var vsColorName = Activator.CreateInstance(colorNameType, true);
            ((dynamic)vsColorName).Category = colorName.Category;
            ((dynamic)vsColorName).Name = colorName.Name;

            var colorEntry = indexer.GetValue(CurrentTheme, new object[] { vsColorName });
            if (colorEntry == null) return null;
            return new VsColorEntry(colorEntry, colorName);
        }
    }

}
