using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PathfindingInControlFlowGraph.Models
{
    public static class CyclePath
    {
        public static IEnumerable<NodeCycle[]> FindSimpleCycles(NodeCycle startNode)
        {
            return FindSimpleCyclesCore(new Stack<NodeCycle>(new[] { startNode }));
        }

        private static IEnumerable<NodeCycle[]> FindSimpleCyclesCore(Stack<NodeCycle> pathSoFar)
        {
            var nodeJustAdded = pathSoFar.Peek();
            foreach (var target in nodeJustAdded.Targets)
            {
                if (pathSoFar.Contains(target))
                {
                    yield return pathSoFar.Reverse().Concat(new[] { target }).ToArray();
                }
                else
                {
                    pathSoFar.Push(target);
                    foreach (var simpleCycle in FindSimpleCyclesCore(pathSoFar))
                    {
                        yield return simpleCycle;
                    }
                    pathSoFar.Pop();
                }
            }
        }
    }

    public class NodeCycle
    {
        public string Id { get;  set; }
        public readonly List<NodeCycle> Targets = new List<NodeCycle>();

        public NodeCycle(string id)
        {
            this.Id = id;
        }
    }
}