using FineCodeCoverage.Collection.CoverageProjectManagement;
using FineCodeCoverage.Collection.CoverageProjectManagement.Settings;
using FineCodeCoverageTests.TestHelpers;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FineCodeCoverageTests
{
    public class CoverageProjectSettingsProvider_Tests
    {
        [Test]
        public async Task Should_Return_The_FineCodeCoverage_Labelled_PropertyGroup_Async()
        {
            var coverageProjectSettingsProvider = new CoverageProjectSettingsProvider(null);
            var mockCoverageProject = new Mock<ICoverageProject>();
            var fccLabelledPropertyGroup = @"
    <PropertyGroup Label='FineCodeCoverage'>
        <Setting1/>
    </PropertyGroup>

";
            var projectFileXElement = XElement.Parse($@"
<Project>
    {fccLabelledPropertyGroup}
</Project>
");
            mockCoverageProject.Setup(cp => cp.ProjectFileXElement).Returns(projectFileXElement);
            var coverageProject = mockCoverageProject.Object;
            var coverageProjectSettings = await coverageProjectSettingsProvider.ProvideAsync(coverageProject);
            XmlAssert.NoXmlDifferences(coverageProjectSettings.ToString(), fccLabelledPropertyGroup);
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task Should_Return_Using_VsBuild_When_No_Labelled_PropertyGroup_Async(bool returnNull)
        {
            var mockCoverageProject = new Mock<ICoverageProject>();
            var coverageProjectGuid = Guid.NewGuid();
            mockCoverageProject.Setup(cp => cp.Id).Returns(coverageProjectGuid);
            var notFccLabelledPropertyGroup = @"
    <PropertyGroup Label='NotFineCodeCoverage'>
    </PropertyGroup>

";
            var projectFileXElement = XElement.Parse($@"
<Project>
    {notFccLabelledPropertyGroup}
</Project>
");
            mockCoverageProject.Setup(cp => cp.ProjectFileXElement).Returns(projectFileXElement);

            var mockVsBuildFCCSettingsProvider = new Mock<IVsBuildFCCSettingsProvider>();
            var settingsElementFromVsBuildFCCSettingsProvider = returnNull ? null : new XElement("Root");
            mockVsBuildFCCSettingsProvider.Setup(
                vsBuildFCCSettingsProvider =>
                vsBuildFCCSettingsProvider.GetSettingsAsync(coverageProjectGuid)
            ).ReturnsAsync(settingsElementFromVsBuildFCCSettingsProvider);

            var coverageProjectSettingsProvider = new CoverageProjectSettingsProvider(mockVsBuildFCCSettingsProvider.Object);
            
            var coverageProject = mockCoverageProject.Object;
            var coverageProjectSettings = await coverageProjectSettingsProvider.ProvideAsync(coverageProject);

            Assert.AreSame(settingsElementFromVsBuildFCCSettingsProvider, coverageProjectSettings);
        }
    }
}