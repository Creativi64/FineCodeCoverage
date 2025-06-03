using System.Linq;
using System.Xml.Linq;

namespace FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage
{
    internal static class RunSettingsHelper
    {
        public const string FriendlyNameAttributeName = "friendlyName";
        public const string UriAttributeName = "uri";
        public const string MsDataCollectorUri = "datacollector://Microsoft/CodeCoverage/2.0";
        public const string MsDataCollectorFriendlyName = "Code Coverage";
        public static bool IsMsDataCollector(XElement dataCollectorElement)
        {
            XAttribute friendlyNameAttribute = dataCollectorElement.Attribute(FriendlyNameAttributeName);
            if (friendlyNameAttribute != null)
            {
                return IsFriendlyMsCodeCoverage(friendlyNameAttribute.Value);
            }

            XAttribute uriAttribute = dataCollectorElement.Attribute(UriAttributeName);
            return uriAttribute != null && IsMsCodeCoverageUri(uriAttribute.Value);
        }

        public static bool IsFriendlyMsCodeCoverage(string friendlyName)
            => friendlyName == MsDataCollectorFriendlyName;

        public static bool IsMsCodeCoverageUri(string uri) => uri == MsDataCollectorUri;

        public static XElement FindMsDataCollector(XElement dataCollectors)
            => dataCollectors.Elements().FirstOrDefault(IsMsDataCollector);
    }
}