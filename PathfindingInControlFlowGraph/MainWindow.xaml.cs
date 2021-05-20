using PathfindingInControlFlowGraph.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Windows;
namespace PathfindingInControlFlowGraph
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static string workingDirectory = Directory.GetCurrentDirectory();
        string path = Directory.GetParent(workingDirectory).Parent.FullName + @"\Files\";

        static string openedFile;
        private readonly ToastViewModel _notifier;
        public MainWindow()
        {
            InitializeComponent();
            _notifier = new ToastViewModel();
        }
        private void LoadCode(object sender, RoutedEventArgs e)
        {
            string fileContent;

            Microsoft.Win32.OpenFileDialog openDialog = new Microsoft.Win32.OpenFileDialog();
            if (openDialog.ShowDialog() == true)
                openedFile = File.ReadAllText(openDialog.FileName);

            textEditor.Text = openedFile;
            fileContent = openedFile + Environment.NewLine;
            SaveToFile(path + "code.c", fileContent);
            _notifier.ShowSuccess("Plik został załadowany");

            VerifyCode();
        }
        private void SaveCode(object sender, RoutedEventArgs e)
        {
            string fileContent;
            string openedFile = textEditor.Text;

            fileContent = openedFile + Environment.NewLine;
            File.WriteAllText(path + "code.c", fileContent);
            _notifier.ShowSuccess("Kod został zapisany");
        }
        private void VerifyCode()
        {
            Process process = new Process();
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.WorkingDirectory = Directory.GetParent(workingDirectory).Parent.FullName + @"\Files\";
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = $"/C gcc code.c ";
            process.Start();

            string stderrx = process.StandardError.ReadToEnd();

            process.WaitForExit();


            if (stderrx != "")
            {
                _notifier.ShowError("Kod żródłowy nie spełnia wymagań" + stderrx);
            }
            else
            {
                _notifier.ShowSuccess("Kod żródłowy poprawny");
            }
        }
        private void GenerateCFGFiles()
        {
            Process graph = new Process();
            graph.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            graph.StartInfo.WorkingDirectory = Directory.GetParent(workingDirectory).Parent.FullName + @"\Files";
            graph.StartInfo.FileName = "cmd.exe";
            graph.StartInfo.Arguments = $"/C gcc -fdump-tree-all-graph  code.c ";
            graph.Start();
            graph.WaitForExit();

            if (File.Exists(Directory.GetParent(workingDirectory).Parent.FullName + @"\Files\code.c.011t.cfg.dot"))
            {
                _notifier.ShowSuccess("Graf został utworzony");
            }
            else
            {
                _notifier.ShowError("Nie utworzono plików");
            }
        }
        private void GenerateCFGImage(object sender, RoutedEventArgs e)
        {
            GenerateCFGFiles();
            CustomizeDOTFile();

            Process graphviz = new Process();
            graphviz.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            graphviz.StartInfo.WorkingDirectory = Directory.GetParent(workingDirectory).Parent.FullName + @"\Files";
            graphviz.StartInfo.FileName = "cmd.exe";
            graphviz.StartInfo.Arguments = $"/C dot -Tpng code.dot -o graf.png";
            graphviz.Start();
            graphviz.WaitForExit();
        }
        private void CustomizeDOTFile()
        {
            string fileContent;
            string dotFile = LoadFromFile("code.c.011t.cfg.dot");
            string factoredFile = dotFile.Replace("record", "Mcircle");
            fileContent = factoredFile + Environment.NewLine;
            SaveToFile((path + "code.dot"), fileContent);
        }
        private string LoadFromFile(string fileName)
        {
            string fileRead;
            fileRead = File.ReadAllText(path + fileName);
            return fileRead;
        }
        private void SaveToFile(string path, string fileContent)
        {
            File.WriteAllText(path, fileContent);
        }
        public string getBetween(string strSource)
        {
            int Start, End;
            string start = "subgraph \"cluster_";
            string end = "\" {";
            if (strSource.Contains(start) && strSource.Contains(end))
            {
                Start = strSource.IndexOf(start, 0) + start.Length;
                End = strSource.IndexOf(end, Start);
                return strSource.Substring(Start, End - Start);
            }
            else
            {
                return "";
            }
        }
        private string getNode(string line, string start, string end)
        {
            int p1 = line.IndexOf(start) + start.Length;
            int p2 = line.IndexOf(end, p1);

            if (end == "") return (line.Substring(p1));
            else return line.Substring(p1, p2 - p1);
        }
        private List<SingleFunction> RemoveHeaderAndDivideFunction()
        {
            List<SingleFunction> singleFunction = new List<SingleFunction>();
            StringBuilder DotFileSb = new StringBuilder();
            string line;
            int iterator = 0;
            int lineNumber = File.ReadAllLines(Directory.GetParent(workingDirectory).Parent.FullName + @"\Files\" + "code.dot").Length;
            StreamReader file = new StreamReader(Directory.GetParent(workingDirectory).Parent.FullName + @"\Files\" + "code.dot");

            while ((line = file.ReadLine()) != null)
            {
                iterator++;

                if (iterator > 2 && iterator < lineNumber)
                {
                    DotFileSb.Append(line);
                    DotFileSb.Append(Environment.NewLine);
                }
            }
            string DOT_File_Stringified = DotFileSb.ToString();

            StringBuilder singleFunctionSb = new StringBuilder();
            int breaker = 0;

            foreach (char sign in DOT_File_Stringified)
            {
                singleFunctionSb.Append(sign);
                if (sign == '{')
                {
                    breaker++;
                }
                else if (sign == '}')
                {
                    breaker--;

                    if (breaker == 0)
                    {
                        SingleFunction sf = new SingleFunction();
                        sf.functionBody = singleFunctionSb.ToString();
                        sf.functionName = getBetween(singleFunctionSb.ToString());
                        singleFunction.Add(sf);
                        singleFunctionSb.Clear();
                    }
                }
            }
            //GetNodesFromEveryFunction(singleFunction);
            return singleFunction;
        }
        private List<SingleFunction> GetNodesFromEveryFunction(List<SingleFunction> singleFunctions)
        {
            if (singleFunctions.Count != 0)
            {
                for (int i = 0; i < singleFunctions.Count; i++)
                {
                    List<Node> nodes = new List<Node>();
                    string text = singleFunctions[i].functionBody;
                    StringReader strReader = new StringReader(text);
                    string line;

                    while ((line = strReader.ReadLine()) != null)
                    {
                        if (line.Contains("->") == true)
                        {
                            bool addedFlag = false;
                            string nodeNumber = getNode(line, "_basic_block_", ":s");
                            line = line.Replace("_basic_block_" + nodeNumber + ":s", "");
                            string nodesLinked = getNode(line, "_basic_block_", ":n");
                            int nodesLinkedParsedToInt = int.Parse(nodesLinked);

                            if (nodes.Count > 0)
                            {
                                foreach (Node n in nodes)
                                {
                                    if (n.node == int.Parse(nodeNumber))
                                    {
                                        n.nodesLinked.Add(nodesLinkedParsedToInt);
                                        addedFlag = true;
                                        break;
                                    }
                                }
                            }

                            if (addedFlag == false)
                            {
                                Node node = new Node();
                                node.node = int.Parse(nodeNumber);
                                node.nodesLinked = new List<int>();
                                node.nodesLinked.Add(nodesLinkedParsedToInt);
                                nodes.Add(node);
                            }
                        }
                    }
                    singleFunctions[i].nodes = nodes;
                }
            }
            return singleFunctions;
        }
        private void FindPathSet(object sender, RoutedEventArgs e)
        {
            List<SingleFunction> singleFunctions;
            List<int> numberOfNodes = new List<int>();
            List<List<string>> resultList = new List<List<string>>();

            singleFunctions = RemoveHeaderAndDivideFunction(); // usuwa nagłowki
            var singleFunctionsWithNodes = GetNodesFromEveryFunction(singleFunctions); // wydobywa wierzchołki z poszczególnych funcji i ich połączenia
            SaveToFileLinkedNodes(singleFunctionsWithNodes); // zapisuje do pliku wierzchołki z połaczeniami
            var normalPathList = GetPathWithoutCycles(0, 1, singleFunctionsWithNodes);
            var cyclePathList = GetCycleByFunction(singleFunctionsWithNodes);
            var parsedPath = ParsePathToInt(normalPathList);
            var parsedCycle = ParseCycleToInt(cyclePathList);
            var combinedPath = GetCombinedPaths(parsedCycle, parsedPath); // zwraca połączone sciezki

            var linearIndependentPathSet = SeekForLinearlyIndependentPath(combinedPath, singleFunctionsWithNodes);
            SaveCombinedPath(linearIndependentPathSet);
            _notifier.ShowSuccess("Zbiór dróg liniowo niezależnych został obliczony");
        }

        private List<List<string>> GetCycleByFunction(List<SingleFunction> singleFunctions)
        {
            List<List<string>> functionCyclesPath = new List<List<string>>();

            foreach (var function in singleFunctions)
            {
                functionCyclesPath.Add(GetCycle(function.nodes));
            }
            return functionCyclesPath;

        }
        private List<string> GetCycle(List<Node> nodeList)
        {
            List<NodeCycle> tempNode = new List<NodeCycle>();
            List<Node> lista = nodeList;

            for (int i = 0; i < lista.Count; i++)
            {
                NodeCycle nodzik = new NodeCycle(lista[i].node.ToString());
                tempNode.Add(nodzik);
            }
            int gf;
            for (int i = 0; i < tempNode.Count; i++)
            {
                gf = i;
                string thisName = tempNode[i].Id;
                var tn = lista.Find(a => a.node.ToString() == thisName);

                if (tn.nodesLinked.Count > 0)
                {
                    for (int x = 0; x < tn.nodesLinked.Count; x++)
                    {
                        if (tn.node == 0)
                        {
                            if (tn.nodesLinked[x] == 1)
                            {

                            }
                            else
                            {
                                string connectWith = tn.nodesLinked[x].ToString();

                                var z = tempNode.Find(a => a.Id == connectWith);
                                tempNode[i].Targets.Add(z);
                            }
                        }
                        else if (gf + 1 == tempNode.Count)
                        {
                            string connectWith = tn.nodesLinked[x].ToString();

                            var z = new NodeCycle("1");
                            tempNode[i].Targets.Add(z);
                        }
                        else
                        {
                            string connectWith = tn.nodesLinked[x].ToString();

                            var z = tempNode.Find(a => a.Id == connectWith);
                            tempNode[i].Targets.Add(z);
                        }
                    }
                }
            }
            StringBuilder cycles = new StringBuilder();
            List<string> cycleList = new List<string>();
            foreach (var cycle in CyclePath.FindSimpleCycles(tempNode[0]))
            {
                Console.WriteLine(string.Join(",", cycle.Select(n => n.Id)));
                foreach (var node in cycle)
                {
                    cycles.Append(node.Id + " ");
                }
                var cyclesStringified = cycles.ToString();
                cycleList.Add(cyclesStringified.Trim());
                cycles.Clear();
            }
            return cycleList;
        }
        private void SaveCombinedPath(List<List<List<int>>> combinedNodes)
        {
            StringBuilder functionNode = new StringBuilder();
            StringBuilder path = new StringBuilder();
            StringBuilder nodes = new StringBuilder();

            foreach (var line in combinedNodes)
            {
                foreach (var node in line)
                {
                    foreach (var linked in node)
                    {
                        nodes.Append(" < " + linked + " > ");
                    }
                    path.Append("Scieżka : " + nodes);
                    path.Append(Environment.NewLine);
                    nodes.Clear();
                }
                functionNode.Append("1 Funkcja zawiera następujące sciezki : " + Environment.NewLine + path);
                functionNode.Append(Environment.NewLine);
                path.Clear();
            }
            textEditor.Text = functionNode.ToString();

            var pathSet = functionNode + Environment.NewLine;
            SaveToFile(path + "paths_set.txt", pathSet);
        }
        private List<List<List<int>>> ParsePathToInt(List<List<string>> normalPathList)
        {
            List<List<List<int>>> parsedFunctionPath = new List<List<List<int>>>();
            List<List<int>> parsedPath = new List<List<int>>();
            List<int> parsedNode = new List<int>();

            foreach (var function in normalPathList)
            {
                foreach (var normalPath in function)
                {
                    if (normalPath != "0 1")
                    {
                        parsedNode = new List<int>(Array.ConvertAll(normalPath.Split(' '), Convert.ToInt32));
                        parsedPath.Add(parsedNode);
                    }
                }
                parsedFunctionPath.Add(parsedPath);
                parsedPath = new List<List<int>>();
            }

            return parsedFunctionPath;
        }
        private List<List<List<int>>> ParseCycleToInt(List<List<string>> cyclePathList)
        {
            List<List<List<int>>> parsedFunctionCycle = new List<List<List<int>>>();
            List<List<int>> parsedCycle = new List<List<int>>();
            List<int> parsedCycleNode = new List<int>();

            foreach (var function in cyclePathList)
            {
                foreach (var cyclePath in function)
                {
                    parsedCycleNode = new List<int>(Array.ConvertAll(cyclePath.Split(' '), Convert.ToInt32));
                    parsedCycle.Add(parsedCycleNode);
                }
                parsedFunctionCycle.Add(parsedCycle);
                parsedCycle = new List<List<int>>();
            }

            return parsedFunctionCycle;
        }
        private List<List<List<int>>> GetCombinedPaths(List<List<List<int>>> cyclePathList, List<List<List<int>>> normalPathList)
        {
            List<List<List<int>>> combinedPath = new List<List<List<int>>>();

            for (int i = 0; i < cyclePathList.Count(); i++)
            {
                var result = GetCombinedPathsFromFunction(cyclePathList[i], normalPathList[i]);

                foreach (var normalPath in normalPathList[i])
                {
                    result.Add(normalPath);
                }
                combinedPath.Add(result);
            }
            return combinedPath;
        }
        private List<List<int>> GetCombinedPathsFromFunction(List<List<int>> cyclePathList, List<List<int>> normalPathList)
        {
            List<List<int>> combinedPath = new List<List<int>>(); // lista sciezek
            List<int> pathTemp = new List<int>(); // sciezka
            List<int> cycleTemp = new List<int>(); // sciezka
            List<int> pathHolder = new List<int>(); // sciezka

            foreach (var cycle in cyclePathList)
            {
                foreach (var normalPath in normalPathList)
                {
                    if (normalPath.Contains(cycle.Last()))
                    {
                        foreach (var item in normalPath) // usuwanie elementów
                        {
                            pathHolder.Add(item);
                        }
                        foreach (int node in normalPath)
                        {
                            if (cycle.Last() == node)
                            {
                                pathTemp.Add(node);
                                foreach (int cycleNode in cycle)
                                {
                                    cycleTemp.Add(cycleNode); // dodaje elementy cyklu do cycleTemp
                                }
                                break;
                            }
                            else
                            {
                                pathTemp.Add(node); // dodaje elementy do sciezki tymczasowej pathTemp do momentu wystapienia poczatku cyklu
                            }
                        }
                        foreach (var removedItem in pathTemp.ToList()) // usuwanie elementów
                        {
                            pathHolder.Remove(removedItem);
                        }
                        pathTemp = new List<int>();
                        var cycleHolder = cycleTemp;
                        cycleTemp = new List<int>();
                        foreach (var item in pathHolder) // usuwanie elementów
                        {
                            cycleHolder.Add(item);
                        }
                        pathHolder = new List<int>();
                        combinedPath.Add(cycleHolder);
                        cycleHolder = new List<int>();
                    }
                }
            }
            return combinedPath;
        }
        private List<List<string>> GetPathWithoutCycles(int source, int destiny, List<SingleFunction> cleanedNodesWithLinkedNodes)
        {
            int iterator = 0;
            Graph g = new Graph(14);
            List<string> result = new List<string>();
            StringBuilder paths = new StringBuilder();
            List<string> normalPaths = new List<string>();
            List<List<string>> functionNormalPath = new List<List<string>>();

            foreach (var function in cleanedNodesWithLinkedNodes)
            {
                g = new Graph(function.nodes.Count + 1);
                foreach (var entryNode in function.nodes)
                {
                    foreach (var linkedNode in entryNode.nodesLinked)
                    {
                        g.addEdge(entryNode.node, linkedNode);
                    }
                }
                result = g.printAllPaths(source, destiny);

                //functionNormalPath.Add(result);
                result.Add("#");
                iterator++;
            }

            foreach (var onePath in result)
            {
                paths.Append(onePath + Environment.NewLine);
                if (onePath.Contains("#"))
                {
                    functionNormalPath.Add(normalPaths);
                    normalPaths = new List<string>();
                }
                else
                {
                    normalPaths.Add(onePath);

                }

            }

            var pathWithoutCyclesSet = paths + Environment.NewLine;
            SaveToFile(path + "paths_without_cycles_set.txt", pathWithoutCyclesSet);

            return functionNormalPath;
        }
        private List<List<List<int>>> SeekForLinearlyIndependentPath(List<List<List<int>>> allPathSet, List<SingleFunction> nodesWithConnection)
        {
            int lenght = 0;
            List<int> basePath = new List<int>();
            List<int> basePathHolder = new List<int>();
            List<int> part = new List<int>();
            List<int> basePathHolderPart = new List<int>();
            List<Node> decisionNodes = new List<Node>();
            List<int> decisionNodesWithCycle = new List<int>();
            List<List<List<int>>> linearlyIndependentPath = new List<List<List<int>>>();
            List<List<int>> perFunctionLinearlyIndependentPath = new List<List<int>>();
            List<List<int>> perFunctionLinearlyIndependentPathCopy = new List<List<int>>();
            List<NodeCovered> result = new List<NodeCovered>();
            int county = 0;
            int perFunctionCounter = 0;
            Boolean flag = false;
            Boolean basePathFlag = false;
            // znalezienie najdłuższej scieżki
            foreach (var function in allPathSet)
            {
                while (flag == false)
                {
                    // zamień sciezke bazową na sciezke bazową która zawiera nie pokryty wierzchołek
                    if (basePathFlag == true)
                    {
                        basePath = takePathWhichCoverTheMostUncoveredNodes(allPathSet[county], result);
                    }
                    else
                    {
                        GetLongestPath(ref lenght, ref basePath, function);
                    }

                    basePathHolder = new List<int>();
                    foreach (var copy in basePath)
                    {
                        basePathHolder.Add(copy);
                    }
                    decisionNodes = GetDecisionNodes(nodesWithConnection, county);
                    decisionNodesWithCycle = GetDecisionNodesWithCycles(basePath, decisionNodes);

                    perFunctionLinearlyIndependentPath = new List<List<int>>();
                    // iteruj po kazdym elemencie sciezki bazowej i sprawdzaj czy jest wierzchołkiem decyzyjnym
                    for (int i = basePath.Count; i > 0; i--)
                    {
                        // sprawdz czy znaleziono decyzje
                        foreach (var decisionNode in decisionNodes.ToList())
                        {
                            if (basePath[i - 1] == decisionNode.node && decisionNodesWithCycle.Contains(basePath[i - 1]) == false) // sprawdz czy decyzja nie jest cyklem
                            {

                                foreach (var linked in decisionNode.nodesLinked.ToList()) // usuwa obecny nastepnik decyzji
                                {
                                    if (basePath[i] == linked)
                                    {
                                        decisionNode.nodesLinked.Remove(linked);
                                    }
                                }

                                basePathHolderPart = new List<int>();
                                basePathHolder[i] = decisionNode.nodesLinked[0];
                                var counter = i;
                                for (int j = 0; j <= counter; j++)
                                {
                                    basePathHolderPart.Add(basePathHolder[j]);
                                }
                                var elementCounter = 0;

                                foreach (var singlePath in allPathSet[county])
                                {
                                    part = new List<int>();
                                    for (int g = 0; g < basePathHolderPart.Count; g++)
                                    {

                                        part.Add(singlePath[g]);
                                    }

                                    foreach (var elementToCheck in basePathHolderPart)
                                    {
                                        if (part.Contains(elementToCheck))
                                        {
                                            elementCounter++;
                                        }
                                        if (elementCounter == basePathHolderPart.Count)
                                        {
                                            break;
                                        }
                                    }
                                    if (elementCounter == basePathHolderPart.Count)
                                    {
                                        if (perFunctionLinearlyIndependentPath.Contains(singlePath) == false)
                                        {
                                            perFunctionLinearlyIndependentPath.Add(singlePath);
                                        }

                                        break;
                                    }
                                    else
                                    {

                                    }
                                    elementCounter = 0;
                                }

                            }
                            else if (basePath[i - 1] == decisionNode.node && decisionNodesWithCycle.Contains(basePath[i - 1]) == true)
                            {
                                decisionNodesWithCycle.Remove(basePath[i - 1]);
                            }
                        }
                    }

                    perFunctionLinearlyIndependentPath.Add(basePath);
                    foreach (var elem in perFunctionLinearlyIndependentPath)
                    {
                        perFunctionLinearlyIndependentPathCopy.Add(elem);
                    }

                    linearlyIndependentPath.Add(perFunctionLinearlyIndependentPathCopy);
                    perFunctionLinearlyIndependentPathCopy = new List<List<int>>();
                    result = checkIfAllNodesAreCovered(linearlyIndependentPath[perFunctionCounter], nodesWithConnection[county]);

                    perFunctionCounter++;
                    if (result.Count == 0)
                    {
                        basePathFlag = false;
                        flag = true;
                    }
                    else
                    {
                        basePathFlag = true;
                    }
                }
                perFunctionCounter = 0;
                county++;
                flag = false;
                //basePathFlag = false;
            }
            return linearlyIndependentPath;
        }


        private static List<int> GetDecisionNodesWithCycles(List<int> basePath, List<Node> decisionNodes)
        {
            List<int> decisionNodesWithCycle = new List<int>();
            foreach (var decision in decisionNodes)
            {
                var counter = 0;
                foreach (var baseNode in basePath)
                {
                    if (decision.node == baseNode)
                    {
                        counter++;
                    }
                    if (counter == 2)
                    {
                        decisionNodesWithCycle.Add(decision.node);
                        counter = 0;
                    }
                }
            }

            return decisionNodesWithCycle;
        }

        private static List<Node> GetDecisionNodes(List<SingleFunction> nodesWithConnection, int county)
        {
            List<Node> decisionNodes = new List<Node>();
            foreach (var node in nodesWithConnection[county].nodes)
            {
                if (node.nodesLinked.Count > 1 && node.node != 0)
                {
                    decisionNodes.Add(node);
                }
            }

            return decisionNodes;
        }

        private static void GetLongestPath(ref int lenght, ref List<int> basePath, List<List<int>> function)
        {
            foreach (var path in function)
            {
                int count = path.Count;
                if (count > lenght)
                {
                    lenght = count;
                    basePath = path;
                }
            }
        }

        private List<int> GetDecisionNode(List<int> nodesWithEdges)
        {

            return nodesWithEdges;
        }
        private List<int> GetDecisionNodeWithCycle(List<int> nodesWithEdges)
        {

            return nodesWithEdges;
        }
        private List<int> takePathWhichCoverTheMostUncoveredNodes(List<List<int>> AllPaths, List<NodeCovered> uncoveredNode)
        {
            List<int> newBasePath = new List<int>();
            List<int> newBasePathHeuristic = new List<int>();
            List<PathCoverage> pathCoverage = new List<PathCoverage>();

            int counter = 0;

            foreach (var path in AllPaths)
            {
                foreach (var uncovered in uncoveredNode)
                {
                    if (path.Contains(uncovered.node))
                    {
                        counter++;
                    }
                }
                pathCoverage.Add(new PathCoverage(path, counter));
                counter = 0;
            }
            pathCoverage.Sort((x, y) => x.uncoveredNode.CompareTo(y.uncoveredNode));
            pathCoverage.Reverse();
            return pathCoverage[0].path; // zwraca sciezke najbardziej obiecujacą
        }
        private List<NodeCovered> checkIfAllNodesAreCovered(List<List<int>> linearlyIndependentPath, SingleFunction singleFunction)
        {
            List<NodeCovered> CoveredNode = new List<NodeCovered>();


            foreach (var single in singleFunction.nodes)
            {
                CoveredNode.Add(new NodeCovered(single.node, false));
            }

            foreach (var node in CoveredNode)
            {
                foreach (var path in linearlyIndependentPath)
                {
                    foreach (var linearNode in path)
                    {
                        if (linearNode == node.node)
                        {
                            node.isCovered = true;
                        }
                        else
                        {
                            //CoveredNode.Add(linearNode);
                        }
                    }
                }
            }
            return CoveredNode.Where(node => node.isCovered = false).ToList();
        }
        private void SaveToFileLinkedNodes(List<SingleFunction> cleanedNodesWithLinkedNodes)
        {
            StringBuilder functionNode = new StringBuilder();
            StringBuilder oneNode = new StringBuilder();
            StringBuilder linkedNodes = new StringBuilder();

            foreach (var line in cleanedNodesWithLinkedNodes)
            {
                foreach (var node in line.nodes)
                {
                    foreach (var linked in node.nodesLinked)
                    {
                        linkedNodes.Append(" < " + linked + " > ");
                    }
                    oneNode.Append("Wierzchołek | " + node.node + " | Jest połaczony z wierzchołkami : " + linkedNodes);
                    oneNode.Append(Environment.NewLine);
                    linkedNodes.Clear();
                }
                functionNode.Append("Funkcja | " + line.functionName + " | zawiera następujące wierzchołki : " + Environment.NewLine + oneNode);
                functionNode.Append(Environment.NewLine);
                oneNode.Clear();
            }
            textEditor.Text = functionNode.ToString();
            var nodesContent = functionNode + Environment.NewLine;
            SaveToFile(path + "nodes_with_edges_set.txt", nodesContent);
        }
        private void ShowNodesWithEdges(object sender, RoutedEventArgs e)
        {
            textEditor.Text = LoadFromFile("nodes_with_edges_set.txt");
        }
        private void Reset(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
            System.Windows.Forms.Application.Restart();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ImageViewer iv = new ImageViewer();
            iv.Show();

        }

    }

}
