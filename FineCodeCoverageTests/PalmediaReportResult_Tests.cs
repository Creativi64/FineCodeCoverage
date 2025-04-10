using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine.Cobertura;
using FineCodeCoverage.Engine.ReportGenerator;
using Moq;
using NUnit.Framework;
using Palmmedia.ReportGenerator.Core.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace FineCodeCoverageTests
{
    internal class PalmediaTestReportResult
    {
        public PalmediaTestReportResult(PalmmediaReportResult palmediaReportResult, string tempDirectory)
        {
            PalmediaReportResult = palmediaReportResult;
            TempDirectory = tempDirectory;
        }

        public PalmmediaReportResult PalmediaReportResult { get; }
        public string TempDirectory { get; }
    }

    internal static class PalmmediaTestReportGenerator
    {
        public static string TempDirectory { get; private set; }
        public static PalmmediaReportResult GenerateFromCobertura(Func<string,string> generateCobertura)
        {
            var reportGenerator = new PalmmediaReportGenerator(new Mock<IHtmlFilesToFolder>().Object);
            TempDirectory = new FileUtil().CreateTempDirectory();
            var cobertura = generateCobertura(TempDirectory);
            var coberturaFilePath = WriteCobertura(cobertura);
            
            LoggerFactory.Configure((palmmediaVerbosityLevel, message) =>
            {

            });
            LoggerFactory.VerbosityLevel = Palmmedia.ReportGenerator.Core.Logging.VerbosityLevel.Verbose;
            var result =  reportGenerator.Generate(new List<string> { coberturaFilePath }, TempDirectory, new List<string> { "Cobertura" });
            return (PalmmediaReportResult)result;
        }

        public static PalmmediaReportResult GenerateFromCoberturaPath(Func<string,string> generateCoberturaFilePath)
        {
            Func<string,string> generateCobertura = (tmpDirPath) => File.ReadAllText(generateCoberturaFilePath(tmpDirPath));
            return GenerateFromCobertura(generateCobertura);
        }

        public static PalmmediaReportResult GenerateFromCoberturaReport(Func<string,CoberturaReport> generateCoberturaReport)
        {
            Func<string, string> generateCobertura = (tmpDirPath) =>
            {
                var coberturaReport = generateCoberturaReport(tmpDirPath);
                var stringBuilder = new StringBuilder();
                using (var xmlWriter = XmlWriter.Create(stringBuilder))
                {
                    var xmlSerializer = new XmlSerializer(typeof(CoberturaReport));

                    xmlSerializer.Serialize(xmlWriter, coberturaReport);
                }
                var cobertura = stringBuilder.ToString();
                // Insert DOCTYPE after the XML declaration
                string doctype = "<!DOCTYPE coverage SYSTEM \"http://cobertura.sourceforge.net/xml/coverage-04.dtd\">";
                int index = cobertura.IndexOf("?>") + 2;
                cobertura = cobertura.Insert(index, Environment.NewLine + doctype);
                return cobertura;
            };
            

            return GenerateFromCobertura(generateCobertura);
        }

        private static string WriteCobertura(string cobertura)
        {
            var coberturaPath = Path.Combine(TempDirectory, "initial-cobertura.xml");
            File.WriteAllText(coberturaPath, cobertura, Encoding.Unicode);
            return coberturaPath;
        }
    }

    internal class PalmediaReportResult_Tests
    {
        private Class CreateClass(string name, string fileName,List<Method> methods)
        {
            var lines = methods.SelectMany(m => m.Lines).ToList();
            return new Class { 
                Name = name,
                Filename = fileName,
                Methods = methods,
                Lines = lines
            };
        }

        private void AddData(CoberturaReport coberturaReport)
        {
            var totalLineCount = 0;
            var totalLineHits = 0;
            var totalBranchCount = 0;
            var totalBranchLineHits = 0;

            foreach (var package in coberturaReport.Packages)
            {
                var packageLineCount = 0;
                var packageLineHits = 0;
                var packageBranchCount = 0;
                var packageBranchLineHits = 0;
                foreach (var clss in package.Classes)
                {
                    var classLineCount = 0;
                    var classLineHits = 0;
                    var classBranchCount = 0;
                    var classBranchLineHits = 0;
                    foreach (var method in clss.Methods)
                    {
                        var lineCount = 0;
                        var lineHits = 0;
                        var branchLineCount = 0;
                        var branchLineHits = 0;
                        foreach (var line in method.Lines)
                        {
                            lineCount++;
                            var isBranch = line.Branch == "true";
                            if (isBranch)
                            {
                                branchLineCount++;
                            }
                            if (line.Hits > 0)
                            {
                                lineHits++;
                                if (isBranch)
                                {
                                    branchLineHits++;
                                }
                            }

                        }
                        var (lineRate, branchRate) = GetRates(lineCount, lineHits, branchLineCount, branchLineHits);
                        method.LineRate = lineRate;
                        method.BranchRate = branchRate;

                        classLineCount += lineCount;
                        classLineHits += lineHits;
                        classBranchCount += branchLineCount;
                        classBranchLineHits += branchLineHits;
                    }

                    var classRates = GetRates(classLineCount, classLineHits, classBranchCount, classBranchLineHits);
                    clss.LineRate = classRates.LineRate;
                    clss.BranchRate = classRates.BranchRate;

                    packageLineCount += classLineCount;
                    packageLineHits += classLineHits;
                    packageBranchCount += classBranchCount;
                    packageBranchLineHits += classBranchLineHits;
                }

                var packageRates = GetRates(packageLineCount, packageLineHits, packageBranchCount, packageBranchLineHits);
                package.LineRate = packageRates.LineRate;
                package.BranchRate = packageRates.BranchRate;

                totalLineCount += packageLineCount;
                totalLineHits += packageLineHits;
                totalBranchCount += packageBranchCount;
                totalBranchLineHits += packageBranchLineHits;
            }
            coberturaReport.LinesValid = totalLineCount;
            coberturaReport.LinesCovered = totalLineHits;
            var totalRates = GetRates(totalLineCount, totalLineHits, totalBranchCount, totalBranchLineHits);
            coberturaReport.LineRate = totalRates.LineRate;
            coberturaReport.BranchRate = totalRates.BranchRate;

            (float LineRate, float BranchRate) GetRates(int lineCount, int lineHits, int branchLineCount, int branchLineHits)
            {
                var lineRate = lineHits == 0 ? 0 : (float)lineHits / lineCount;
                var branchRate = branchLineCount == 0 ? 1 : (float)branchLineHits / branchLineCount;
                return (lineRate, branchRate);
            }
        }

        [TearDown]
        public void DeleteTempDirectory()
        {
            if (PalmmediaTestReportGenerator.TempDirectory != null)
            {
                Directory.Delete(PalmmediaTestReportGenerator.TempDirectory);
            }
        }

        [Test]
        public void Should_Reuse_CodeElements_Between_Assembly_And_Directories()
        {
            var result = PalmmediaTestReportGenerator.GenerateFromCoberturaReport(tmpDir =>
            {
                List<Method> methods = new List<Method>
                {
                    new Method
                    {
                        Name = "Method1",
                        Signature = "()",
                        Lines = new List<Line>
                        {
                            new Line
                            {
                                Number = 6,
                                Hits = 1,
                                Branch = "false"
                            },
                            new Line
                            {
                                Number = 8,
                                Hits = 1,
                                Branch = "false"
                            },
                        }
                    },
                    new Method
                    {
                        Name = "Method2",
                        Signature = "()",
                        Lines = new List<Line>
                        {
                            new Line
                            {
                                Number = 10,
                                Hits = 0,
                                Branch = "false"
                            },
                            new Line
                            {
                                Number = 12,
                                Hits = 0,
                                Branch = "false"
                            },
                        }
                    },

                };
                var classFileName = Path.Combine(tmpDir, "Class1.cs");
                File.WriteAllText(classFileName, "");
                var coberturaReport = new CoberturaReport
                {
                    Sources = new Sources
                    {

                    },
                    Packages = new List<Package>
                    {
                        new Package
                        {
                            Name = "TestPackage",
                            Classes = new List<Class>
                            {
                                CreateClass("Namespace.Class1",classFileName, methods)
                            }
                        }
                    }
                };

                AddData(coberturaReport);
                return coberturaReport;
            });
            var classCodeElements = result.Assemblies.ElementAt(0).Classes.ElementAt(0).CodeElements;
            // a list and not readonlycollection
            var sourceFileClassCodeElements = result.Directory.SourceFiles[0].Classes.ElementAt(0).CodeElements;
            Assert.That(classCodeElements, Has.Count.EqualTo(sourceFileClassCodeElements.Count));

            for(var i = 0; i<classCodeElements.Count; i++)
            {
                Assert.That(classCodeElements.ElementAt(i), Is.SameAs(sourceFileClassCodeElements.ElementAt(i)));
            }
        }
    }
}
