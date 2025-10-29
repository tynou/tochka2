using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    // Класс для представления графа и управления им
    class Graph
    {
        public Dictionary<string, HashSet<string>> AdjacencyList { get; } = new Dictionary<string, HashSet<string>>();

        public void AddEdge(string u, string v)
        {
            if (!AdjacencyList.ContainsKey(u)) AdjacencyList[u] = new HashSet<string>();
            if (!AdjacencyList.ContainsKey(v)) AdjacencyList[v] = new HashSet<string>();
            AdjacencyList[u].Add(v);
            AdjacencyList[v].Add(u);
        }

        public void RemoveEdge(string u, string v)
        {
            if (AdjacencyList.ContainsKey(u)) AdjacencyList[u].Remove(v);
            if (AdjacencyList.ContainsKey(v)) AdjacencyList[v].Remove(u);
        }
    }

    // Реализация Поиска в ширину (BFS)
    // Возвращает словарь {узел -> расстояние от startNode}
    static Dictionary<string, int> Bfs(string startNode, Graph graph)
    {
        var distances = new Dictionary<string, int>();
        if (!graph.AdjacencyList.ContainsKey(startNode))
        {
            return distances; // Стартовый узел не в графе
        }

        var queue = new Queue<string>();
        
        queue.Enqueue(startNode);
        distances[startNode] = 0;

        while (queue.Count > 0)
        {
            var currentNode = queue.Dequeue();
            foreach (var neighbor in graph.AdjacencyList[currentNode].OrderBy(n => n))
            {
                if (!distances.ContainsKey(neighbor))
                {
                    distances[neighbor] = distances[currentNode] + 1;
                    queue.Enqueue(neighbor);
                }
            }
        }
        return distances;
    }

    static List<string> Solve(List<(string, string)> edges)
    {
        var result = new List<string>();
        var graph = new Graph();
        var gateways = new HashSet<string>();
        string virusPos = "a";

        // 1. Построение графа и определение шлюзов
        foreach (var (u, v) in edges)
        {
            graph.AddEdge(u, v);
            if (char.IsUpper(u[0])) gateways.Add(u);
            if (char.IsUpper(v[0])) gateways.Add(v);
        }

        // 2. Основной цикл симуляции
        while (true)
        {
            // --- ЭТАП 1: Анализ от лица вируса (чтобы мы могли принять решение) ---

            // 1a. Находим кратчайшие пути от вируса
            var distFromVirus = Bfs(virusPos, graph);

            // 1b. Ищем целевой шлюз
            string targetGateway = null;
            int minDistance = int.MaxValue;

            // Используем SortedSet для автоматической сортировки шлюзов по имени
            foreach (var gateway in gateways.OrderBy(g => g))
            {
                int currentGatewayDist = int.MaxValue;
                if (!graph.AdjacencyList.ContainsKey(gateway)) continue;

                // Расстояние до шлюза = 1 + минимальное расстояние до его соседа
                foreach (var neighbor in graph.AdjacencyList[gateway])
                {
                    if (distFromVirus.TryGetValue(neighbor, out int dist))
                    {
                        currentGatewayDist = Math.Min(currentGatewayDist, dist + 1);
                    }
                }
                
                if (currentGatewayDist < minDistance)
                {
                    minDistance = currentGatewayDist;
                    targetGateway = gateway;
                }
            }
            
            // 1c. Если шлюзы недостижимы, игра окончена
            if (targetGateway == null)
            {
                break;
            }

            // --- ЭТАП 2: Наш ход (отключение коридора) ---

            // 2a. Находим узел, который нужно отключить от targetGateway.
            // Это сосед шлюза на кратчайшем пути от вируса.
            // По правилу детерминированности, выбираем лексикографически наименьший.
            string nodeToCut = graph.AdjacencyList[targetGateway]
                .Where(neighbor => distFromVirus.ContainsKey(neighbor) && distFromVirus[neighbor] + 1 == minDistance)
                .OrderBy(n => n)
                .First();

            // 2b. Форматируем и сохраняем результат, отключаем коридор
            string edgeToCut = $"{targetGateway}-{nodeToCut}";
            result.Add(edgeToCut);
            graph.RemoveEdge(targetGateway, nodeToCut);

            // --- ЭТАП 3: Ход вируса (перемещение) ---
            
            // 3a. Находим путь вируса на графе, который был ДО нашего хода.
            // Для этого временно вернем удаленное ребро.
            graph.AddEdge(targetGateway, nodeToCut);
            var distToGateway = Bfs(targetGateway, graph);
            graph.RemoveEdge(targetGateway, nodeToCut); // И уберем его снова

            // 3b. Определяем следующий шаг вируса
            string virusNextNode = null;
            if (graph.AdjacencyList.ContainsKey(virusPos))
            {
                virusNextNode = graph.AdjacencyList[virusPos]
                    .Where(neighbor => distToGateway.ContainsKey(neighbor) && distToGateway[neighbor] < distToGateway[virusPos])
                    .OrderBy(n => n)
                    .FirstOrDefault();
            }

            // 3c. Обновляем позицию вируса
            if (virusNextNode != null)
            {
                virusPos = virusNextNode;
            }
            else
            {
                 // Вирусу некуда идти, чтобы приблизиться к цели (например, мы отрезали единственный путь)
                 // В рамках задачи, это состояние эквивалентно победе, так как он не может достичь шлюза.
                 // Можно было бы завершить цикл, но он завершится на следующей итерации, когда targetGateway не будет найден.
            }
        }

        return result;
    }

    static void Main()
    {
        var edges = new List<(string, string)>();
        string line;

        while ((line = Console.ReadLine()) != null && line != "")
        {
            line = line.Trim();
            if (!string.IsNullOrEmpty(line) && line != "")
            {
                var parts = line.Split('-');
                if (parts.Length == 2)
                {
                    // Гарантируем лексикографический порядок для удобства, хотя для HashSet это неважно
                    if (string.Compare(parts[0], parts[1]) > 0)
                    {
                        edges.Add((parts[1], parts[0]));
                    }
                    else
                    {
                        edges.Add((parts[0], parts[1]));
                    }
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