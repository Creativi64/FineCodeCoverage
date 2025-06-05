using System;

namespace FineCodeCoverage.Engine.Model
{
    internal interface ISettingsXmlParser
    {
        object Parse(string xml);

        Array ParseArray(string[] xml, bool nullable);
    }
}
