using FineCodeCoverage.Engine.Model;
using Markdig.Renderers.Normalize;
using Markdig.Syntax.Inlines;
using Markdig.Syntax;
using OptionsExtractor;
using System;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Readme
{
    [Export(typeof(IReadmeProvider))]
    internal class ReadmeProvider : IReadmeProvider
    {
        private readonly IReadmeOptionsReplacer readmeOptionsReplacer;

        [ImportingConstructor]
        public ReadmeProvider(IReadmeOptionsReplacer readmeOptionsReplacer)
        {
            this.readmeOptionsReplacer = readmeOptionsReplacer;
        }

        public string GetReadme()
        {
            FileInfo readmeFile = GetReadMeTemplateFile();
            MarkdownDocument markdownDocument = GetMarkdownDocument(readmeFile);
            FixPaths(markdownDocument, readmeFile.Directory);

            return this.readmeOptionsReplacer.ReplaceReadMeMarkerWithOptionsTable(
                MardownDocumentToString(markdownDocument),
                "{{FCCOptionsTable}}",
                typeof(FCCPackage),
                typeof(CoverageSettings)
                );
        }

        private static string MardownDocumentToString(MarkdownDocument markdownDocument)
        {
            var stringWriter = new StringWriter();
            var normalizeRenderer = new NormalizeRenderer(stringWriter);
            _ = normalizeRenderer.Render(markdownDocument);
            return stringWriter.ToString();
        }

        private static void FixPaths(MarkdownDocument markdownDocument, DirectoryInfo readMeDirectory)
        {
            IEnumerable<LinkInline> assets = markdownDocument.Descendants<LinkInline>()
                .Where(linkInline => linkInline.IsImage && linkInline.Url != null && Uri.IsWellFormedUriString(linkInline.Url, UriKind.Relative));
            foreach (LinkInline asset in assets)
            {
                string combinedPath = Path.Combine(readMeDirectory.FullName, asset.Url);
                asset.Url = new Uri(combinedPath).AbsoluteUri;
            }
        }

        private static FileInfo GetReadMeTemplateFile()
        {
            DirectoryInfo dir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;
            return dir.EnumerateFiles("README-Template.md", SearchOption.AllDirectories).FirstOrDefault();
        }

        private static MarkdownDocument GetMarkdownDocument(FileInfo readmeFile)
        {
            string readMe = File.ReadAllText(readmeFile.FullName);
            readMe = RemoveAfterSupport(readMe);
            return Markdown.Parse(readMe);
        }

        private static string RemoveAfterSupport(string readMe)
        {
            /*
                Note that all information and links should be pertaining to the version of the extension that is being used.
                In particular, whenver a new youtube video is created the old one should remain.

                todo
                consider marking the readme with comments for sections that should not be removed
                https://stackoverflow.com/questions/4823468/comments-in-markdown
            */
            int supportIndex = readMe.IndexOf("## Please support the project");
            if (supportIndex != -1)
            {
                readMe = readMe.Substring(0, supportIndex);
            }

            return readMe;
        }
    }
}
