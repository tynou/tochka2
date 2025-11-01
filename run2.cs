using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public void RemoveEdge(char node1, char node2)
        {
            if (AdjacencyList.TryGetValue(node1, out var value1))
                value1.Remove(node2);

            if (AdjacencyList.TryGetValue(node2, out var value2))
                value2.Remove(node1);
        }
    }

    private static string GetVirusPath(List<string> paths)
    {
        var minLength = paths.Select(path => path.Length).Min();
        var shortestPaths = paths.Where(path => path.Length == minLength).ToList();
        shortestPaths.Sort((p1, p2) =>
        {
            var gateCompare = p1.Last().CompareTo(p2.Last());
            if (gateCompare != 0)
                return gateCompare;

            if (p1.Length > 1 && p2.Length > 1)
                return p1[1].CompareTo(p2[1]);

            return 0;
        });

        return shortestPaths[0];
    }

    private static List<string> GetShortestRoutes(char currentNode, Graph graph)
    {
        var shortestPaths = new List<string>();
        var queue = new Queue<string>();
        queue.Enqueue(currentNode.ToString());

        while (queue.Count > 0)
        {
            var currentLevelSize = queue.Count;
            for (var i = 0; i < currentLevelSize; i++)
            {
                var path = queue.Dequeue();
                var lastNode = path.Last();

                foreach (var neighbor in graph.AdjacencyList[lastNode].OrderBy(n => n))
                {
                    if (path.Contains(neighbor))
                        continue;

                    var newPath = path + neighbor;
                    if (graph.Gates.Contains(neighbor))
                        shortestPaths.Add(newPath);
                    else
                        queue.Enqueue(newPath);
                }
            }

            if (shortestPaths.Count > 0)
                return shortestPaths;
        }

        return shortestPaths;
    }

    static List<string> Solve(List<(string, string)> edges)
    {
        var graph = new Graph(edges);
        var result = new List<string>();
        var currentNode = 'a';

        while (true)
        {
            var shortestPaths = GetShortestRoutes(currentNode, graph);

            if (shortestPaths.Count == 0)
                break;

            var chosenPath = GetVirusPath(shortestPaths);

            var gateway = chosenPath.Last();
            var adjacentNode = chosenPath[^2];

            result.Add($"{gateway}-{adjacentNode}");
            graph.RemoveEdge(gateway, adjacentNode);

            var pathsForMovement = GetShortestRoutes(currentNode, graph);

            if (pathsForMovement.Count != 0)
            {
                var actualMovePath = GetVirusPath(pathsForMovement);

                if (actualMovePath.Length > 1)
                    currentNode = actualMovePath[1];
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