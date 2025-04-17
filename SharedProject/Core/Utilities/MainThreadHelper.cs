using FineCodeCoverage.Output;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using System;

namespace FineCodeCoverage.Core.Utilities
{
    internal class FaultEventName
    {
        private static void ThrowIfEmpty(string part,string parameterName)
        {
            if (String.IsNullOrEmpty(part))
            {
                throw new ArgumentNullException("FaultEventName part cannot be null or empty",parameterName);
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

    internal interface IBuildFaultEventNameFromFeatureHierarchy
    {
        FaultEventName BuildFromFeatureNameHierarchy(params string[] hierarchy);
    }

    internal class ProductFaultEventNameBuilder
    {
        private readonly string product;
        public static string EntityName<TEntity>() => typeof(TEntity).FullName;
        
        private class BuildFaultEventNameFromFeatureHierarchy: IBuildFaultEventNameFromFeatureHierarchy
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
                if(hierarchy.Length == 0)
                {
                    throw new ArgumentException("Empty hierarchy",nameof(hierarchy));
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
    
    internal static class FCCFaultEventName
    {
        private static readonly ProductFaultEventNameBuilder productFaultEventNameBuilder = ProductFaultEventNameBuilder.Create("FineCodeCoverage");
        
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

    internal static class LoggerSingleton
    {
        private static Lazy<ILogger> _logger = new Lazy<ILogger>(() =>
        {
            var componentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
            if (componentModel == null)
            {
                throw new InvalidOperationException("IComponentModel service not available.");
            }

            return componentModel.GetService<ILogger>();
        });

        public static ILogger Instance => _logger.Value;
    }
    /*
        https://learn.microsoft.com/en-us/dotnet/api/microsoft.visualstudio.shell.vstasklibraryhelper.fileandforget?view=visualstudiosdk-2022
        FileAndForget will 
        https://learn.microsoft.com/en-us/dotnet/api/microsoft.visualstudio.telemetry.telemetrysession.postevent?view=visualstudiosdk-2022#microsoft-visualstudio-telemetry-telemetrysession-postevent(microsoft-visualstudio-telemetry-telemetryevent)
        Microsoft.VisualStudio.Telemetry.TelemetryService.DefaultSession.PostEvent - a FaultEvent but not to Watson, to AI
        and
        https://learn.microsoft.com/en-us/dotnet/api/microsoft.visualstudio.shell.interop.ivsactivitylog.logentry?view=visualstudiosdk-2022
    */

    internal static class MainThreadHelper
    {
        public static void SwitchAndFileAndForget(FaultEventName faultEventName, Action action, string faultDescription = null)
        {
#pragma warning disable VSSDK007
            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                action();
            }).FileAndForget(faultEventName.ToString(), faultDescription);
#pragma warning restore VSSDK007
        }

        public static void SwitchAndCatch(Action action, bool rethrow = false)
        {
            var faultEventName = FCCFaultEventName.WithEntityName(nameof(MainThreadHelper))
                .BuildFromFeatureNameHierarchy("Utilities", "SwitchAndCatch");
            SwitchAndFileAndForget(faultEventName, () =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    LoggerSingleton.Instance.Log(ex.ToString());
                    if (rethrow)
                    {
                        throw;
                    }
                }
            });
        }
    }
}
