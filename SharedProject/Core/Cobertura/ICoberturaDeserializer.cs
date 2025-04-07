namespace FineCodeCoverage.Engine.Cobertura
{
    internal interface ICoberturaDeserializer
    {
        CoberturaReport Deserialize(string xmlFile);
    }
}
