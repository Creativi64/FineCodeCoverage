using System;
using System.Collections.Generic;
using System.Linq;

namespace FineCodeCoverage.Engine.Model
{
    internal class SettingsXmlParser<T, TNullable> : ISettingsXmlParser
    {
        private readonly TryParseDelegate<T> _tryParse;

        public SettingsXmlParser(TryParseDelegate<T> tryParse) => this._tryParse = tryParse;

        public object Parse(string xml) => this._tryParse(xml, out T result) ? result : (object)null;

        public Array ParseArray(string[] xml, bool nullable)
        {
            IEnumerable<T> valid = ParseValid(xml, this._tryParse);
            return nullable ? valid.Cast<TNullable>().ToArray() : (Array)valid.ToArray();
        }

        private static IEnumerable<T> ParseValid(string[] input, TryParseDelegate<T> tryParse)
        {
            foreach (string s in input)
            {
                if (tryParse(s, out T val))
                    yield return val;
            }
        }
    }
}
