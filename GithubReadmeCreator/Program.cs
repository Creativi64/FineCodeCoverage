using System.IO;

namespace GithubReadmeCreator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var readmeOptionsReplacer = new ReadmeOptionsReplacer();
            var replacedReadme = readmeOptionsReplacer.ReplaceReadMeMarkerWithOptionsTable(args[0], args[1]);
            File.WriteAllText(args[2], replacedReadme);
        }
    }
}
