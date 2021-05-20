using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathfindingInControlFlowGraph.Models
{
    class Node
    {
        public int node { get; set; }
        public List<int> nodesLinked { get; set; }
    }
}
