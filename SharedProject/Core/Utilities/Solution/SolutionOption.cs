using System.IO;

namespace FineCodeCoverage.Core.Utilities.Solution
{
    internal abstract class SolutionOption<T> : ISolutionOption
    {
        private readonly IJsonConvertService jsonConvertService;

        public SolutionOption(IJsonConvertService jsonConvertService)
        {
            this.jsonConvertService = jsonConvertService;
            Value = GetDefaultValue();
        }

        public T Value { get; set; }

        public abstract string Key { get; protected set; }

        public void Load(Stream stream)
        {
            var previousValue = Value;
            using (var sr = new StreamReader(stream))
            {
                var optionAsString = sr.ReadToEnd();
                if (typeof(T) == typeof(string)) {
                    Value = (T)(object)optionAsString;
                }
                Value = (T)jsonConvertService.DeserializeObject(optionAsString, typeof(T));
            }
            Loaded();
        }

        protected virtual void Loaded()
        {

        }

        protected abstract T GetDefaultValue();

        public void Save(Stream stream)
        {
            using (var sw = new StreamWriter(stream))
            {
                sw.Write(jsonConvertService.SerializeObject(Value));
                sw.Flush();
            }
            Value = GetDefaultValue();
        }

        protected virtual void Saved() { }
    }
}
