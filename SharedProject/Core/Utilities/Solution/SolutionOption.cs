using System;
using System.IO;

namespace FineCodeCoverage.Core.Utilities.Solution
{
    internal class SolutionOptionLoadEventArgs<T>
    {
        public SolutionOptionLoadEventArgs(T previousValue)
        {
            PreviousValue = previousValue;
        }

        public T PreviousValue { get; }
    }
    interface ISolutionOptionEvents<T> {
        event EventHandler<SolutionOptionLoadEventArgs<T>> LoadedEvent;
        // event EventHandler SavedEvent;
    }

    internal abstract class SolutionOption<T> : ISolutionOption, ISolutionOptionEvents<T>
    {
        private readonly IJsonConvertService jsonConvertService;

        public SolutionOption(IJsonConvertService jsonConvertService)
        {
            this.jsonConvertService = jsonConvertService;
        }
        public T Value { get; set; }
        public abstract string Key { get; protected set; }

        public event EventHandler<SolutionOptionLoadEventArgs<T>> LoadedEvent;

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
            LoadedEvent?.Invoke(this, new SolutionOptionLoadEventArgs<T>(previousValue));
        }

        public void Save(Stream stream)
        {
            using (var sw = new StreamWriter(stream))
            {
                sw.Write(jsonConvertService.SerializeObject(Value));
                sw.Flush();
            }
        }
    }
}
