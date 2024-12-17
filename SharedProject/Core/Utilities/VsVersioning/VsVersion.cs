using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;

namespace FineCodeCoverage.Core.Utilities
{
    [Export(typeof(IVsVersion))]
    internal class VsVersion : IVsVersion
    {
        private readonly IServiceProvider serviceProvider;
        private string semanticVersion;
        private string releaseVersion;
        private string displayVersion;
        private string editionName;

        [ImportingConstructor]
        public VsVersion(
            [Import(typeof(SVsServiceProvider))]
            IServiceProvider serviceProvider
        )
        {
#if VS2022
            this.Is2022 = true;

#endif
            this.serviceProvider = serviceProvider;
        }

        public bool Is2022 { get; }

        public string GetSemanticVersion()
        {
            if(this.semanticVersion == null)
            {
                this.semanticVersion = this.GetAppIdStringProperty(-8642);
            }

            return this.semanticVersion;
        }

        public string GetReleaseVersion()
        {
            if (this.releaseVersion == null)
            {
                this.releaseVersion = this.GetAppIdStringProperty(-8597);
            }

            return this.releaseVersion;
        }

        public string GetDisplayVersion()
        {
            if (this.displayVersion == null)
            {
                this.displayVersion = this.GetAppIdStringProperty(-8641);
            }

            return this.displayVersion;
        }

        public string GetEditionName()
        {
            if(this.editionName == null)
            {
                this.editionName = this.GetAppIdStringProperty(-8620);
            }

            return this.editionName;
        }

        private string GetAppIdStringProperty(int propId)
        {
            var vsAppId = this.serviceProvider.GetService(typeof(SVsAppId)) as IVsAppId;
            _ = vsAppId.GetProperty(propId, out object v);
            return v as string;
        }
    }
}
