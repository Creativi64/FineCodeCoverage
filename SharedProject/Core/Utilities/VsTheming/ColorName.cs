using System;

namespace FineCodeCoverage.Core.Utilities
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
