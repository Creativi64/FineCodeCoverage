using System;
using System.IO;
using System.Linq;
using FineCodeCoverage.Collection.TestingPlatform.TUnit;
using NUnit.Framework;

namespace FineCodeCoverageTests
{
    public class CommandLineParser_Tests
    {
        private CommandLineParser commandLineParser;

        [SetUp]
        public void SetUp() => this.commandLineParser = new CommandLineParser();

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase("\r\n\t ")]
        public void Should_Return_Empty_When_No_Arguments(string argumentsString)
        {
            CommandLineParseResult result = this.commandLineParser.Parse(argumentsString);

            Assert.That(result, Is.SameAs(CommandLineParseResult.Empty));
        }

        [Test]
        public void Should_Parse_A_Single_Option_Without_Arguments()
        {
            CommandLineParseResult result = this.commandLineParser.Parse("--coverage");

            Assert.Multiple(() =>
            {
                Assert.That(result.Options.Count, Is.EqualTo(1));
                Assert.That(result.Options[0].Name, Is.EqualTo("coverage"));
                Assert.That(result.Options[0].Arguments, Is.Empty);
                Assert.That(result.HasError, Is.False);
            });
        }

        [TestCase("--coverage-output-format cobertura")]
        [TestCase("--coverage-output-format:cobertura")]
        [TestCase("--coverage-output-format=cobertura")]
        public void Should_Parse_An_Option_With_A_Value(string argumentsString)
        {
            CommandLineParseResult result = this.commandLineParser.Parse(argumentsString);

            Assert.Multiple(() =>
            {
                Assert.That(result.Options.Count, Is.EqualTo(1));
                Assert.That(result.Options[0].Name, Is.EqualTo("coverage-output-format"));
                Assert.That(result.Options[0].Arguments, Is.EqualTo(new[] { "cobertura" }));
            });
        }

        [Test]
        public void Should_Parse_Multiple_Options()
        {
            CommandLineParseResult result = this.commandLineParser.Parse("--coverage --coverage-output-format cobertura --minimum-expected-tests 5");

            Assert.Multiple(() =>
            {
                Assert.That(result.Options.Select(o => o.Name), Is.EqualTo(new[] { "coverage", "coverage-output-format", "minimum-expected-tests" }));
                Assert.That(result.Options[0].Arguments, Is.Empty);
                Assert.That(result.Options[1].Arguments, Is.EqualTo(new[] { "cobertura" }));
                Assert.That(result.Options[2].Arguments, Is.EqualTo(new[] { "5" }));
            });
        }

        [Test]
        public void Should_Parse_A_Quoted_Path_With_Spaces_As_A_Single_Argument()
        {
            CommandLineParseResult result = this.commandLineParser.Parse("--coverage-settings \"C:\\Path With Spaces\\coverage.config\"");

            Assert.Multiple(() =>
            {
                Assert.That(result.Options.Count, Is.EqualTo(1));
                Assert.That(result.Options[0].Name, Is.EqualTo("coverage-settings"));
                Assert.That(result.Options[0].Arguments, Is.EqualTo(new[] { "C:\\Path With Spaces\\coverage.config" }));
            });
        }

        [Test]
        public void Should_Parse_A_Value_Spanning_Multiple_Lines()
        {
            // an msbuild property is commonly authored across lines for readability
            string argumentsString = @"
                --coverage
                --coverage-output-format cobertura
                --minimum-expected-tests 3
            ";

            CommandLineParseResult result = this.commandLineParser.Parse(argumentsString);

            Assert.Multiple(() =>
            {
                Assert.That(result.Options.Select(o => o.Name), Is.EqualTo(new[] { "coverage", "coverage-output-format", "minimum-expected-tests" }));
                Assert.That(result.Options[1].Arguments, Is.EqualTo(new[] { "cobertura" }));
                Assert.That(result.Options[2].Arguments, Is.EqualTo(new[] { "3" }));
            });
        }

        [Test]
        public void Should_Expand_A_Response_File()
        {
            string responseFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.rsp");
            File.WriteAllText(responseFilePath, "# a comment\r\n--coverage\r\n--coverage-output-format cobertura\r\n");
            try
            {
                CommandLineParseResult result = this.commandLineParser.Parse($"--minimum-expected-tests 1 @{responseFilePath}");

                Assert.Multiple(() =>
                {
                    Assert.That(result.HasError, Is.False);
                    Assert.That(result.Options.Select(o => o.Name), Is.EqualTo(new[] { "minimum-expected-tests", "coverage", "coverage-output-format" }));
                    Assert.That(result.Options[2].Arguments, Is.EqualTo(new[] { "cobertura" }));
                });
            }
            finally
            {
                File.Delete(responseFilePath);
            }
        }

        [Test]
        public void Should_Error_When_A_Response_File_Does_Not_Exist()
        {
            CommandLineParseResult result = this.commandLineParser.Parse($"--coverage @{Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())}.rsp");

            Assert.That(result.HasError, Is.True);
        }

        [Test]
        public void Should_Not_StackOverflow_For_A_Realistic_Property_Value()
        {
            // Parse(string) used to call itself
            Assert.That(
                () => this.commandLineParser.Parse("--coverage --coverage-output \"C:\\out\\coverage.cobertura.xml\""),
                Throws.Nothing);
        }
    }
}
