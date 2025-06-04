using System;

namespace FineCodeCoverage.Core.Utilities.Telemetry
{
    internal sealed class ProductFaultEventNameBuilder
    {
        private readonly string _product;
        public static string EntityName<TEntity>() => typeof(TEntity).FullName;

        private class BuildFaultEventNameFromFeatureHierarchy : IBuildFaultEventNameFromFeatureHierarchy
        {
            private readonly string _product;
            private readonly string _entityName;

            public BuildFaultEventNameFromFeatureHierarchy(string product, string entityName)
            {
                this._product = product;
                this._entityName = entityName;
            }

            private static string GetFeatureName(params string[] hierarchy)
                => hierarchy.Length == 0 ? throw new ArgumentException("Empty hierarchy", nameof(hierarchy))
                    : string.Join("/", hierarchy);

            public FaultEventName BuildFromFeatureNameHierarchy(params string[] hierarchy)
                => new FaultEventName(this._product, GetFeatureName(hierarchy), this._entityName);
        }

        private ProductFaultEventNameBuilder(string product) => this._product = product;

        public FaultEventName Build(string featureName, string entityName)
            => new FaultEventName(this._product, featureName, entityName);

        public FaultEventName Build<TEntity>(string featureName)
            => new FaultEventName(this._product, featureName, EntityName<TEntity>());

        public IBuildFaultEventNameFromFeatureHierarchy WithEntityName(string entityName)
            => new BuildFaultEventNameFromFeatureHierarchy(this._product, entityName);

        public IBuildFaultEventNameFromFeatureHierarchy WithEntityName<TEntity>()
            => this.WithEntityName(EntityName<TEntity>());

        public static ProductFaultEventNameBuilder Create(string productName)
            => new ProductFaultEventNameBuilder(productName);
    }
}
