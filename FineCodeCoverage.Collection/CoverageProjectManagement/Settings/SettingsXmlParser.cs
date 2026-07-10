using System;
using System.Collections.Generic;
using System.Linq;

namespace FineCodeCoverage.Collection.CoverageProjectManagement.Settings
{
    internal sealed class SettingsXmlParser<T, TNullable> : ISettingsXmlParser
    {
        private readonly TryParseDelegate<T> _tryParse;

        public SettingsXmlParser(TryParseDelegate<T> tryParse) => _tryParse = tryParse;

        public object Parse(string xml) => _tryParse(xml, out T result) ? result : (object)null;

        public Array ParseArray(string[] xml, bool nullable)
        {
            IEnumerable<T> valid = ParseValid(xml, _tryParse);
            return nullable ? valid.Cast<TNullable>().ToArray() : (Array)valid.ToArray();
        }

        private static IEnumerable<T> ParseValid(string[] input, TryParseDelegate<T> tryParse)
        {
            foreach (string s in input)
            {
                if (tryParse(s, out T val))
                {
                    yield return val;
                }
            }
        }
    }
}
