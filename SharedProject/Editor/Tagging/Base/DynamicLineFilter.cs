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
        private readonly IReportViews _reportViews;
        private IChangeset _changeset = null;
        private bool _shouldGetChangeset = true;

        [ImportingConstructor]
        public DynamicLineFilter(
            IReportViews reportViews
        )
        {
            this._reportViews = reportViews;
            reportViews.Changed += this.ReportViews_Changed;
        }

        private void ReportViews_Changed(object sender, ReportViewChangedEventArgs e)
        {
            if (!e.ChangesetChanged)
            {
                return;
            }

            this._shouldGetChangeset = true;
            FilterChanged?.Invoke(this, EventArgs.Empty);
        }

        private IChangeset GetChangeset()
        {
            if (this._shouldGetChangeset)
            {
                this._changeset = this._reportViews.GetChangeset();
                this._shouldGetChangeset = false;
            }

            return this._changeset;
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
