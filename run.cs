class Program
{
    private static readonly Dictionary<char, int> EnergyCost = new()
    {
        { 'A', 1 }, { 'B', 10 }, { 'C', 100 }, { 'D', 1000 }
    };

    private static readonly Dictionary<char, int> TargetRoomIndex = new()
    {
        { 'A', 0 }, { 'B', 1 }, { 'C', 2 }, { 'D', 3 }
    };

    private static readonly int[] RoomPositions = [2, 4, 6, 8];

    private static readonly int[] ValidHallwayStops = [0, 1, 3, 5, 7, 9, 10];

    private record State
    {
        public string Hallway { get; init; }
        public IReadOnlyList<string> Rooms { get; init; }
        public int RoomDepth { get; init; }

        public override int GetHashCode()
        {
            var hashCode = Hallway.GetHashCode();
            foreach (var room in Rooms)
                hashCode = HashCode.Combine(hashCode, room.GetHashCode());
            return hashCode;
        }

        public virtual bool Equals(State? other)
        {
            if (other is null) return false;
            if (Hallway != other.Hallway) return false;
            for (var i = 0; i < Rooms.Count; i++)
                if (Rooms[i] != other.Rooms[i]) return false;
            return true;
        }
    }

    static int Solve(List<string> lines)
    {
        var initialState = ParseState(lines);
        if (initialState is null) return -1;

        var gScore = new Dictionary<State, int>();
        var openSet = new PriorityQueue<State, int>();

        gScore[initialState] = 0;
        openSet.Enqueue(initialState, CalculateHeuristic(initialState));

        while (openSet.Count > 0)
        {
            var current = openSet.Dequeue();

            if (IsFinalState(current))
                return gScore[current];

            foreach (var (neighbor, moveCost) in GetNextStates(current))
            {
                var stateScore = gScore[current] + moveCost;

                if (stateScore < gScore.GetValueOrDefault(neighbor, int.MaxValue))
                {
                    gScore[neighbor] = stateScore;
                    var fScore = stateScore + CalculateHeuristic(neighbor);
                    openSet.Enqueue(neighbor, fScore);
                }
            }
        }

        return 0;
    }

    private static IEnumerable<(State newState, int cost)> GetNextStates(State state)
    {
        // из коридора в комнату
        for (var i = 0; i < state.Hallway.Length; i++)
        {
            var hallwayObject = state.Hallway[i];
            if (hallwayObject == '.') continue;

            var targetRoom = TargetRoomIndex[hallwayObject];
            var targetObject = "ABCD"[targetRoom];

            var roomIsReady = state.Rooms[targetRoom].All(c => c == '.' || c == targetObject);
            if (!roomIsReady) continue;

            var roomX = RoomPositions[targetRoom];
            var start = Math.Min(i, roomX);
            var end = Math.Max(i, roomX);
            var pathIsClear = true;
            for (var j = start; j <= end; j++)
            {
                if (i != j && state.Hallway[j] != '.')
                {
                    pathIsClear = false;
                    break;
                }
            }

            if (!pathIsClear) continue;

            var targetDepth = -1;
            for (var d = state.RoomDepth - 1; d >= 0; d--)
            {
                if (state.Rooms[targetRoom][d] == '.')
                {
                    targetDepth = d;
                    break;
                }
            }

            if (targetDepth == -1) continue;

            var hallwayDist = Math.Abs(i - roomX);
            var roomDist = targetDepth + 1;
            var cost = (hallwayDist + roomDist) * EnergyCost[hallwayObject];

            var newHallway = state.Hallway.ToCharArray();
            newHallway[i] = '.';

            var newRooms = state.Rooms.Select(r => r.ToCharArray()).ToList();
            newRooms[targetRoom][targetDepth] = hallwayObject;

            yield return (
                new State
                {
                    Hallway = new string(newHallway),
                    Rooms = newRooms.Select(r => new string(r)).ToList(),
                    RoomDepth = state.RoomDepth
                },
                cost
                );
        }

        //  из комнаты в коридор
        for (var roomIndex = 0; roomIndex < 4; roomIndex++)
        {
            var needsToMoveOut = !state.Rooms[roomIndex].All(c => c == '.' || TargetRoomIndex[c] == roomIndex);

            if (!needsToMoveOut) continue;

            var depthToMove = -1;
            var objectToMove = '.';
            for (var d = 0; d < state.RoomDepth; d++)
            {
                if (state.Rooms[roomIndex][d] != '.')
                {
                    depthToMove = d;
                    objectToMove = state.Rooms[roomIndex][d];
                    break;
                }
            }

            if (objectToMove == '.') continue;

            foreach (var stopPos in ValidHallwayStops)
            {
                var roomX = RoomPositions[roomIndex];
                var start = Math.Min(stopPos, roomX);
                var end = Math.Max(stopPos, roomX);
                var pathIsClear = true;
                for (var j = start; j <= end; j++)
                {
                    if (state.Hallway[j] != '.')
                    {
                        pathIsClear = false;
                        break;
                    }
                }

                if (!pathIsClear) continue;

                var hallwayDist = Math.Abs(stopPos - roomX);
                var roomDist = depthToMove + 1;
                var cost = (hallwayDist + roomDist) * EnergyCost[objectToMove];

                var newHallway = state.Hallway.ToCharArray();
                newHallway[stopPos] = objectToMove;

                var newRooms = state.Rooms.Select(r => r.ToCharArray()).ToList();
                newRooms[roomIndex][depthToMove] = '.';

                yield return (
                    new State
                    {
                        Hallway = new string(newHallway),
                        Rooms = newRooms.Select(r => new string(r)).ToList(),
                        RoomDepth = state.RoomDepth
                    },
                    cost
                    );
            }
        }
    }

    private static int CalculateHeuristic(State state)
    {
        var totalCost = 0;

        // для объектов в коридоре
        for (var i = 0; i < state.Hallway.Length; i++)
        {
            var hallwayObject = state.Hallway[i];
            if (hallwayObject == '.') continue;

            var targetRoom = TargetRoomIndex[hallwayObject];
            var roomX = RoomPositions[targetRoom];
            totalCost += (Math.Abs(i - roomX) + 1) * EnergyCost[hallwayObject];
        }

        // для объектов в комнатах
        for (var roomIndex = 0; roomIndex < 4; roomIndex++)
        {
            for (var d = 0; d < state.RoomDepth; d++)
            {
                var roomObject = state.Rooms[roomIndex][d];
                if (roomObject == '.') continue;

                var targetRoom = TargetRoomIndex[roomObject];
                if (targetRoom == roomIndex) continue;

                var currentRoomX = RoomPositions[roomIndex];
                var targetRoomX = RoomPositions[targetRoom];

                totalCost += (Math.Abs(currentRoomX - targetRoomX) + d + 2) * EnergyCost[roomObject];
            }
        }
        return totalCost;
    }

    private static bool IsFinalState(State state)
    {
        for (var i = 0; i < 4; i++)
        {
            var targetObject = "ABCD"[i];
            foreach (var c in state.Rooms[i])
                if (c != targetObject) return false;
        }
        return true;
    }

    private static State? ParseState(List<string> lines)
    {
        if (lines.Count < 5) return null;
        var hallway = lines[1].Substring(1, 11);
        var roomDepth = lines.Count - 3;
        var rooms = new List<string> { "", "", "", "" };

        for (var d = 0; d < roomDepth; d++)
        {
            rooms[0] += lines[2 + d][3];
            rooms[1] += lines[2 + d][5];
            rooms[2] += lines[2 + d][7];
            rooms[3] += lines[2 + d][9];
        }

        return new State { Hallway = hallway, Rooms = rooms, RoomDepth = roomDepth };
    }

    static void Main()
    {
        var lines = new List<string>();
        string? line;

        while ((line = Console.ReadLine()) != null && line != "")
            lines.Add(line);

        var result = Solve(lines);
        Console.WriteLine(result);
    }
}