using System;
using System.Collections.Generic;
using AutoMoq;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine.Cobertura;
using FineCodeCoverage.Engine.Model;
using FineCodeCoverageTests.TestHelpers;
using Moq;
using NUnit.Framework;

namespace FineCodeCoverageTests
{
    public class CoberturaUtil_Tests
    {
        [Test]
        public void Should_Populate_And_Sort_FileLineCoverage_From_Deserialized_Report()
        {
            var autoMoqer = new AutoMoqer();
            var reportPath = "reportpath";
            
            var noHitsLine = new Line
            {
                Hits = 0,
                Number = 1
            };
            var partialHitsLine = new Line
            {
                Hits = 1,
                ConditionCoverage = "99% .....",
                Number = 2
            };
            var coveredLine = new Line
            {
                Hits = 1,
                ConditionCoverage = "100% .....",
                Number = 3
            };
            var noConditionCoverageLine = new Line
            {
                Hits = 1,
                Number = 4
            };
            var coverageReport = new CoberturaReport
            {
                Packages = new List<Package>
                {
                    new Package
                    {
                        Classes = new List<Class>
                        {
                            new Class
                            {
                                Filename = "filename",

                                    Lines = new List<Line>
                                    {
                                        noHitsLine,
                                        partialHitsLine,
                                        coveredLine,
                                        noConditionCoverageLine
                                    }
                                    
                            }
                        }
                    }
                }
            };
            autoMoqer.Setup<ICoberturaDeserializer, CoberturaReport>(x => x.Deserialize(reportPath)).Returns(coverageReport);
            var mockFileLineCoverage = new Mock<IFileLineCoverage>();
            var expectedLines = new List<ICoberturaLine>
            {
                CreateExpectedLine(1, CoverageType.NotCovered),
                CreateExpectedLine(2, CoverageType.Partial),
                CreateExpectedLine(3, CoverageType.Covered),
                CreateExpectedLine(4, CoverageType.Covered)
            };
            var sorted = false;
            mockFileLineCoverage.Setup(fileLineCoverage => fileLineCoverage.Add("filename", MoqMatchers.EnumerableExpected(
                expectedLines,
                (a, b) => a.Number == b.Number && a.CoverageType == b.CoverageType)
            )).Callback(() => Assert.That(sorted, Is.False));
            mockFileLineCoverage.Setup(fileLineCoverage => fileLineCoverage.Sort()).Callback(() => sorted = true);

            autoMoqer.Setup<IFileLineCoverageFactory, IFileLineCoverage>(fileLineCoverageFactory => fileLineCoverageFactory.Create())
                .Returns(mockFileLineCoverage.Object);
            var coberturaUtil = autoMoqer.Create<CoberturaUtil>();
            
            var processed = coberturaUtil.ProcessCoberturaXml(reportPath);


            Assert.That(processed, Is.SameAs(mockFileLineCoverage.Object));
            mockFileLineCoverage.VerifyAll();
        }

        [Test]
        public void Should_Update_FileLineCoverage_When_File_Renamed()
        {
            var autoMoqer = new AutoMoqer();
            var mockFileRenameListener = autoMoqer.GetMock<IFileRenameListener>();
            Action<string, string> fileRenamedCallback = null;
            mockFileRenameListener.Setup(fileRenameListener => fileRenameListener.ListenForFileRename(It.IsAny<Action<string,string>>()))
                .Callback<Action<string, string>>(action => fileRenamedCallback = action);  
                
            var coverageReport = new CoberturaReport
            {
                Packages = new List<Package>()
            };
            autoMoqer.Setup<ICoberturaDeserializer, CoberturaReport>(x => x.Deserialize(It.IsAny<string>())).Returns(coverageReport);
            var mockFileLineCoverage = new Mock<IFileLineCoverage>();
            autoMoqer.Setup<IFileLineCoverageFactory, IFileLineCoverage>(fileLineCoverageFactory => fileLineCoverageFactory.Create())
                .Returns(mockFileLineCoverage.Object);
            var coberturaUtil = autoMoqer.Create<CoberturaUtil>();

            coberturaUtil.ProcessCoberturaXml("");
            fileRenamedCallback("oldfile", "newfile");

            mockFileLineCoverage.Verify(fileLineCoverage => fileLineCoverage.UpdateRenamed("oldfile", "newfile"), Times.Once);

        }

        [Test]
        public void Should_Not_Throw_When_File_Renamed_And_No_Coverage()
        {
            var autoMoqer = new AutoMoqer();
            var mockFileRenameListener = autoMoqer.GetMock<IFileRenameListener>();
            mockFileRenameListener.Setup(fileRenameListener => fileRenameListener.ListenForFileRename(It.IsAny<Action<string, string>>()))
                .Callback<Action<string, string>>(action => action("",""));

            var coberturaUtil = autoMoqer.Create<CoberturaUtil>();


        }
        private static ICoberturaLine CreateExpectedLine(int number, CoverageType coverageType)
        {
            var mockLine = new Mock<ICoberturaLine>();
            mockLine.SetupGet(x => x.Number).Returns(number);
            mockLine.SetupGet(x => x.CoverageType).Returns(coverageType);
            return mockLine.Object;
        }
    }
}
