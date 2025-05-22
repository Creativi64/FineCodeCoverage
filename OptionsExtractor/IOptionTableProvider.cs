using System;

namespace OptionsExtractor
{
    internal interface IOptionTableProvider
    {
        string GetTableString(Type packageType, Type coverageSettingsType);
    }
}
