using System.IO;
using AutoMoq;
using FineCodeCoverage.Collection.CoverageToolOutput;
using FineCodeCoverage.Options;
using FineCodeCoverageTests.TestHelpers;
using NUnit.Framework;

namespace FineCodeCoverageTests.CoverageToolOutput_Tests
{
    class AppOptionsCoverageToolOutputFolderSolutionProvider_Tests
    {
        private AutoMoqer mocker;

        [SetUp]
        public void SetUp()
        {
            mocker = new AutoMoqer();

        }

        [TestCase(null)]
        [TestCase("")]
        public void Should_Return_Null_Without_Getting_Solution_Folder_When_AppOption_FCCSolutionOutputDirectoryName_NotSet(string optionValue)
        {
            var mockOutputOptionsProvider = mocker.GetMock<IOptionsProvider<OutputOptions>>();
            mockOutputOptionsProvider.Setup(aop => aop.Get()).Returns(new OutputOptions { FCCSolutionOutputDirectoryName  = optionValue});

            var provider = mocker.Create<AppOptionsCoverageToolOutputFolderSolutionProvider>();
            var providedSolutionFolder = false;
            Assert.Null(provider.Provide(() =>
            {
                providedSolutionFolder = true;
                return null;
            }));

            Assert.False(providedSolutionFolder);
        }

        [Test]
        public void Should_Return_Null_If_No_Solution_Folder_Provided_To_It()
        {
            var mockOutputOptionsProvider = mocker.GetMock<IOptionsProvider<OutputOptions>>();
            mockOutputOptionsProvider.Setup(aop => aop.Get()).Returns(new OutputOptions { FCCSolutionOutputDirectoryName = "Value"});
            var provider = mocker.Create<AppOptionsCoverageToolOutputFolderSolutionProvider>();
            Assert.Null(provider.Provide(() => null));
        }

        [Test]
        public void Should_Combine_The_Solution_Folder_With_FCCSolutionOutputDirectoryName()
        {
            var mockOutputOptionsProvider = mocker.GetMock<IOptionsProvider<OutputOptions>>();
            mockOutputOptionsProvider.Setup(aop => aop.Get()).Returns(new OutputOptions { FCCSolutionOutputDirectoryName = "FCCOutput" });
            var provider = mocker.Create<AppOptionsCoverageToolOutputFolderSolutionProvider>();
            Assert.AreEqual(provider.Provide(() => "SolutionFolder"), Path.Combine("SolutionFolder", "FCCOutput"));
        }

        [Test]
        public void Should_Have_First_Order()
        {
            MefOrderAssertions.TypeHasExpectedOrder(typeof(AppOptionsCoverageToolOutputFolderSolutionProvider), 1);
        }
    }
}
