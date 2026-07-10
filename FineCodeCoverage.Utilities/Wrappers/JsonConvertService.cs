using System;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace FineCodeCoverage.Utilities.Wrappers
{
    [ExcludeFromCodeCoverage]
    [Export(typeof(IJsonConvertService))]
    public class JsonConvertService : IJsonConvertService
    {
        public object DeserializeObject(string serialized, Type propertyType) => JsonConvert.DeserializeObject(serialized, propertyType);

        public T DeserializeObject<T>(string serialized) => JsonConvert.DeserializeObject<T>(serialized);

        public string SerializeObject(object value) => JsonConvert.SerializeObject(value);
    }
}
