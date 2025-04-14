using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Core.Utilities.Solution;
using FineCodeCoverage.Output;
using Moq;
using NUnit.Framework;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FineCodeCoverageTests
{
    class SolutionOptions_Tests
    {
        [Test]
        public async Task Should_Get_Keys_From_Options_Async()
        {
            var solutionOptions = new SolutionOptions(new ISolutionOption[] { CreateOption("key1"), CreateOption("key2") }, new Mock<ISolutionEvents>().Object);
            
            var keys = await solutionOptions.GetKeysAsync();
            
            Assert.That(keys, Is.EqualTo(new string[] { "key1", "key2" }));

            ISolutionOption CreateOption(string key)
            {
                var mockSolutionOption = new Mock<ISolutionOption>();
                mockSolutionOption.SetupGet(o => o.Key).Returns(key);
                return mockSolutionOption.Object;
            }
        }

        [Test]
        public void Should_Delegate_Load_To_Option()
        {
            var mockSolutionOption = new Mock<ISolutionOption>();
            mockSolutionOption.SetupGet(o => o.Key).Returns("key");
            var solutionOptions = new SolutionOptions(new ISolutionOption[] {mockSolutionOption.Object }, new Mock<ISolutionEvents>().Object);

            var stream = new MemoryStream();
            solutionOptions.LoadOptions("key", stream);

            mockSolutionOption.Verify(o => o.Load(stream));
        }

        [Test]
        public void Should_Delegate_Save_To_Option()
        {
            var mockSolutionOption = new Mock<ISolutionOption>();
            mockSolutionOption.SetupGet(o => o.Key).Returns("key");
            var solutionOptions = new SolutionOptions(new ISolutionOption[] { mockSolutionOption.Object }, new Mock<ISolutionEvents>().Object);

            var stream = new MemoryStream();
            solutionOptions.SaveOptions("key", stream);

            mockSolutionOption.Verify(o => o.Save(stream));
        }

        [Test]
        public void Should_Call_Unloaded_On_Each_Option_After_Solution_Closing()
        {
            var mockSolutionEvents = new Mock<ISolutionEvents>();
            var mockSolutionOption = new Mock<ISolutionOption>();
            var mockSolutionOption2 = new Mock<ISolutionOption>();
            var solutionOptions = new SolutionOptions(
                new ISolutionOption[] { mockSolutionOption.Object,mockSolutionOption2.Object }, mockSolutionEvents.Object);

            mockSolutionEvents.Raise(se => se.AfterClosing += null, EventArgs.Empty);

            mockSolutionOption.Verify(so => so.Unloaded());
            mockSolutionOption2.Verify(so => so.Unloaded());
        }
    }
    class ReportViewSolutionOption_Tests
    {
        [Test]
        public void ReportViewSolutionOption_Default_Should_Have_ReportStyle_Assembly_ReportContentType_Full()
        {
            var reportViewSolutionOptionValueDefault = ReportViewSolutionOptionValue.Default;
            AssertEqual(reportViewSolutionOptionValueDefault, new ReportViewSolutionOptionValue
            {
                ReportContent = ReportContentType.Full,
                ReportStyle = ReportStyle.Assembly
            });
        }

        [Test]
        public void Should_Have_Value_Equal_To_GetDefaultValue()
        {
            var option = new ReportViewSolutionOption(null);
            AssertValueDefault(option);
        }

        private static void AssertEqual(ReportViewSolutionOptionValue first, ReportViewSolutionOptionValue second)
        {
            Assert.That(first, Is.EqualTo(second).Using<ReportViewSolutionOptionValue>(IsEqual));
        }
        private static void AssertValueDefault(ReportViewSolutionOption option)
        {
            AssertEqual(option.Value, ReportViewSolutionOptionValue.Default);
        }
        private static bool IsEqual(ReportViewSolutionOptionValue x, ReportViewSolutionOptionValue y)
        {
            return x.ReportStyle == y.ReportStyle &&
                    x.ReportContent == y.ReportContent &&
                    x.SelectedBranchName == y.SelectedBranchName &&
                    x.SelectedRepository == y.SelectedRepository;
        }

        [Test]
        public void Should_Be_Different_Default_Each_Time()
        {
            Assert.That(ReportViewSolutionOptionValue.Default, Is.Not.SameAs(ReportViewSolutionOptionValue.Default));
        }

        [Test]
        public void Load_Should_JSONConvert_The_Stream_To_Value()
        {
            string deserialized = "Deserialized";
            var mockJsonConvertService = new Mock<IJsonConvertService>();
            var deserializedReportViewSolutionOptionValue = new ReportViewSolutionOptionValue
            {
                ReportStyle = ReportStyle.Source,
                ReportContent = ReportContentType.Full
            };
            mockJsonConvertService.Setup(jsonConvertService => jsonConvertService.DeserializeObject(deserialized, typeof(ReportViewSolutionOptionValue))).Returns(deserializedReportViewSolutionOptionValue);
            var option = new ReportViewSolutionOption(mockJsonConvertService.Object);

            
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(deserialized)))
            {
                option.Load(stream);
            }
            Assert.That(option.Value, Is.SameAs(deserializedReportViewSolutionOptionValue));
        }

        private string Save(Action<ReportViewSolutionOption> setup = null)
        {
            var mockJsonConvertService = new Mock<IJsonConvertService>();
            var value = new ReportViewSolutionOptionValue
            {
                ReportStyle = ReportStyle.Source,
                ReportContent = ReportContentType.Full
            };
            mockJsonConvertService.Setup(jsonConvertService => jsonConvertService.SerializeObject(value)).Returns("deserialized");
            var option = new ReportViewSolutionOption(mockJsonConvertService.Object);
            option.Value = value;

            setup?.Invoke(option);

            using (var stream = new MemoryStream())
            {
                option.Save(stream);
                stream.Position = 0;
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
            
        }

        [Test]
        public void Save_Should_Write_The_Serialized_Value_To_The_Stream()
        {
            Assert.That(Save(), Is.EqualTo("deserialized"));
        }

        [Test]
        public void Unloaded_Should_Set_The_Value_To_The_GetDefaultValue()
        {
            ReportViewSolutionOption option = new ReportViewSolutionOption(null);
            option.Value = new ReportViewSolutionOptionValue
            {
                ReportContent = ReportContentType.Full,
                ReportStyle = ReportStyle.Source,
            };
            option.Unloaded();
            AssertValueDefault(option);
        }

        [Test]
        public void Unloaded_Should_Raise_The_Unloaded_Event()
        {
            var raisedEvent = false;
            ReportViewSolutionOption option = new ReportViewSolutionOption(null);
            option.UnloadedEvent += (_, __) => raisedEvent = true;
            option.Unloaded();

            Assert.IsTrue(raisedEvent);
        }

        [Test]
        public void Should_Load_What_Saved()
        {
            var jsonConvertService = new JsonConvertService();
            var option = new ReportViewSolutionOption(jsonConvertService);
            var value = new ReportViewSolutionOptionValue
            {
                ReportContent = ReportContentType.Changeset,
                ReportStyle = ReportStyle.Source,
                SelectedBranchName = "branch",
                SelectedRepository = "repo"
            };
            option.Value = value;
            using (var stream = new MemoryStream())
            {
                option.Save(stream);
                stream.Position = 0;
                option.Load(stream);
                AssertEqual(option.Value, value);
            }
        }
    }
}
