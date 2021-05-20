using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathfindingInControlFlowGraph.Models
{
    class PathCoverage
    {
        public PathCoverage(List<int> path, int uncoveredNode)
        {
            this.path = path;
            this.uncoveredNode = uncoveredNode;
        }

        public List<int> path { get; set; }
        public int uncoveredNode { get; set; }
    }
}
