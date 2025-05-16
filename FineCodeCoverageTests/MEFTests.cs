using FineCodeCoverage.Output;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;

namespace FineCodeCoverageTests
{
    [Export]
    public class Cycle
    {
        [ImportingConstructor]
        public Cycle(Cycle2 cycle) { }
    }
    [Export]
    public class Cycle2
    {
        [ImportingConstructor]
        public Cycle2(Cycle cyle) { }
    }

    // Corrected from ChatGPT - but more to check
    internal static class MEFCycleChecker { 
    
        public static bool Check(Type assemblyType,Action<string> cycleLogger = null)
        {
            var hasCycle = false;
            var graph = BuildDependencyGraph(GetMEFExportedTypes(assemblyType));

            foreach (var node in graph.Keys)
            {
                var visited = new HashSet<Type>();
                var stack = new Stack<Type>();
                if (HasCycle(node, graph, visited, stack, out var cycle))
                {
                    hasCycle = true;
                    cycleLogger?.Invoke($"Cycle detected: {string.Join(" -> ", cycle.Select(t => t.Name))}");
                }
            }
            return hasCycle;
        }

        private static Dictionary<Type, List<Type>> BuildDependencyGraph(List<Type> exportedTypes)
        {
            var graph = new Dictionary<Type, List<Type>>();

            foreach (var type in exportedTypes)
            {
                var dependencies = new List<Type>();

                var constructor = type.GetConstructors()
                    .FirstOrDefault(c => c.IsDefined(typeof(ImportingConstructorAttribute), true));

                if (constructor != null)
                {
                    foreach (var param in constructor.GetParameters())
                    {
                        dependencies.Add(ExtractImportType(param.ParameterType));
                    }
                }

                // Properties
                foreach (var prop in type.GetProperties())
                {
                    if (IsImport(prop))
                        dependencies.Add(ExtractImportType(prop.PropertyType));
                }

                graph[type] = dependencies
                    .Where(d => exportedTypes.Contains(d)) // only track dependencies between exports
                    .Distinct()
                    .ToList();
            }

            return graph;
        }

        private static Type ExtractImportType(Type type)
        {
            if (type.IsArray)
                return type.GetElementType();

            if (type.IsGenericType)
            {
                var genericDef = type.GetGenericTypeDefinition();

                if (genericDef == typeof(IEnumerable<>) ||
                    genericDef == typeof(Lazy<>) ||
                    genericDef == typeof(Lazy<,>))
                {
                    return type.GetGenericArguments()[0];
                }
            }

            return type;
        }

        private static bool IsImport(ICustomAttributeProvider member)
        {
            return member.IsDefined(typeof(ImportAttribute), true) ||
                   member.IsDefined(typeof(ImportManyAttribute), true);
        }

        private static bool HasCycle(Type node, Dictionary<Type, List<Type>> graph, HashSet<Type> visited, Stack<Type> stack, out List<Type> cycle)
        {
            if (stack.Contains(node))
            {
                cycle = stack.Reverse().SkipWhile(t => t != node).ToList();
                cycle.Add(node); // close the loop
                return true;
            }

            if (visited.Contains(node))
            {
                cycle = null;
                return false;
            }

            visited.Add(node);
            stack.Push(node);

            if (graph.TryGetValue(node, out var neighbors))
            {
                foreach (var neighbor in neighbors)
                {
                    if (HasCycle(neighbor, graph, visited, stack, out cycle))
                        return true;
                }
            }

            stack.Pop();
            cycle = null;
            return false;
        }

        private static List<Type> GetMEFExportedTypes(Type assemblyType)
        {
            return assemblyType.Assembly.GetTypes()
            .Where(t => t.GetCustomAttributes(typeof(ExportAttribute), true).Any())
            .ToList();
        }

    }


    internal class MEFTests
    {
        [Test]
        public void Should_Have_MEF_Cycle()
        {
            Assert.True(MEFCycleChecker.Check(typeof(Cycle)));
        }

        [Test]
        public void Should_Have_No_MEF_Cycle()
        {
            Assert.False(MEFCycleChecker.Check(typeof(FCCPackage)));
        }
    }
}
