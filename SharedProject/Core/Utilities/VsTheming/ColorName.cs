using System;

namespace FineCodeCoverage.Core.Utilities
{
    public class ColorName
    {
        public ColorName(Guid category, string name)
        {
            this.Category = category;
            this.Name = name;
        }
        public Guid Category { get; }

        public string Name { get; }
    }
}