using System;

namespace FineCodeCoverage.Core.Utilities.Telemetry
{
    internal class FaultEventName
    {
        private static void ThrowIfEmpty(string part, string parameterName)
        {
            if (string.IsNullOrEmpty(part))
            {
                throw new ArgumentException("FaultEventName part cannot be null or empty", parameterName);
            }
        }

        public FaultEventName(string product, string featureName, string entityName)
        {
            ThrowIfEmpty(product, nameof(product));
            ThrowIfEmpty(featureName, nameof(featureName));
            ThrowIfEmpty(entityName, nameof(entityName));

            this.Product = product;
            this.FeatureName = featureName;
            this.EntityName = entityName;
        }
        public override string ToString() => $"{this.Product}/{this.FeatureName}/{this.EntityName}";
        public string Product { get; }
        public string FeatureName { get; }
        public string EntityName { get; }
    }
}