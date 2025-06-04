using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.VisualStudio.Imaging.Interop;

namespace FineCodeCoverage.Output
{
    public abstract class ReportTreeItemBase : VisualStudioTreeItemBase
    {
        private bool _isExpanded;
        private string _name;
        private int _coverableLines;
        private int _coveredLines;
        private int _partialLines;
        private int _cyclomaticComplexity;
        private int _npathComplexity;
        private decimal _crapScore;
        private int _totalBranches;
        private int _coveredBranches;
        private int _notCoveredLines;

        protected ObservableCollection<ReportTreeItemBase> ObservableChildren { get; } = new ObservableCollection<ReportTreeItemBase>();

        protected ReportTreeItemBase() => this.Children = this.ObservableChildren;
        public IReadOnlyList<ReportTreeItemBase> ReportChildren => this.ObservableChildren;

        public abstract ImageMoniker ImageMoniker { get; }

        public string Name
        {
            get => this._name;
            set => this.Set(ref this._name, value);
        }

        public int CoverableLines
        {
            get => this._coverableLines;
            set => this.Set(ref this._coverableLines, value);
        }

        public int CoveredLines
        {
            get => this._coveredLines;
            set => this.Set(ref this._coveredLines, value);
        }

        public int NotCoveredLines
        {
            get => this._notCoveredLines;
            set => this.Set(ref this._notCoveredLines, value);
        }

        public int PartialLines
        {
            get => this._partialLines;
            set => this.Set(ref this._partialLines, value);
        }

        public int CyclomaticComplexity
        {
            get => this._cyclomaticComplexity;
            set => this.Set(ref this._cyclomaticComplexity, value);
        }

        public int NPathComplexity
        {
            get => this._npathComplexity;
            set => this.Set(ref this._npathComplexity, value);
        }

        public decimal CrapScore
        {
            get => this._crapScore;
            set => this.Set(ref this._crapScore, value);
        }

        public int TotalBranches
        {
            get => this._totalBranches;
            set => this.Set(ref this._totalBranches, value);
        }

        public int CoveredBranches
        {
            get => this._coveredBranches;
            set => this.Set(ref this._coveredBranches, value);
        }

        public override bool IsExpanded
        {
            get => this._isExpanded;
            set => this.Set(ref this._isExpanded, value);
        }

        public int NotCoveredBranches => this.TotalBranches - this.CoveredBranches;

        internal static double SharedAdditionalAdjustment { get; set; } = 26;
        // crisp image width and margin
        protected override double AdditionalAdjustment => SharedAdditionalAdjustment;
    }
}