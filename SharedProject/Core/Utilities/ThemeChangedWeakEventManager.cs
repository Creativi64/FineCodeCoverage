using System;
using System.Windows;
using Microsoft.VisualStudio.PlatformUI;

namespace FineCodeCoverage.Core.Utilities
{
    public class ThemeChangedWeakEventManager : WeakEventManager
    {
        public static void AddListener(IWeakEventListener listener)
        {
            CurrentManager.ProtectedAddListener(null, listener);
        }

        public static void RemoveListener(IWeakEventListener listener)
        {
            CurrentManager.ProtectedRemoveListener(null, listener);
        }

        private static ThemeChangedWeakEventManager CurrentManager
        {
            get
            {
                Type managerType = typeof(ThemeChangedWeakEventManager);
                var manager = (ThemeChangedWeakEventManager)GetCurrentManager(managerType);
                if (manager == null)
                {
                    manager = new ThemeChangedWeakEventManager();
                    SetCurrentManager(managerType, manager);
                }

                return manager;
            }
        }

        protected sealed override void StartListening(object source)
        {
            VSColorTheme.ThemeChanged += this.ThemeChanged;
        }

        protected sealed override void StopListening(object source)
        {
            VSColorTheme.ThemeChanged -= this.ThemeChanged;
        }

        void ThemeChanged(EventArgs e)
        {
            this.DeliverEvent(null, e);
        }
    }
}
