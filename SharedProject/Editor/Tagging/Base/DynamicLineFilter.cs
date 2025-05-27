using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using FineCodeCoverage.Editor.DynamicCoverage;
using FineCodeCoverage.Output;

namespace FineCodeCoverage.Editor.Tagging.Base
{
    [Export(typeof(IDynamicLineFilter))]
    internal class DynamicLineFilter : IDynamicLineFilter
    {
        private readonly IReportViews reportViews;

        [ImportingConstructor]
        public DynamicLineFilter(
            IReportViews reportViews
        )
        {
            this.reportViews = reportViews;
            reportViews.Changed += this.ReportViews_Changed;
        }

        private void ReportViews_Changed(object sender, ReportViewChangedEventArgs e)
        {
            if (e.ChangesetChanged)
            {
                this.shouldGetChangeset = true;
                FilterChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private IChangeset changeset = null;
        private bool shouldGetChangeset = true;

        private IChangeset GetChangeset()
        {
            if (this.shouldGetChangeset)
            {
                this.changeset = this.reportViews.GetChangeset();
                this.shouldGetChangeset = false;
            }

            return this.changeset;
        }

        public event EventHandler FilterChanged;

        public Func<IDynamicLine, bool> GetFileFilter(string filePath)
        {
            IChangeset changeset = this.GetChangeset();
            if (changeset == null)
            {
                return (_) => true;
            }

            List<int> lineNumbers = changeset.GetLineNumbers(filePath);
            return (dynamicLine) => lineNumbers.Contains(dynamicLine.OriginalLineNumber + 1);
        }
    }
}