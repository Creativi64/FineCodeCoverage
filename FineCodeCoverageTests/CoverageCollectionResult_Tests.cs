using System;
using FineCodeCoverage.Collection.CoverageProjectManagement;
using FineCodeCoverage.Collection.Engine;
using Moq;
using NUnit.Framework;

namespace FineCodeCoverageTests
{
    public class CoverageCollectionResult_Tests
    {
        [Test]
        public void Should_Not_Assume_An_Attachment_Confirms_Every_VsTest_Project()
        {
            var result = new CoverageCollectionResult(new[] { "merged.cobertura.xml" },
                new[] { Mock.Of<ICoverageProject>(), Mock.Of<ICoverageProject>() });
            Assert.That(result.ProjectResults, Has.All.Matches<ProjectCoverageResult>(project => project.State == ProjectCoverageState.Unknown));
            Assert.That(result.Summary, Does.Contain("2 project(s) could not be verified individually"));
        }

        [Test]
        public void Should_Preserve_Project_Status_When_Combining_Collectors()
        {
            ICoverageProject mtp = Mock.Of<ICoverageProject>(project => project.ProjectName == "MTP");
            ICoverageProject vstest = Mock.Of<ICoverageProject>(project => project.ProjectName == "VSTest");
            var mtpResult = new CoverageCollectionResult(new[] { "mtp.xml" }, new[] { mtp },
                new[] { new ProjectCoverageResult(mtp, ProjectCoverageState.Collected, "Collected.") });
            var vsTestResult = new CoverageCollectionResult(Array.Empty<string>(), new[] { vstest });

            CoverageCollectionResult combined = CoverageCollectionResult.Combine(mtpResult, vsTestResult);

            Assert.That(combined.Summary, Is.EqualTo("Coverage collected for 1 of 2 test projects."));
            Assert.That(combined.Details, Does.Contain("VSTest: No coverage file was produced."));
        }
    }
}
