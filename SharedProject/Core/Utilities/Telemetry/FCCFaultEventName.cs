namespace FineCodeCoverage.Core.Utilities.Telemetry
{
    internal static class FCCFaultEventName
    {
        private static readonly ProductFaultEventNameBuilder productFaultEventNameBuilder =
            ProductFaultEventNameBuilder.Create("FineCodeCoverage");

        public static FaultEventName Create(string featureName, string entityName)
            => productFaultEventNameBuilder.Build(featureName, entityName);

        public static FaultEventName Create<TEntity>(string featureName)
            => productFaultEventNameBuilder.Build<TEntity>(featureName);

        public static IBuildFaultEventNameFromFeatureHierarchy WithEntityName(string entityName)
            => productFaultEventNameBuilder.WithEntityName(entityName);

        public static IBuildFaultEventNameFromFeatureHierarchy WithEntityName<TEntity>()
            => productFaultEventNameBuilder.WithEntityName<TEntity>();
    }
}
