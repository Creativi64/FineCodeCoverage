namespace OptionsExtractor
{
    internal interface IStringReplacer
    {
        string Replace(string originalReadme, string marker, string replacement);
    }
}
