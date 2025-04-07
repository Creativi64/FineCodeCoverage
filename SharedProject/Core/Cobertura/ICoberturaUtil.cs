using FineCodeCoverage.Engine.Model;

namespace FineCodeCoverage.Engine.Cobertura
{
    interface ICoberturaUtil
    {
        IFileLineCoverage ProcessCoberturaXml(string xmlFile);
	}
}