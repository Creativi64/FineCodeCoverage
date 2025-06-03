using System;

namespace FineCodeCoverage.Engine.Model
{
    interface ISettingsXmlParser
    {
        object Parse(string xml);
        Array ParseArray(string[] xml, bool nullable);
    }
}