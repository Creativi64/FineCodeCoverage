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

        protected ReportTreeItemBase() => Children = ObservableChildren;

        public IReadOnlyList<ReportTreeItemBase> ReportChildren => ObservableChildren;

        public abstract ImageMoniker ImageMoniker { get; }

        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }

        public int CoverableLines
        {
            get => _coverableLines;
            set => Set(ref _coverableLines, value);
        }

        public int CoveredLines
        {
            get => _coveredLines;
            set => Set(ref _coveredLines, value);
        }

        public int NotCoveredLines
        {
            get => _notCoveredLines;
            set => Set(ref _notCoveredLines, value);
        }

        public int PartialLines
        {
            get => _partialLines;
            set => Set(ref _partialLines, value);
        }

        public int CyclomaticComplexity
        {
            get => _cyclomaticComplexity;
            set => Set(ref _cyclomaticComplexity, value);
        }

        public int NPathComplexity
        {
            get => _npathComplexity;
            set => Set(ref _npathComplexity, value);
        }

        public decimal CrapScore
        {
            get => _crapScore;
            set => Set(ref _crapScore, value);
        }

        public int TotalBranches
        {
            get => _totalBranches;
            set => Set(ref _totalBranches, value);
        }

        public int CoveredBranches
        {
            get => _coveredBranches;
            set => Set(ref _coveredBranches, value);
        }

        public override bool IsExpanded
        {
            get => _isExpanded;
            set => Set(ref _isExpanded, value);
        }

        public int NotCoveredBranches => TotalBranches - CoveredBranches;

        internal static double SharedAdditionalAdjustment { get; set; } = 26;

        // crisp image width and margin
        protected override double AdditionalAdjustment => SharedAdditionalAdjustment;
    }
}
