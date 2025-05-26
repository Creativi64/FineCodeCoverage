using FineCodeCoverage.Core.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Documents;

namespace FineCodeCoverage.Readme
{
    [Export(typeof(IReadMeMarkdownViewModel))]
    internal class ReadMeMarkdownViewModel : IReadMeMarkdownViewModel
    {
        private readonly IProcess process;
        private readonly ITemplatedReadmeProvider readmeProvider;
        private readonly IFCCMarkdownFlowDocumentProvider fccMarkdownFlowDocumentProvider;

        [ImportingConstructor]
        public ReadMeMarkdownViewModel(
            IProcess process,
            ITemplatedReadmeProvider readmeProvider,
            IFCCMarkdownFlowDocumentProvider fccMarkdownFlowDocumentProvider
            )
        {
            this.process = process;
            this.readmeProvider = readmeProvider;
            this.fccMarkdownFlowDocumentProvider = fccMarkdownFlowDocumentProvider;
        }

        private FlowDocument GetFlowDocument()
        {
            var templatedReadmeInfo = this.readmeProvider.GetTemplatedReadme();
            var flowDocumentElementMarkers = fccMarkdownFlowDocumentProvider.Provide(
                templatedReadmeInfo,
                "FCCOptionsTable"
                )();
            this.ElementAndMarkers = flowDocumentElementMarkers.ElementAndMarkers;
            return flowDocumentElementMarkers.FlowDocument;
        }

        public FlowDocument FlowDocument
        {
            get => GetFlowDocument();
        }

        public IReadOnlyList<ElementAndMarker> ElementAndMarkers { get; private set; }


        public void LinkClicked(string url)
        {
            this.process.Start(url);
        }
    }
}
