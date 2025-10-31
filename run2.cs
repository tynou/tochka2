using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    private class Graph
    {
        public readonly Dictionary<char, HashSet<char>> AdjacencyList;
        public readonly HashSet<char> Gates;

        public Graph(List<(string, string)> edges)
        {
            AdjacencyList = new Dictionary<char, HashSet<char>>();
            Gates = [];

            var allNodes = new HashSet<char>();
            foreach (var edge in edges)
            {
                allNodes.Add(edge.Item1[0]);
                allNodes.Add(edge.Item2[0]);
            }

            foreach (var node in allNodes)
            {
                AdjacencyList[node] = [];
                if (char.IsUpper(node))
                    Gates.Add(node);
            }

            foreach (var edge in edges)
            {
                var u = edge.Item1[0];
                var v = edge.Item2[0];
                AdjacencyList[u].Add(v);
                AdjacencyList[v].Add(u);
            }
        }

        public void RemoveEdge(char u, char v)
        {
            if (AdjacencyList.TryGetValue(u, out var value1))
                value1.Remove(v);

            if (AdjacencyList.TryGetValue(v, out var value2))
                value2.Remove(u);
        }

        public void MakeEdge(char u, char v)
        {
            AdjacencyList[u].Add(v);
            AdjacencyList[v].Add(u);
        }
    }

    private static string? GetShortestPath(char currentNode, Graph graph)
    {
        var paths = GetPaths(currentNode, graph);
        if (paths.Count == 0)
            return null;

        paths.Sort((p1, p2) =>
        {
            var gateCompare = p1.Last().CompareTo(p2.Last());
            if (gateCompare != 0)
                return gateCompare;

            if (p1.Length > 1 && p2.Length > 1)
                return p1[1].CompareTo(p2[1]);

            return 0;
        });

        return paths[0];
    }

    private static List<string> GetPaths(char currentNode, Graph graph)
    {
        var paths = new List<string>();
        var queue = new Queue<string>();
        queue.Enqueue(currentNode.ToString());

        while (queue.Count > 0)
        {
            var path = queue.Dequeue();
            var lastNode = path.Last();

            foreach (var neighbor in graph.AdjacencyList[lastNode].OrderBy(n => n))
            {
                if (path.Contains(neighbor))
                    continue;

                var newPath = path + neighbor;
                if (graph.Gates.Contains(neighbor))
                    paths.Add(newPath);
                else
                    queue.Enqueue(newPath);
            }
        }

        return paths;
    }

    static List<string> Solve(List<(string, string)> edges)
    {
        var graph = new Graph(edges);
        var result = new List<string>();
        var currentNode = 'a';

        while (true)
        {
            var cutOptions = new List<(char, char)>();
            foreach (var gate in graph.Gates.OrderBy(g => g))
                cutOptions.AddRange(graph.AdjacencyList[gate].OrderBy(n => n).Select(adjacent => (gate, adjacent)));

            if (cutOptions.Count == 0)
                break;

            foreach (var (gate, adj) in cutOptions)
            {
                graph.RemoveEdge(gate, adj);

                var path = GetShortestPath(currentNode, graph);
                if (path is null || path.Length > 2)
                {
                    result.Add($"{gate}-{adj}");
                    if (path is not null)
                        currentNode = path[1];
                    break;
                }

                graph.MakeEdge(gate, adj);
            }
        }

        return result;
    }

    static void Main()
    {
        var edges = new List<(string, string)>();
        string? line;

        while ((line = Console.ReadLine()) != null && line != "")
        {
            line = line.Trim();
            if (!string.IsNullOrEmpty(line))
            {
                var parts = line.Split('-');
                if (parts.Length == 2)
                    edges.Add((parts[0], parts[1]));
            }
        }

        var result = Solve(edges);
        foreach (var edge in result)
            Console.WriteLine(edge);
    }
}