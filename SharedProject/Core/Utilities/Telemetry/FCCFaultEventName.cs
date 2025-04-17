namespace FineCodeCoverage.Core.Utilities.Telemetry
{
    internal static class FCCFaultEventName
    {
        private static readonly ProductFaultEventNameBuilder productFaultEventNameBuilder =
            ProductFaultEventNameBuilder.Create("FineCodeCoverage");

        public static FaultEventName Create(string featureName, string entityName)
        {
            return productFaultEventNameBuilder.Build(featureName, entityName);
        }

        public static FaultEventName Create<TEntity>(string featureName)
        {
            return productFaultEventNameBuilder.Build<TEntity>(featureName);
        }

        public static IBuildFaultEventNameFromFeatureHierarchy WithEntityName(string entityName)
        {
            return productFaultEventNameBuilder.WithEntityName(entityName);
        }

        public static IBuildFaultEventNameFromFeatureHierarchy WithEntityName<TEntity>()
        {
            return productFaultEventNameBuilder.WithEntityName<TEntity>();
        }
    }
}
