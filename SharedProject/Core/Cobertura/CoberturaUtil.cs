using System.Linq;
using System.Collections.Generic;
using FineCodeCoverage.Engine.Model;
using System.ComponentModel.Composition;
using FineCodeCoverage.Core.Utilities;

namespace FineCodeCoverage.Engine.Cobertura
{
    [Export(typeof(ICoberturaUtil))]
	internal class CoberturaUtil:ICoberturaUtil
    {
        private readonly ICoberturaDeserializer coberturaDeserializer;
        private readonly IFileLineCoverageFactory fileLineCoverageFactory;
        private CoberturaReport coberturaReport;
        private IFileLineCoverage fileLineCoverage;

        private class FileLine : ILine
        {
			public FileLine(Line line)
			{
                CoverageType = GetCoverageType(line);
				Number = line.Number;
            }

            private static CoverageType GetCoverageType(Line line)
            {
                var lineConditionCoverage = line.ConditionCoverage?.Trim();

                var coverageType = CoverageType.NotCovered;

                if (line.Hits > 0)
                {
                    coverageType = CoverageType.Covered;

                    if (!string.IsNullOrWhiteSpace(lineConditionCoverage) && !lineConditionCoverage.StartsWith("100"))
                    {
                        coverageType = CoverageType.Partial;
                    }
                }
                return coverageType;
            }
            public int Number { get; }
            public CoverageType CoverageType { get; }
        }

        [ImportingConstructor]
		public CoberturaUtil(
			ICoberturaDeserializer coberturaDeserializer,
            IFileRenameListener fileRenameListener,
			IFileLineCoverageFactory fileLineCoverageFactory
        )
		{
            fileRenameListener.ListenForFileRename((oldFile, newFile) => fileLineCoverage?.UpdateRenamed(oldFile, newFile));
            this.coberturaDeserializer = coberturaDeserializer;
            this.fileLineCoverageFactory = fileLineCoverageFactory;
        }

		public IFileLineCoverage ProcessCoberturaXml(string xmlFile)
		{
			fileLineCoverage = fileLineCoverageFactory.Create();

			coberturaReport = coberturaDeserializer.Deserialize(xmlFile);

            AddThenSort();
            return fileLineCoverage;
		}

        private void AddThenSort()
        {
            foreach (var package in coberturaReport.Packages)
            {
                foreach (var classs in package.Classes)
                {
                    fileLineCoverage.Add(classs.Filename, classs.Lines.Select(l => new FileLine(l)).Cast<ILine>());
                }
            }

            fileLineCoverage.Sort();
        }
    }
}