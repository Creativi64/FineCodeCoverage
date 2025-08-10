using System.Collections.Generic;
using ReflectObject;

namespace FineCodeCoverage.Collection.TestExplorer.InternalTypes
{
    public class TestConfiguration : ReflectObjectProperties
    {
        public TestConfiguration(object toReflect)
            : base(toReflect)
        {
        }

        public object UserRunSettings { get; protected set; }

        public IEnumerable<Container> Containers { get; protected set; }

        public string SolutionDirectory { get; protected set; }
    }
}
