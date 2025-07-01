using FineCodeCoverage.Collection.CoverageProjectManagement.Settings;
using FineCodeCoverage.Utilities.Wrappers;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace FineCodeCoverageTests
{
    public class FCCSettingsFilesProvider_Tests
    {
        [Test]
        public void Should_Return_All_FCC_Options_In_Project_Folder_And_Ascendants_Top_Level_First()
        {
            var fccOptionsElements = Provide("<Root1></Root1>", "<Root2></Root2>");
            Assert.True(fccOptionsElements.Count == 2);
            Assert.True(fccOptionsElements[0].Name == "Root2");
            Assert.True(fccOptionsElements[1].Name == "Root1");
        }

        [Test]
        public void Should_Stop_At_TopLevel()
        {
            var fccOptionsElements = Provide("<Root1 topLevel='true'></Root1>", "<Root2></Root2>");
            Assert.True(fccOptionsElements.Count == 1);
            Assert.True(fccOptionsElements[0].Name == "Root1");
        }

        [Test]
        public void Should_Ignore_Exceptions()
        {
            var fccOptionsElements = Provide("<Bad", "<Root2></Root2>");
            Assert.True(fccOptionsElements.Count == 1);
            Assert.True(fccOptionsElements[0].Name == "Root2");
        }

        private List<XElement> Provide(string projectDirectoryFCCOptions, string solutionParentDirectoryFCCOptions)
        {
            var projectPath = "projectPath";
            var mockFileUtil = new Mock<IFileUtil>();
            var projectDirectoryFCCOptionsPath = Path.Combine(projectPath, FCCSettingsFilesProvider.FCCOptionsFileName);
            mockFileUtil.Setup(fileUtil => fileUtil.Exists(projectDirectoryFCCOptionsPath)).Returns(true);
            mockFileUtil.Setup(fileUtil => fileUtil.ReadAllText(projectDirectoryFCCOptionsPath)).Returns(projectDirectoryFCCOptions);
            
            var solutionPath = "Solution";
            var solutionDirectoryFCCOptionsPath = Path.Combine(solutionPath, FCCSettingsFilesProvider.FCCOptionsFileName);
            mockFileUtil.Setup(fileUtil => fileUtil.DirectoryParentPath(projectPath)).Returns(solutionPath);

            // will want a gap where it does not exist
            mockFileUtil.Setup(fileUtil => fileUtil.Exists(solutionDirectoryFCCOptionsPath)).Returns(false);

            var solutionParentPath = "SolutionParent";
            var solutionParentDirectoryFCCOptionsPath = Path.Combine(solutionParentPath, FCCSettingsFilesProvider.FCCOptionsFileName);
            mockFileUtil.Setup(fileUtil => fileUtil.DirectoryParentPath(solutionPath)).Returns(solutionParentPath);

            mockFileUtil.Setup(fileUtil => fileUtil.Exists(solutionParentDirectoryFCCOptionsPath)).Returns(true);
            mockFileUtil.Setup(fileUtil => fileUtil.ReadAllText(solutionParentDirectoryFCCOptionsPath)).Returns(solutionParentDirectoryFCCOptions);
            mockFileUtil.Setup(fileUtil => fileUtil.DirectoryParentPath(solutionParentPath)).Returns((string)null);


            var fccOptionsProvider = new FCCSettingsFilesProvider(mockFileUtil.Object);
            return fccOptionsProvider.Provide(projectPath);
        }
    }
}