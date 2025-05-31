using System;
using System.IO;
using System.Text;

namespace FineCodeCoverage.Core.Utilities.Solution
{
    internal abstract class SolutionOption<T> : ISolutionOption
    {
        private readonly IJsonConvertService jsonConvertService;
        public event EventHandler UnloadedEvent;

        protected SolutionOption(IJsonConvertService jsonConvertService)
        {
            this.jsonConvertService = jsonConvertService;
            this.Value = this.GetDefaultValue();
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
                    this.Value = (T)(object)optionAsString;
                }

                this.Value = this.jsonConvertService.DeserializeObject<T>(optionAsString);
            }

            this.Loaded();
        }

        protected virtual void Loaded()
        {

        }

        protected abstract T GetDefaultValue();

        public void Save(Stream stream)
        {
            using (var sw = new StreamWriter(stream, Encoding.UTF8, 1024, leaveOpen: true))
            {
                sw.Write(this.jsonConvertService.SerializeObject(this.Value));
                sw.Flush();
            }

            this.Saved();
        }

        protected virtual void Saved() { }

        public void Unloaded()
        {
            this.Value = this.GetDefaultValue();
            UnloadedEvent?.Invoke(this, EventArgs.Empty);
        }
    }
}
