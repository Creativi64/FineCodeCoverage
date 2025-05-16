using System;
using System.ComponentModel;

namespace FineCodeCoverage.Options
{
    interface IProfileOptionsProvider
    {
        object LoadSettingsFromStorage();
        void SaveSettingsToStorage();
        Lazy<PropertyDescriptorCollection> LazyOptionsPropertyDescriptorCollection { get; }
        bool Initializing { get; set; }
    }
}
