using System.ComponentModel;
using Microsoft.VisualStudio.PlatformUI;

namespace VsThemedDialogs
{
    public class ThemeChangeObservable : INotifyPropertyChanged
    {
        public static ThemeChangeObservable Instance { get; } = new ThemeChangeObservable();

        public event PropertyChangedEventHandler PropertyChanged;

        public ThemeChangeObservable() => VSColorTheme.ThemeChanged += VSColorTheme_ThemeChanged;

        public object ChangeMarker { get; private set; } = new object();

        private void VSColorTheme_ThemeChanged(ThemeChangedEventArgs e)
        {
            ChangeMarker = new object();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ChangeMarker)));
        }
    }
}
