using System;
using System.Collections.Generic;
using UnityEngine;

public static class DiceSolver
{
    public struct DiceOrientation
    {
        public int Top, Bottom, North, South, East, West;

        public static DiceOrientation Default => new DiceOrientation
        {
            Top = 1,
            Bottom = 6,
            North = 3,
            South = 4,
            East = 2,
            West = 5
        };
    }

    private struct DiceState
    {
        public int X, Y;
        public DiceOrientation Orientation;
    }

    /// <summary>
    /// Encuentra el camino de minimos movimientos para el dado.
    /// Devuelve una lista con el valor de la cara superior tras cada salto.
    /// Devuelve null si no hay camino posible.
    /// </summary>
    /// <param name="requiredTopAtGoal">
    /// Si es mayor que 0, el dado debe llegar a la meta con ese numero en la cara superior.
    /// Si es 0, cualquier cara es valida.
    /// </param>
    public static List<int> Solve(
        bool[] tiles,
        int width,
        int height,
        int startX,
        int startY,
        int goalX,
        int goalY,
        DiceOrientation orientation,
        int requiredTopAtGoal = 0)
    {
        var start = new DiceState { X = startX, Y = startY, Orientation = orientation };

        var visited = new HashSet<StateKey>();
        var cameFrom = new Dictionary<StateKey, (StateKey prev, int topAfterJump, DiceState state)?>();
        var queue = new Queue<DiceState>();

        var startKey = ToKey(start);
        visited.Add(startKey);
        cameFrom[startKey] = null;
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (current.X == goalX && current.Y == goalY)
            {
                bool topOk = requiredTopAtGoal == 0 || current.Orientation.Top == requiredTopAtGoal;
                if (topOk)
                    return ReconstructPath(cameFrom, ToKey(current), start, width, height, goalX, goalY);
                // Si estamos en la meta pero con la cara incorrecta, seguimos buscando
                // (el BFS continuara explorando otros caminos desde aqui)
            }

            int distance = current.Orientation.Top;

            foreach (var dir in Directions)
            {
                int dx = dir.dx;
                int dy = dir.dy;

                bool blocked = false;
                for (int step = 1; step <= distance; step++)
                {
                    int nx = current.X + dx * step;
                    int ny = current.Y + dy * step;
                    if (!IsWalkable(tiles, width, height, nx, ny))
                    {
                        blocked = true;
                        break;
                    }
                }

                if (blocked) continue;

                int destX = current.X + dx * distance;
                int destY = current.Y + dy * distance;
                var newOrientation = dir.roll(current.Orientation);

                var stepped = new DiceState { X = destX, Y = destY, Orientation = newOrientation };
                var key = ToKey(stepped);
                if (visited.Contains(key)) continue;

                visited.Add(key);
                cameFrom[key] = (ToKey(current), newOrientation.Top, stepped);
                queue.Enqueue(stepped);
            }
        }

        return null;
    }

    private static readonly (int dx, int dy, Func<DiceOrientation, DiceOrientation> roll)[] Directions =
    {
        ( 1,  0, RollEast),
        (-1,  0, RollWest),
        ( 0,  1, RollNorth),
        ( 0, -1, RollSouth)
    };

    private static DiceOrientation RollEast(DiceOrientation o) => new DiceOrientation
    {
        Top = o.West,
        Bottom = o.East,
        East = o.Top,
        West = o.Bottom,
        North = o.North,
        South = o.South
    };

    private static DiceOrientation RollWest(DiceOrientation o) => new DiceOrientation
    {
        Top = o.East,
        Bottom = o.West,
        East = o.Bottom,
        West = o.Top,
        North = o.North,
        South = o.South
    };

    private static DiceOrientation RollNorth(DiceOrientation o) => new DiceOrientation
    {
        Top = o.South,
        Bottom = o.North,
        North = o.Top,
        South = o.Bottom,
        East = o.East,
        West = o.West
    };

    private static DiceOrientation RollSouth(DiceOrientation o) => new DiceOrientation
    {
        Top = o.North,
        Bottom = o.South,
        North = o.Bottom,
        South = o.Top,
        East = o.East,
        West = o.West
    };

    private static bool IsWalkable(bool[] tiles, int width, int height, int x, int y)
        => x >= 0 && x < width && y >= 0 && y < height && tiles[y * width + x];

    private static List<int> ReconstructPath(
        Dictionary<StateKey, (StateKey prev, int topAfterJump, DiceState state)?> cameFrom,
        StateKey endKey,
        DiceState start,
        int width,
        int height,
        int goalX,
        int goalY)
    {
        var states = new List<DiceState>();
        var tops = new List<int>();
        var current = endKey;

        while (cameFrom[current].HasValue)
        {
            var (prev, top, state) = cameFrom[current].Value;
            states.Add(state);
            tops.Add(top);
            current = prev;
        }

        states.Reverse();
        tops.Reverse();

        PrintBoard(start, width, height, goalX, goalY, "INICIO", 0);

        for (int i = 0; i < states.Count; i++)
        {
            string label = states[i].X == goalX && states[i].Y == goalY ? "META" : "Salto " + (i + 1);
            PrintBoard(states[i], width, height, goalX, goalY, label, tops[i]);
        }

        return tops;
    }

    private static void PrintBoard(DiceState state, int width, int height, int goalX, int goalY, string label, int top)
    {
        string header = label + (top > 0 ? "  (Top=" + top + ")" : "");
        var sb = new System.Text.StringBuilder();
        sb.AppendLine(header);

        for (int y = height - 1; y >= 0; y--)
        {
            string row = "";
            for (int x = 0; x < width; x++)
            {
                if (x == state.X && y == state.Y)
                    row += "[" + state.Orientation.Top + "]";
                else if (x == goalX && y == goalY)
                    row += "[G]";
                else
                    row += "[ ]";
            }
            sb.AppendLine(row);
        }

        sb.AppendLine("Top=" + state.Orientation.Top + " Bottom=" + state.Orientation.Bottom +
                      " North=" + state.Orientation.North + " South=" + state.Orientation.South +
                      " East=" + state.Orientation.East + " West=" + state.Orientation.West);

        Debug.Log(sb.ToString());
    }

    private readonly struct StateKey : IEquatable<StateKey>
    {
        public readonly int X, Y, Top, Bottom, North, South, East, West;

        public StateKey(int x, int y, int top, int bottom, int north, int south, int east, int west)
        {
            X = x; Y = y;
            Top = top; Bottom = bottom;
            North = north; South = south;
            East = east; West = west;
        }

        public bool Equals(StateKey o) =>
            X == o.X && Y == o.Y &&
            Top == o.Top && Bottom == o.Bottom &&
            North == o.North && South == o.South &&
            East == o.East && West == o.West;

        public override bool Equals(object obj) => obj is StateKey k && Equals(k);

        public override int GetHashCode() =>
            HashCode.Combine(X, Y, Top, Bottom, North, South, East, West);
    }

    private static StateKey ToKey(DiceState d) => new StateKey(
        d.X, d.Y,
        d.Orientation.Top, d.Orientation.Bottom,
        d.Orientation.North, d.Orientation.South,
        d.Orientation.East, d.Orientation.West
    );
}