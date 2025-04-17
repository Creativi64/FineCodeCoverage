using System;

namespace FineCodeCoverage.Core.Utilities.Telemetry
{
    internal class FaultEventName
    {
        private static void ThrowIfEmpty(string part, string parameterName)
        {
            if (String.IsNullOrEmpty(part))
            {
                throw new ArgumentNullException("FaultEventName part cannot be null or empty", parameterName);
            }
        }
        public FaultEventName(string product, string featureName, string entityName)
        {
            ThrowIfEmpty(product, nameof(product));
            ThrowIfEmpty(featureName, nameof(featureName));
            ThrowIfEmpty(entityName, nameof(entityName));

            Product = product;
            FeatureName = featureName;
            EntityName = entityName;
        }
        public override string ToString()
        {
            return $"{Product}/{FeatureName}/{EntityName}";
        }
        public string Product { get; }
        public string FeatureName { get; }
        public string EntityName { get; }
    }

}
