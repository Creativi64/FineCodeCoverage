using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Core.Utilities.VsThreading;
using FineCodeCoverage.Editor.DynamicCoverage;

namespace FineCodeCoverage.Editor.Management
{
    [Export(typeof(ICoverageColoursProvider))]
    [Export(typeof(IFontAndColorsInfosProvider))]
    internal class FontAndColorsInfosProvider : ICoverageColoursProvider, IFontAndColorsInfosProvider
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IFontsAndColorsHelper _fontsAndColorsHelper;
        private readonly IThreadHelper _threadHelper;
        private CoverageColours _lastCoverageColours;
        private ICoverageFontAndColorsCategoryItemNames _coverageFontAndColorsCategoryItemNames;

        public ICoverageFontAndColorsCategoryItemNames CoverageFontAndColorsCategoryItemNames
        {
            set => this._coverageFontAndColorsCategoryItemNames = value;
        }

        private readonly struct NameIndex
        {
            public NameIndex(string name, int index)
            {
                this.Name = name;
                this.Index = index;
            }
            public string Name { get; }
            public int Index { get; }
        }

        private class CategoryNameIndices
        {
            public CategoryNameIndices(Guid category) => this.Category = category;
            public Guid Category { get; }
            public List<NameIndex> NameIndices { get; } = new List<NameIndex>();
        }

        [ImportingConstructor]
        public FontAndColorsInfosProvider(
            IEventAggregator eventAggregator,
            IFontsAndColorsHelper fontsAndColorsHelper,
            IThreadHelper threadHelper
        )
        {
            this._eventAggregator = eventAggregator;
            this._fontsAndColorsHelper = fontsAndColorsHelper;
            this._threadHelper = threadHelper;
        }

        private List<(FontAndColorsCategoryItemName, int)> IndexedFontAndColorsCategoryItemNames()
            => new List<(FontAndColorsCategoryItemName, int)>
                {
                    (this._coverageFontAndColorsCategoryItemNames.Covered, 0),
                    (this._coverageFontAndColorsCategoryItemNames.NotCovered, 1),
                    (this._coverageFontAndColorsCategoryItemNames.PartiallyCovered, 2),
                    (this._coverageFontAndColorsCategoryItemNames.Dirty,3),
                    (this._coverageFontAndColorsCategoryItemNames.NewLines,4),
                    (this._coverageFontAndColorsCategoryItemNames.NotIncluded,5)
                };

        private List<CategoryNameIndices> GetCategoryNameIndices()
        {
            var lookup = new Dictionary<Guid, CategoryNameIndices>();

            List<(FontAndColorsCategoryItemName, int)> indexedFontAndColorsCategoryItemNames = this.IndexedFontAndColorsCategoryItemNames();

            foreach ((FontAndColorsCategoryItemName fontAndColorsCategoryItemName, int index) in indexedFontAndColorsCategoryItemNames)
            {
                if (!lookup.TryGetValue(fontAndColorsCategoryItemName.Category, out CategoryNameIndices categoryNameIndices))
                {
                    categoryNameIndices = new CategoryNameIndices(fontAndColorsCategoryItemName.Category);
                    lookup.Add(fontAndColorsCategoryItemName.Category, categoryNameIndices);
                }

                categoryNameIndices.NameIndices.Add(new NameIndex(fontAndColorsCategoryItemName.ItemName, index));
            }

            return lookup.Values.ToList();
        }

        public ICoverageColours GetCoverageColours() => this.GetCoverageColoursIfRequired();

        private CoverageColours GetCoverageColoursIfRequired()
            => this._lastCoverageColours ?? (this._lastCoverageColours = this.GetCoverageColoursFromFontsAndColors());

        private CoverageColours GetCoverageColoursFromFontsAndColors()
        {
            List<IFontAndColorsInfo> fromFontsAndColors = this.GetItemCoverageInfosFromFontsAndColors();
            return new CoverageColours(
                fromFontsAndColors[0],//touched
                fromFontsAndColors[1],//not touched
                fromFontsAndColors[2],//partial
                fromFontsAndColors[3],//dirty
                fromFontsAndColors[4],//newlines
                fromFontsAndColors[5]//not included
            );
        }

        private List<IFontAndColorsInfo> GetItemCoverageInfosFromFontsAndColors()
            => this._threadHelper.JoinableTaskFactory.Run(() => this.GetItemCoverageInfosFromFontsAndColorsAsync());

        private async Task<List<IFontAndColorsInfo>> GetItemCoverageInfosFromFontsAndColorsAsync()
        {
            List<(IFontAndColorsInfo fontAndColorsInfo, int nameIndex)>[] results = await this.GetItemCoverageInfosWithNameIndexFromFontsAndColorsAsync();
            return results.SelectMany(r => r).OrderBy(r => r.nameIndex).Select(r => r.fontAndColorsInfo).ToList();
        }

        private Task<List<(IFontAndColorsInfo fontAndColorsInfo, int nameIndex)>[]> GetItemCoverageInfosWithNameIndexFromFontsAndColorsAsync()
        {
            List<CategoryNameIndices> allCategoryNameIndices = this.GetCategoryNameIndices();
            return Task.WhenAll(
                allCategoryNameIndices.Select(categoryNameIndices => this.GetAsync(categoryNameIndices))
            );
        }

        private async Task<List<(IFontAndColorsInfo, int)>> GetAsync(CategoryNameIndices categoryNameIndices)
        {
            List<IFontAndColorsInfo> fontAndColorsInfos = await this._fontsAndColorsHelper.GetInfosAsync(
                    categoryNameIndices.Category,
                    categoryNameIndices.NameIndices.Select(ni => ni.Name).ToArray());
            return fontAndColorsInfos.Select((fontAndColorsInfo, i) => (fontAndColorsInfo, categoryNameIndices.NameIndices[i].Index)).ToList();
        }

        public Dictionary<DynamicCoverageType, IFontAndColorsInfo> GetChangedFontAndColorsInfos()
        {
            CoverageColours currentColors = this.GetCoverageColoursFromFontsAndColors();
            Dictionary<DynamicCoverageType, IFontAndColorsInfo> changes = currentColors.GetChanges(this._lastCoverageColours);
            this._lastCoverageColours = currentColors;
            if (changes.Any())
            {
                this._eventAggregator.SendMessage(new CoverageColoursChangedMessage());
            }

            return changes;
        }

        public Dictionary<DynamicCoverageType, IFontAndColorsInfo> GetFontAndColorsInfos()
            => this.GetCoverageColoursIfRequired().GetChanges(null);
    }
}