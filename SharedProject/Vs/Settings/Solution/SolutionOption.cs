using System;
using System.IO;
using System.Text;
using FineCodeCoverage.Utilities.Wrappers;

namespace FineCodeCoverage.Core.Utilities.Solution
{
    internal abstract class SolutionOption<T> : ISolutionOption
    {
        private readonly IJsonConvertService _jsonConvertService;

        public event EventHandler UnloadedEvent;

        protected SolutionOption(IJsonConvertService jsonConvertService)
        {
            _jsonConvertService = jsonConvertService;
            Value = GetDefaultValue();
        }

        public T Value { get; set; }

        public abstract string Key { get; protected set; }

        public void Load(Stream stream)
        {
            using (var sr = new StreamReader(stream))
            {
                string optionAsString = sr.ReadToEnd();
                if (typeof(T) == typeof(string))
                {
                    Value = (T)(object)optionAsString;
                }

                Value = _jsonConvertService.DeserializeObject<T>(optionAsString);
            }

            Loaded();
        }

        protected virtual void Loaded()
        {
        }

        protected abstract T GetDefaultValue();

        public void Save(Stream stream)
        {
            using (var sw = new StreamWriter(stream, Encoding.UTF8, 1024, leaveOpen: true))
            {
                sw.Write(_jsonConvertService.SerializeObject(Value));
                sw.Flush();
            }

            Saved();
        }

        protected virtual void Saved()
        {
        }

        public void Unloaded()
        {
            Value = GetDefaultValue();
            UnloadedEvent?.Invoke(this, EventArgs.Empty);
        }
    }
}
