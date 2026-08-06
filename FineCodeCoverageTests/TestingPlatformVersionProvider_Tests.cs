using System;
using System.IO;
using System.Reflection;
using FineCodeCoverage.Collection.TestingPlatform.TUnit;
using NUnit.Framework;

namespace FineCodeCoverageTests
{
    public class TestingPlatformVersionProvider_Tests
    {
        private TestingPlatformVersionProvider testingPlatformVersionProvider;

        [SetUp]
        public void SetUp() => this.testingPlatformVersionProvider = new TestingPlatformVersionProvider();

        [Test]
        public void Should_Return_Null_When_The_Testing_Platform_Assembly_Is_Not_Present()
            => Assert.That(this.testingPlatformVersionProvider.GetVersion(Path.GetTempPath()), Is.Null);

        [Test]
        public void Should_Return_Null_When_The_Output_Folder_Does_Not_Exist()
            => Assert.That(
                this.testingPlatformVersionProvider.GetVersion(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())),
                Is.Null);

        [Test]
        public void Should_Return_Null_For_A_Null_Output_Folder()
            => Assert.That(this.testingPlatformVersionProvider.GetVersion(null), Is.Null);

        /*
            The informational version is the package version.  FileVersion is not - Microsoft.Testing.Platform
            2.3.3 ships as FileVersion 2.300.326.37705 and 2.0.2 as 2.0.225.56104, so comparing FileVersion
            against 2.3.0 would wrongly accept 2.0.2 style versions.
        */
        [TestCase("2.3.3+44aa76e6a61d4908f06dfd77a51d4b7e3e7ce40f", 2, 3, 3)]
        [TestCase("2.0.2+88c9fc53a3b4f0fdf6b29a54e0e9f39dd44d9ede", 2, 0, 2)]
        [TestCase("2.3.0", 2, 3, 0)]
        [TestCase("3.0.0-preview.25123.1", 3, 0, 0)]
        public void Should_Parse_The_Package_Version_From_The_Informational_Version(
            string informationalVersion, int major, int minor, int build)
        {
            Version parsed = InvokeParse(informationalVersion);

            Assert.That(parsed, Is.EqualTo(new Version(major, minor, build)));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase("not-a-version")]
        public void Should_Return_Null_For_An_Unparseable_Informational_Version(string informationalVersion)
            => Assert.That(InvokeParse(informationalVersion), Is.Null);

        private static Version InvokeParse(string informationalVersion)
            => (Version)typeof(TestingPlatformVersionProvider)
                .GetMethod("ParseInformationalVersion", BindingFlags.NonPublic | BindingFlags.Static)
                .Invoke(null, new object[] { informationalVersion });
    }
}
