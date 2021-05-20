using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PathfindingInControlFlowGraph.Models
{
    public class Graph
    {
        public string txtPath = @"H:\Studia Magisterskie\NO\PROJEKT\CFG_generator\CFG_generator\Files\" + "flows.txt";
        // No. of vertices in graph  
        private int v;
        static List<string> singleFlowList = new List<string>();
        // adjacency list  
        private List<int>[] adjList;

        // Constructor  
        public Graph(int vertices)
        {

            // initialise vertex count  
            this.v = vertices;

            // initialise adjacency list  
            initAdjList();
        }

        // utility method to initialise  
        // adjacency list  
        private void initAdjList()
        {
            adjList = new List<int>[v];

            for (int i = 0; i < v; i++)
            {
                adjList[i] = new List<int>();
            }
        }

        // add edge from u to v  
        public void addEdge(int u, int v)
        {
            // Add v to u's list.  
            adjList[u].Add(v);
        }

        // Prints all paths from  
        // 's' to 'd'  
        public List<string> printAllPaths(int s, int d)
        {
            bool[] isVisited = new bool[v];
            List<int> pathList = new List<int>();

            // add source to path[]  
            pathList.Add(s);

            // Call recursive utility  
            printAllPathsUtil(s, d, isVisited, pathList);
            return singleFlowList;
        }

        // A recursive function to print  
        // all paths from 'u' to 'd'.  
        // isVisited[] keeps track of  
        // vertices in current path.  
        // localPathList<> stores actual  
        // vertices in the current path  
        private void printAllPathsUtil(int u, int d,
                                        bool[] isVisited,
                                List<int> localPathList)
        {
            // Mark the current node  
            isVisited[u] = true;

            if (u.Equals(d))
            {
                
                Console.WriteLine(string.Join(" ", localPathList));
                singleFlowList.Add(string.Join(" ", localPathList));
                // if match found then no need  
                // to traverse more till depth  
                isVisited[u] = false;
                return;
            }

            // Recur for all the vertices  
            // adjacent to current vertex  
            foreach (int i in adjList[u])
            {
                if (isVisited[i] == false) // jezeli nie visited i taka sciezka istnieje
                {
                    // store current node  
                    // in path[]  
                    localPathList.Add(i);
                    printAllPathsUtil(i, d, isVisited,
                                        localPathList);
                    // remove current node  
                    // in path[]  
                    localPathList.Remove(i);
                }
            }
            // Mark the current node  
            isVisited[u] = false;
        }

        private bool isExist(List<int> list1, List<string> list2)
        {
            List<string> list1Converted = new List<string>();
            string combinedString = String.Join(" ", list1);
            list1Converted.Add(combinedString);
            {
                if (list2.Contains(combinedString))
                {
                    return true;
                } else
                {
                    return false;
                }
            }
        }
    }
}