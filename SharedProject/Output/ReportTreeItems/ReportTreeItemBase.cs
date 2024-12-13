using System.Collections.ObjectModel;
using Microsoft.VisualStudio.Imaging.Interop;

namespace FineCodeCoverage.Output
{
    public abstract class ReportTreeItemBase : VisualStudioTreeItemBase
    {
        private bool _isExpanded;
        protected readonly ObservableCollection<ReportTreeItemBase> observableChildren = new ObservableCollection<ReportTreeItemBase>();

        public ReportTreeItemBase() => this.Children = this.observableChildren;

        public abstract ImageMoniker ImageMoniker { get; }
        private string _name;
        public string Name
        {
            get => this._name;
            set => this.Set<string>(ref this._name, value, nameof(this.Name));
        }

        private double _coverableLines;
        public double CoverableLines
        {
            get => this._coverableLines;
            set => this.Set<double>(ref this._coverableLines, value, nameof(this.CoverableLines));
        }

        private int cyclomaticComplexity;
        public int CyclomaticComplexity
        {
            get => this.cyclomaticComplexity;
            set => this.Set<int>(ref this.cyclomaticComplexity, value, nameof(this.CyclomaticComplexity));
        }


        private int npathComplexity;
        public int NPathComplexity
        {
            get => this.npathComplexity;
            set => this.Set<int>(ref this.npathComplexity, value, nameof(this.NPathComplexity));
        }
        private decimal crapScore;
        public decimal CrapScore
        {
            get => this.crapScore;
            set => this.Set<decimal>(ref this.crapScore, value, nameof(this.CrapScore));
        }
        private int blocksCovered;
        public int BlocksCovered
        {
            get => this.blocksCovered;
            set => this.Set<int>(ref this.blocksCovered, value, nameof(this.BlocksCovered));
        }
        private int blocksNotCovered;
        public int BlocksNotCovered
        {
            get => this.blocksNotCovered;
            set => this.Set<int>(ref this.blocksNotCovered, value, nameof(this.BlocksNotCovered));
        }

        public override bool IsExpanded
        {
            get => this._isExpanded;
            set => this.Set<bool>(ref this._isExpanded, value, nameof(this.IsExpanded));
        }

        // crisp image width and margin
        protected override double AdditionalAdjustment => 26;
    }
}
