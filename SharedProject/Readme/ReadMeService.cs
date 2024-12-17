using System;
using System.IO;
using Markdig.Renderers.Normalize;
using Markdig.Syntax.Inlines;
using System.Reflection;
using Markdig.Syntax;
using System.Linq;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using Markdig;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Output;
using FineCodeCoverage.Options;
using Microsoft.VisualStudio.Settings;

namespace FineCodeCoverage.Readme
{
    [Export(typeof(IReadMeService))]
    [Export(typeof(IReadMeMarkdownViewModel))]
    internal class ReadMeService : IReadMeService, IReadMeMarkdownViewModel
    {
        private readonly WritableSettingsStore writableUserSettingsStore;
        private readonly IProcess process;
        private readonly IToolWindowService toolWindowService;
        private string markdown;
        private const string readMeShowCollection = "FCCReadmeShowCollection";
        private const string readMeShownProperty = "FCCReadmeShown";

        public event EventHandler ReadMeShown;

        [ImportingConstructor]
        public ReadMeService(
            IProcess process,
            IToolWindowService toolWindowService,
            IWritableUserSettingsStoreProvider writableUserSettingsStoreProvider
        )
        {
            this.writableUserSettingsStore = writableUserSettingsStoreProvider.Provide();
            this.HasShownReadMe = this.writableUserSettingsStore.GetBoolean(readMeShowCollection, readMeShownProperty, false);
            this.process = process;
            this.toolWindowService = toolWindowService;
        }

        // alternative is html
        // Markdown.ToHtml(markdownDocument);
        public string MarkdownString
        {
            get
            {
                if(this.markdown == null)
                {
                    FileInfo readmeFile = GetReadMeFile();
                    MarkdownDocument markdownDocument = GetMarkdownDocument(readmeFile);
                    FixPaths(markdownDocument, readmeFile.Directory);

                    // the alternative to this is to not use the MarkdownViewer control
                    // and instead construct the FlowDocument manually
                    // https://github.com/Kryptos-FR/markdig.wpf/blob/3dced7721c9245b618ce62732d4e078b38d22a89/src/Markdig.Wpf/MarkdownViewer.cs#L84

                    this.markdown = MardownDocumentToString(markdownDocument);
                }

                return this.markdown;
            }
        }

        public MarkdownPipeline MarkdownPipeline { get; } = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        public bool HasShownReadMe { get; private set; }

        public void ShowReadMe()
        {
            if (!this.HasShownReadMe)
            {
                if(!this.writableUserSettingsStore.CollectionExists(readMeShowCollection))
                {
                    this.writableUserSettingsStore.CreateCollection(readMeShowCollection);
                }
                this.writableUserSettingsStore.SetBoolean(readMeShowCollection, readMeShownProperty, true);
            }
            this.HasShownReadMe = true;
            ReadMeShown?.Invoke(this, EventArgs.Empty);
            
            _ = this.toolWindowService.ShowToolWindowAsync(typeof(ReadmeToolWindow), 0, true);
        }

        public void LinkClicked(string url)
        {
            if (IsRelativePath(url))
            {
                url = "https://github.com/FortuneN/FineCodeCoverage/blob/master/" + url;
            }

            this.process.Start(url);
        }

        private static bool IsRelativePath(string url) => Uri.IsWellFormedUriString(url, UriKind.Relative);

        private static bool IsYoutubeImage(string url) => url.StartsWith("https://img.youtube.com");

        public void ImageClicked(string url)
        {
            if (IsYoutubeImage(url))
            {
                this.process.Start(YoutubeImageToYoutubeVideo(url));
            }
        }

        private static string YoutubeImageToYoutubeVideo(string url)
        {
            string[] segments = url.Split('/');
            string videoId = segments[segments.Length - 2];
            return $"https://youtu.be/{videoId}";
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
            IEnumerable<LinkInline> assets = markdownDocument.Descendants<LinkInline>().Where(linkInline => linkInline.IsImage && linkInline.Url != null && Uri.IsWellFormedUriString(linkInline.Url, UriKind.Relative));
            foreach (LinkInline asset in assets)
            {
                string combinedPath = Path.Combine(readMeDirectory.FullName, asset.Url);
                asset.Url = new Uri(combinedPath).AbsoluteUri;
            }
        }

        private static FileInfo GetReadMeFile()
        {
            DirectoryInfo dir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;
            return dir.EnumerateFiles("README.md", SearchOption.AllDirectories).FirstOrDefault();
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
