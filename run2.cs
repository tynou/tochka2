using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static List<string> Solve(List<(string, string)> edges)
    {
        var result = new List<string>();
        var adj = new Dictionary<string, HashSet<string>>();
        foreach (var (node1, node2) in edges)
        {
            if (!adj.ContainsKey(node1)) adj[node1] = new HashSet<string>();
            adj[node1].Add(node2);
            if (!adj.ContainsKey(node2)) adj[node2] = new HashSet<string>();
            adj[node2].Add(node1);
        }
        var gates = adj.Keys.Where(node => char.IsUpper(node[0])).ToHashSet();
        var virusPosition = "a";

        while (true)
        {
            if (FindPathToClosestGate(adj, virusPosition, gates).Length == 0)
                break;
            
            var possibleMoves = new List<string>();
            foreach (var gate in gates.OrderBy(g => g))
                if (adj.TryGetValue(gate, out var neighbors))
                    foreach (var neighbor in neighbors.OrderBy(n => n))
                        possibleMoves.Add($"{gate}-{neighbor}");

            string bestMove = null;
            foreach (var move in possibleMoves)
            {
                var adjCopy = new Dictionary<string, HashSet<string>>();
                foreach (var item in adj)
                    adjCopy[item.Key] = new HashSet<string>(item.Value);
                var parts = move.Split('-');
                var node1 = parts[0];
                var node2 = parts[1];
                adjCopy[node1].Remove(node2);
                adjCopy[node2].Remove(node1);
                var virusMovePath = FindPathToClosestGate(adjCopy, virusPosition, gates);
                if (virusMovePath.Length == 1)
                    continue;
                bestMove = move;
                break;
            }
            var bestMoveParts = bestMove.Split('-');
            var gateToCut = bestMoveParts[0];
            var nodeToCut = bestMoveParts[1];
            adj[gateToCut].Remove(nodeToCut);
            adj[nodeToCut].Remove(gateToCut);
            result.Add(bestMove);
            var nextVirusPath = FindPathToClosestGate(adj, virusPosition, gates);
            if (nextVirusPath.Length > 0)
                virusPosition = nextVirusPath[0].Item2;
        }

        return result;
    }

    private static (string, string)[] FindPathToClosestGate(Dictionary<string, HashSet<string>> edgesDict,
        string position,
        ISet<string> gates)
    {
        var pathsToGates = new List<(string, string)[]>();
        foreach (var gate in gates)
        {
            var path = FindMinPathToGate(edgesDict, position, gate);
            if (path.Length > 0)
                pathsToGates.Add(path);
        }

        if (pathsToGates.Count == 0)
            return [];
        pathsToGates
            .Sort((x, y) =>
            {
                if (x.Length.CompareTo(y.Length) != 0) return x.Length.CompareTo(y.Length);
                return x.Last().Item2[0].CompareTo(y.Last().Item2[0]);
            });
        return pathsToGates[0];
    }

    private static (string, string)[] FindMinPathToGate(Dictionary<string, HashSet<string>> edgesDict,
        string position, string gate)
    {
        if (position == gate)
            return new List<(string, string)>().ToArray();
        var queue = new Queue<string>();
        var visited = new Dictionary<string, string>();
        var pathEdges = new List<(string, string)>();
        queue.Enqueue(position);
        visited[position] = null;
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current == gate)
            {
                var node = gate;
                while (visited[node] != null)
                {
                    pathEdges.Add((visited[node], node));
                    node = visited[node];
                }

                pathEdges.Reverse();
                return pathEdges.ToArray();
            }

            if (edgesDict.TryGetValue(current, out var value))
                foreach (var neighbor in value.OrderBy(n => n))
                    if (visited.TryAdd(neighbor, current))
                        queue.Enqueue(neighbor);
        }

        return [];
    }

    static void Main()
    {
        var edges = new List<(string, string)>();
        string line;

        while ((line = Console.ReadLine()) != null && line.Length > 0)
        {
            line = line.Trim();
            if (!string.IsNullOrEmpty(line))
            {
                var parts = line.Split('-');
                if (parts.Length == 2)
                {
                    edges.Add((parts[0], parts[1]));
                }
            }
        }

        var result = Solve(edges);
        foreach (var edge in result)
        {
            Console.WriteLine(edge);
        }
    }
}