using System;
using System.ComponentModel;

namespace FineCodeCoverage.Options
{
    internal interface IProfileOptionsProvider : IProvideOptions
    {
        void SaveSettingsToStorage();

        Lazy<PropertyDescriptorCollection> LazyOptionsPropertyDescriptorCollection { get; }

        bool Initializing { get; set; }
    }
}
