using System;

namespace FineCodeCoverage.Core.Utilities.Telemetry
{
    internal sealed class ProductFaultEventNameBuilder
    {
        private readonly string product;
        public static string EntityName<TEntity>() => typeof(TEntity).FullName;

        private class BuildFaultEventNameFromFeatureHierarchy : IBuildFaultEventNameFromFeatureHierarchy
        {
            private readonly string product;
            private readonly string entityName;

            public BuildFaultEventNameFromFeatureHierarchy(string product, string entityName)
            {
                this.product = product;
                this.entityName = entityName;
            }

            private static string GetFeatureName(params string[] hierarchy)
            {
                if (hierarchy.Length == 0)
                {
                    throw new ArgumentException("Empty hierarchy", nameof(hierarchy));
                }
                return String.Join("/", hierarchy);
            }

            public FaultEventName BuildFromFeatureNameHierarchy(params string[] hierarchy)
            {
                return new FaultEventName(product, GetFeatureName(hierarchy), this.entityName);
            }
        }

        private ProductFaultEventNameBuilder(string product)
        {
            this.product = product;
        }

        public FaultEventName Build(string featureName, string entityName)
        {
            return new FaultEventName(product, featureName, entityName);
        }

        public FaultEventName Build<TEntity>(string featureName)
        {
            return new FaultEventName(product, featureName, EntityName<TEntity>());
        }

        public IBuildFaultEventNameFromFeatureHierarchy WithEntityName(string entityName)
        {
            return new BuildFaultEventNameFromFeatureHierarchy(product, entityName);
        }

        public IBuildFaultEventNameFromFeatureHierarchy WithEntityName<TEntity>()
        {
            return WithEntityName(EntityName<TEntity>());
        }

        public static ProductFaultEventNameBuilder Create(string productName)
        {
            return new ProductFaultEventNameBuilder(productName);
        }
    }
}
