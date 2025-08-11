using System;

namespace FineCodeCoverage.Vs.Theming.VsColorThemeService
{
    public class ColorName
    {
        public ColorName(Guid category, string name)
        {
            Category = category;
            Name = name;
        }

        public Guid Category { get; }

        public string Name { get; }
    }
}
