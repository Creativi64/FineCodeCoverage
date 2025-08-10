namespace FineCodeCoverage.Utilities.Telemetry
{
    internal static class FCCFaultEventName
    {
        private static readonly ProductFaultEventNameBuilder s_productFaultEventNameBuilder =
            ProductFaultEventNameBuilder.Create("FineCodeCoverage");

        public static FaultEventName Create(string featureName, string entityName)
            => s_productFaultEventNameBuilder.Build(featureName, entityName);

        public static FaultEventName Create<TEntity>(string featureName)
            => s_productFaultEventNameBuilder.Build<TEntity>(featureName);

        public static IBuildFaultEventNameFromFeatureHierarchy WithEntityName(string entityName)
            => s_productFaultEventNameBuilder.WithEntityName(entityName);

        public static IBuildFaultEventNameFromFeatureHierarchy WithEntityName<TEntity>()
            => s_productFaultEventNameBuilder.WithEntityName<TEntity>();
    }
}
