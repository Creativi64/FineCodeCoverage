using System;
using System.ComponentModel;

namespace FineCodeCoverage.Options.Base
{
    public interface IProfileOptionsProvider : IProvideOptions
    {
        void SaveSettingsToStorage();

        Lazy<PropertyDescriptorCollection> LazyOptionsPropertyDescriptorCollection { get; }

        bool Initializing { get; set; }
    }
}
