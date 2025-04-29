using System;
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

        private double _coverableLines;
        public double CoverableLines
        {
            get => this._coverableLines;
            set => this.Set(ref this._coverableLines, value);
        }

        private double _coveredLines;
        public double CoveredLines
        {
            get => this._coveredLines;
            set => this.Set(ref this._coveredLines, value);
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
        private int blocksCovered;
        public int BlocksCovered
        {
            get => this.blocksCovered;
            set => this.Set(ref this.blocksCovered, value);
        }
        private int blocksNotCovered;
        public int BlocksNotCovered
        {
            get => this.blocksNotCovered;
            set => this.Set(ref this.blocksNotCovered, value);
        }

        public override bool IsExpanded
        {
            get => this._isExpanded;
            set => this.Set(ref this._isExpanded, value);
        }

        // crisp image width and margin
        protected override double AdditionalAdjustment => 26;
    }
}
