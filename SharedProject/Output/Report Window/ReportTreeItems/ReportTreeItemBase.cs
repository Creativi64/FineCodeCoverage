using System.Collections.ObjectModel;
using Microsoft.VisualStudio.Imaging.Interop;

namespace FineCodeCoverage.Output
{
    public abstract class ReportTreeItemBase : VisualStudioTreeItemBase
    {
        private bool _isExpanded;
        internal readonly ObservableCollection<ReportTreeItemBase> observableChildren = new ObservableCollection<ReportTreeItemBase>();
        protected ReportTreeItemBase()
        {
            this.Children = this.observableChildren;
        }

        public abstract ImageMoniker ImageMoniker { get; }
        private string _name;
        public string Name
        {
            get => this._name;
            set => this.Set(ref this._name, value);
        }

        private int _coverableLines;
        public int CoverableLines
        {
            get => this._coverableLines;
            set => this.Set(ref this._coverableLines, value);
        }

        private int _coveredLines;
        public int CoveredLines
        {
            get => this._coveredLines;
            set => this.Set(ref this._coveredLines, value);
        }

        private int _notCoveredLines;
        public int NotCoveredLines
        {
            get => this._notCoveredLines;
            set => this.Set(ref this._notCoveredLines, value);
        }

        private int _partialLines;
        public int PartialLines
        {
            get => this._partialLines;
            set => this.Set(ref this._partialLines, value);
        }

        private int cyclomaticComplexity;
        public int CyclomaticComplexity
        {
            get => this.cyclomaticComplexity;
            set => this.Set(ref this.cyclomaticComplexity, value);
        }


        private int npathComplexity;
        public int NPathComplexity
        {
            get => this.npathComplexity;
            set => this.Set(ref this.npathComplexity, value);
        }
        private decimal crapScore;
        public decimal CrapScore
        {
            get => this.crapScore;
            set => this.Set(ref this.crapScore, value);
        }

        private int totalBranches;
        public int TotalBranches
        {
            get => this.totalBranches;
            set => this.Set(ref this.totalBranches, value);
        }
        private int coveredBranches;
        public int CoveredBranches
        {
            get => this.coveredBranches;
            set => this.Set(ref this.coveredBranches, value);
        }

        public int NotCoveredBranches
        {
            get => this.TotalBranches - this.CoveredBranches;
        }
        public override bool IsExpanded
        {
            get => this._isExpanded;
            set => this.Set(ref this._isExpanded, value);
        }
        internal static double SharedAdditionalAdjustment { get; set; } = 26;
        // crisp image width and margin
        protected override double AdditionalAdjustment => SharedAdditionalAdjustment;
    }
}
