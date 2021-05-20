using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathfindingInControlFlowGraph.Models
{
    class NodeCovered
    {
        public NodeCovered(int node, bool isCovered)
        {
            this.node = node;
            this.isCovered = isCovered;
        }

        public int node { get; set; }
        public Boolean isCovered { get; set; }
    }
}
