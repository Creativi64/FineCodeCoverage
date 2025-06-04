using System;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Core.Utilities
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class OrderAttribute : ExportAttribute, IOrderMetadata
    {
        public OrderAttribute(int order, Type contractType)
            : base(contractType) => this.Order = order;

        public OrderAttribute(int order, string contractName)
            : base(contractName) => this.Order = order;

        public OrderAttribute(int order, string contractName, Type contractType)
            : base(contractName, contractType) => this.Order = order;

        public int Order { get; }
    }
}
