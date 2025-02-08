using System;
using System.ComponentModel;
using System.Windows;
using Microsoft.VisualStudio.Shell;

namespace FineCodeCoverage.Wpf
{
    public class DesignTimeVsColorsResource : INotifyPropertyChanged, IThemeChangeListener
    {
        private ThemeResourceKey _themeResourceKey;
        private string defaultThemeName;
        private string themeName;
        private WeakReference<DependencyObject> weakTargetObject;

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnValueChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DesignTimeValue)));
        }

        #endregion

        public DesignTimeVsColorsResource(DependencyObject targetObject,ThemeResourceKey themeResourceKey,string defaultThemeName)
        {
            this.weakTargetObject = new WeakReference<DependencyObject>(targetObject);
            this._themeResourceKey = themeResourceKey;
            this.defaultThemeName = defaultThemeName;
            ThemeNameManager.ListenFromThemeChanges(targetObject, this);
            
        }

        public object DesignTimeValue { 
            get
            {
                var designTimeTheme = GetThemeName();
                return ThemeService.GetResource(designTimeTheme, _themeResourceKey);
            } 
        }

        void IThemeChangeListener.ThemeChanged(string newThemeName)
        {
            themeName = newThemeName;
            OnValueChanged();
        }

        private string GetThemeName()
        {
            if (themeName == null)
            {
                if (!weakTargetObject.TryGetTarget(out var targetObject))
                {
                    return defaultThemeName;
                }
                var rootThemeName = ThemeNameHelper.FindRootThemeName(targetObject);
                themeName = rootThemeName ?? defaultThemeName;
            }
            return themeName;

        }

    }

}
