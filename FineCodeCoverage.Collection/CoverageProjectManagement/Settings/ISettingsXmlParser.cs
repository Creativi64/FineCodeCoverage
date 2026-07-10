using System;

namespace FineCodeCoverage.Collection.CoverageProjectManagement.Settings
{
    internal interface ISettingsXmlParser
    {
        object Parse(string xml);

        Array ParseArray(string[] xml, bool nullable);
    }
}
