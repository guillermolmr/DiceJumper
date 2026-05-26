using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static DiceSolver;

public static class DiceSolverSpecial
{
    // -----------------------------------------------------------------------
    // Estructuras de estado
    // -----------------------------------------------------------------------

    private struct DiceState
    {
        public int X, Y;
        public DiceOrientation Orientation;
    }

    // Estado completo del mundo en un momento dado
    private struct WorldState
    {
        public DiceState Dice;
        public ulong YellowMask;  // bit i = tile amarilla i esta activa
        public ulong GlassMask;   // bit i = tile de cristal i esta rota
    }

    private struct StateKey : IEquatable<StateKey>
    {
        public int X, Y, Top, Bottom, North, South, East, West;
        public ulong YellowMask;
        public ulong GlassMask;

        public bool Equals(StateKey o) =>
            X == o.X && Y == o.Y &&
            Top == o.Top && Bottom == o.Bottom &&
            North == o.North && South == o.South &&
            East == o.East && West == o.West &&
            YellowMask == o.YellowMask &&
            GlassMask == o.GlassMask;

        public override bool Equals(object obj) => obj is StateKey k && Equals(k);

        public override int GetHashCode()
        {
            int h = HashCode.Combine(X, Y, Top, Bottom, North, South, East, West);
            h = HashCode.Combine(h, YellowMask.GetHashCode(), GlassMask.GetHashCode());
            return h;
        }
    }

    // -----------------------------------------------------------------------
    // Entrada publica
    // -----------------------------------------------------------------------

    public static List<int> Solve(
    CustomLevel level,
    DiceOrientation startOrientation,
    int requiredTopAtGoal = 0)
    {
        // REPLACED: List<DebugState> steps = new List<DebugState>();
        List<BfsStateNode> allStates = new List<BfsStateNode>();
        Dictionary<StateKey, int> keyToIndex = new Dictionary<StateKey, int>();

        var yellowTiles = new List<CustomLevel.SpecialTile>();
        var glassTiles = new List<CustomLevel.SpecialTile>();

        foreach (var st in level.specialTiles)
        {
            if (st.type == TileType.Yellow) yellowTiles.Add(st);
            if (st.type == TileType.Glass) glassTiles.Add(st);
        }

        if (yellowTiles.Count > 64 || glassTiles.Count > 64)
        {
            Debug.LogError("Demasiadas tiles especiales para el bitmask (max 64).");
            return null;
        }
        else
        {
            Debug.Log($"Yellow: {yellowTiles.Count}\tglass: {glassTiles.Count}");
        }

        ulong initialYellow = 0;
        for (int i = 0; i < yellowTiles.Count; i++)
            if (yellowTiles[i].value != 0)
                initialYellow |= (1UL << i);

        var startDice = new DiceState
        {
            X = level.startPosition.x,
            Y = level.startPosition.y,
            Orientation = startOrientation
        };

        var startWorld = new WorldState
        {
            Dice = startDice,
            YellowMask = initialYellow,
            GlassMask = 0
        };

        var visited = new HashSet<StateKey>();
        var cameFrom = new Dictionary<StateKey, (StateKey prev, int topAfterJump, WorldState state)?>();
        var queue = new Queue<WorldState>();

        var startKey = ToKey(startWorld);
        visited.Add(startKey);
        cameFrom[startKey] = null;
        queue.Enqueue(startWorld);

        // Record Start State
        allStates.Add(new BfsStateNode
        {
            index = 0,
            x = startWorld.Dice.X,
            y = startWorld.Dice.Y,
            top = startWorld.Dice.Orientation.Top,
            yellowMask = startWorld.YellowMask,
            glassMask = startWorld.GlassMask,
            parentIndex = -1
        });
        keyToIndex[startKey] = 0;

        int goalX = level.goalPosition.x;
        int goalY = level.goalPosition.y;
        int goalStateIndex = -1;

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            var dice = current.Dice;

            if (dice.X == goalX && dice.Y == goalY)
            {
                bool topOk = requiredTopAtGoal <= 0 || dice.Orientation.Top == requiredTopAtGoal;
                if (topOk)
                {
                    goalStateIndex = keyToIndex[ToKey(current)];
                    return ReconstructPath(cameFrom, ToKey(current), startWorld, level, yellowTiles, glassTiles, goalX, goalY);
                }
            }

            int distance = dice.Orientation.Top;

            foreach (var dir in Directions)
            {
                int dx = dir.dx;
                int dy = dir.dy;

                bool blocked = false;
                for (int step = 1; step <= distance; step++)
                {
                    int nx = dice.X + dx * step;
                    int ny = dice.Y + dy * step;
                    bool passable = IsTilePassable(nx, ny, level, yellowTiles, glassTiles, current.YellowMask, current.GlassMask);
                    if (dice.X == 5 && dice.Y == 3 && dice.Orientation.Top==2)
                    {
                        Debug.Log($"  Desde (5,3) step={step} -> ({nx},{ny}) passable={passable} " +
                                  $"IsFloor={level.IsFloor(nx, ny)} " +
                                  $"isSpecial={level.specialTiles.Any(s => s.position.x == nx && s.position.y == ny)} " +
                                  $"specialType={level.specialTiles.FirstOrDefault(s => s.position.x == nx && s.position.y == ny)?.type}");
                    }


                    if (!passable)
                    {
                        blocked = true;
                        break;
                    }

                }

                if (blocked) continue;

                // Redundant check removed for performance, logic relies on the loop above

                int destX = dice.X + dx * distance;
                int destY = dice.Y + dy * distance;

                var newOrientation = dir.roll(dice.Orientation);
                var newDice = new DiceState { X = destX, Y = destY, Orientation = newOrientation };

                var newWorld = ApplyTileEffects(newDice, current.YellowMask, current.GlassMask,
                                                level, yellowTiles, glassTiles);

                var key = ToKey(newWorld);
                if (visited.Contains(key)) continue;

                visited.Add(key);

                // Record new state for visualization
                int newIndex = allStates.Count;
                keyToIndex[key] = newIndex;
                allStates.Add(new BfsStateNode
                {
                    index = newIndex,
                    x = newWorld.Dice.X,
                    y = newWorld.Dice.Y,
                    top = newWorld.Dice.Orientation.Top,
                    yellowMask = newWorld.YellowMask,
                    glassMask = newWorld.GlassMask,
                    parentIndex = keyToIndex[ToKey(current)]
                });

                cameFrom[key] = (ToKey(current), newWorld.Dice.Orientation.Top, newWorld);
                queue.Enqueue(newWorld);
            }
        }

        // If no solution found, save the full BFS tree to JSON to see where it got stuck
        SaveBfsJson(level,allStates, goalStateIndex, "bfs_output.json");
        return null;
    }

    private static void SaveBfsJson(CustomLevel level, List<BfsStateNode> states, int goalIndex, string filename)
    {
        var output = new BfsOutputData
        {
            goalFound = goalIndex != -1,
            goalStateIndex = goalIndex,
            totalVisited = states.Count,
            states = states,
            levelData = new CustomLevelData(level)
        };

        string json = JsonUtility.ToJson(output, true);
        File.WriteAllText(filename, json);
        Debug.Log($"BFS Visualization data saved to {filename}. Total states: {states.Count}");
    }

    public static List<int> Solve_old(
        CustomLevel level,
        DiceOrientation startOrientation,
        int requiredTopAtGoal = 0)
    {

        List<DebugState> steps= new List<DebugState>();
        // Indice de cada special tile para los bitmasks
        var yellowTiles = new List<CustomLevel.SpecialTile>();
        var glassTiles = new List<CustomLevel.SpecialTile>();

        foreach (var st in level.specialTiles)
        {
            if (st.type == TileType.Yellow) yellowTiles.Add(st);
            if (st.type == TileType.Glass) glassTiles.Add(st);
        }

        if (yellowTiles.Count > 64 || glassTiles.Count > 64)
        {
            Debug.LogError("Demasiadas tiles especiales para el bitmask (max 64).");
            return null;
        }
        else
        {
            Debug.LogError($"Yellow: {yellowTiles.Count}\tglass: {glassTiles.Count}");
        }

            // Estado inicial de amarillas
            ulong initialYellow = 0;
        for (int i = 0; i < yellowTiles.Count; i++)
            if (yellowTiles[i].value != 0)
                initialYellow |= (1UL << i);

        var startDice = new DiceState
        {
            X = level.startPosition.x,
            Y = level.startPosition.y,
            Orientation = startOrientation
        };

        var startWorld = new WorldState
        {
            Dice = startDice,
            YellowMask = initialYellow,
            GlassMask = 0
        };

        var visited = new HashSet<StateKey>();
        var cameFrom = new Dictionary<StateKey, (StateKey prev, int topAfterJump, WorldState state)?>();
        var queue = new Queue<WorldState>();

        var startKey = ToKey(startWorld);
        visited.Add(startKey);
        cameFrom[startKey] = null;
        queue.Enqueue(startWorld);

        int goalX = level.goalPosition.x;
        int goalY = level.goalPosition.y;
        //Debug.Log($"=== SOLVER START: ({startDice.X},{startDice.Y}) Top={startOrientation.Top} -> goal=({goalX},{goalY}) ===");
        //Debug.Log($"IsFloor start={level.IsFloor(startDice.X, startDice.Y)} IsFloor goal={level.IsFloor(goalX, goalY)}");
        //Debug.Log($"startPosition={level.startPosition} goalPosition={level.goalPosition}");
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            var dice = current.Dice;

            if (dice.X == goalX && dice.Y == goalY)
            {
                bool topOk = requiredTopAtGoal <= 0 || dice.Orientation.Top == requiredTopAtGoal;
                if (topOk)
                    return ReconstructPath(cameFrom, ToKey(current), startWorld, level, yellowTiles, glassTiles, goalX, goalY);
            }

            int distance = dice.Orientation.Top;
            //Debug.Log($"Procesando ({dice.X},{dice.Y}) Top={dice.Orientation.Top}");
            foreach (var dir in Directions)
            {
                int dx = dir.dx;
                int dy = dir.dy;

                // Recorrer casillas intermedias comprobando bloqueos
                bool blocked = false;
                for (int step = 1; step <= distance; step++)
                {
                    int nx = dice.X + dx * step;
                    int ny = dice.Y + dy * step;

                    if (!IsTilePassable(nx, ny, level, yellowTiles, glassTiles, current.YellowMask, current.GlassMask))
                    {
                        
                        blocked = true;
                        break;
                    }
                }

                if (blocked) continue;
                for (int step = 1; step <= distance; step++)
                {
                    int nx = dice.X + dx * step;
                    int ny = dice.Y + dy * step;

                    bool passable = IsTilePassable(nx, ny, level, yellowTiles, glassTiles, current.YellowMask, current.GlassMask);
                    //Debug.Log($"  Step {step}: ({nx},{ny}) passable={passable} isFloor={level.IsFloor(nx, ny)}");
                    
                    if (!passable)
                    {
                        blocked = true;
                        break;
                    }
                }
                int destX = dice.X + dx * distance;
                int destY = dice.Y + dy * distance;

                // Aplicar rotacion del dado (una sola vez)
                var newOrientation = dir.roll(dice.Orientation);
                var newDice = new DiceState { X = destX, Y = destY, Orientation = newOrientation };

                // Aplicar efectos de la tile destino
                var newWorld = ApplyTileEffects(newDice, current.YellowMask, current.GlassMask,
                                                level, yellowTiles, glassTiles);

                var key = ToKey(newWorld);
                if (visited.Contains(key)) continue;

                visited.Add(key);
                cameFrom[key] = (ToKey(current), newWorld.Dice.Orientation.Top, newWorld);
                queue.Enqueue(newWorld);
                steps.Add(new DebugState(destX, destY, newOrientation.Top));
            }
        }
        SaveToTxt(new ListWrapper(level, steps),"levelData.json");
        //Debug.Log($"Sin solucion. Estados explorados: {visited.Count}");
        return null;
    }
    
    public static void SaveToTxt(ListWrapper data, string fileName)
    {
        string json = JsonUtility.ToJson(data, true);   // pretty-print
        string path = Path.Combine(Application.persistentDataPath, fileName);

        File.WriteAllText(path, json);

        Debug.Log("JSON guardado en: " + path);
    }
    

    // -----------------------------------------------------------------------
    // Logica de tiles
    // -----------------------------------------------------------------------

    private static bool IsTilePassable(
        int x, int y,
        CustomLevel level,
        List<CustomLevel.SpecialTile> yellowTiles,
        List<CustomLevel.SpecialTile> glassTiles,
        ulong yellowMask,
        ulong glassMask)
    {

        if (level.goalPosition.y == y && level.goalPosition.x == x) return true;

        if (!level.IsFloor(x, y)) return false;

        var pos = new Vector2Int(x, y);

        // Tile amarilla desactivada bloquea el paso
        for (int i = 0; i < yellowTiles.Count; i++)
        {
            if (yellowTiles[i].position == pos)
            {
                bool active = (yellowMask & (1UL << i)) != 0;
                if (!active) return false;
            }
        }

        // Tile de cristal rota bloquea el paso
        for (int i = 0; i < glassTiles.Count; i++)
        {
            if (glassTiles[i].position == pos)
            {
                bool broken = (glassMask & (1UL << i)) != 0;
                if (broken) return false;
            }
        }

        return true;
    }

    private static WorldState ApplyTileEffects(
        DiceState dice,
        ulong yellowMask,
        ulong glassMask,
        CustomLevel level,
        List<CustomLevel.SpecialTile> yellowTiles,
        List<CustomLevel.SpecialTile> glassTiles)
    {
        var pos = new Vector2Int(dice.X, dice.Y);
        var special = level.GetSpecialTileAtPosition(pos);

        if (special == null)
            return new WorldState { Dice = dice, YellowMask = yellowMask, GlassMask = glassMask };

        switch (special.type)
        {
            case TileType.Teleport:
                {
                    // Mover el dado a la tile destino sin cambiar orientacion
                    var target = special.targets[0];
                    dice.X = target.x;
                    dice.Y = target.y;
                    break;
                }

            case TileType.PressurePlate:
                {
                    // Toggle de todas las tiles amarillas asignadas
                    foreach (var target in special.targets)
                    {
                        for (int i = 0; i < yellowTiles.Count; i++)
                        {
                            if (yellowTiles[i].position == target)
                                yellowMask ^= (1UL << i);
                        }
                    }
                    break;
                }

            case TileType.RotateLeft:
                {
                    // Girar el dado 90 grados antihorario visto desde arriba
                    // North->West, West->South, South->East, East->North
                    dice.Orientation = new DiceOrientation
                    {
                        Top = dice.Orientation.Top,
                        Bottom = dice.Orientation.Bottom,
                        North = dice.Orientation.East,
                        East = dice.Orientation.South,
                        South = dice.Orientation.West,
                        West = dice.Orientation.North
                    };
                    break;
                }

            case TileType.RotateRight:
                {
                    // Girar el dado 90 grados horario visto desde arriba
                    // North->East, East->South, South->West, West->North
                    dice.Orientation = new DiceOrientation
                    {
                        Top = dice.Orientation.Top,
                        Bottom = dice.Orientation.Bottom,
                        North = dice.Orientation.West,
                        West = dice.Orientation.South,
                        South = dice.Orientation.East,
                        East = dice.Orientation.North
                    };
                    break;
                }

            case TileType.Glass:
                {
                    // Marcar esta tile de cristal como rota
                    for (int i = 0; i < glassTiles.Count; i++)
                    {
                        if (glassTiles[i].position == pos)
                            glassMask |= (1UL << i);
                    }
                    break;
                }

                // TargetTeleport y Yellow no tienen efecto al caer sobre ellas
        }

        return new WorldState { Dice = dice, YellowMask = yellowMask, GlassMask = glassMask };
    }

    // -----------------------------------------------------------------------
    // Rotaciones del dado
    // -----------------------------------------------------------------------

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

    // -----------------------------------------------------------------------
    // Debug y reconstruccion
    // -----------------------------------------------------------------------

    private static List<int> ReconstructPath(
        Dictionary<StateKey, (StateKey prev, int topAfterJump, WorldState state)?> cameFrom,
        StateKey endKey,
        WorldState start,
        CustomLevel level,
        List<CustomLevel.SpecialTile> yellowTiles,
        List<CustomLevel.SpecialTile> glassTiles,
        int goalX,
        int goalY)
    {
        var states = new List<WorldState>();
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

        PrintBoard(start, level, yellowTiles, glassTiles, goalX, goalY, "INICIO", 0);
        for (int i = 0; i < states.Count; i++)
        {
            string label = states[i].Dice.X == goalX && states[i].Dice.Y == goalY ? "META" : "Salto " + (i + 1);
            PrintBoard(states[i], level, yellowTiles, glassTiles, goalX, goalY, label, tops[i]);
        }

        return tops;
    }

    private static void PrintBoard(
        WorldState world,
        CustomLevel level,
        List<CustomLevel.SpecialTile> yellowTiles,
        List<CustomLevel.SpecialTile> glassTiles,
        int goalX, int goalY,
        string label, int top)
    {
        var dice = world.Dice;
        var sb = new System.Text.StringBuilder();
        sb.AppendLine(label + (top > 0 ? "  (Top=" + top + ")" : ""));

        for (int y = level.height - 1; y >= 0; y--)
        {
            string row = "";
            for (int x = 0; x < level.width; x++)
            {
                var pos = new Vector2Int(x, y);

                if (x == dice.X && y == dice.Y)
                {
                    row += "[" + dice.Orientation.Top + "]";
                    continue;
                }

                if (x == goalX && y == goalY) { row += "[G]"; continue; }

                if (!level.IsFloor(x, y)) { row += "   "; continue; }

                var special = level.GetSpecialTileAtPosition(pos);
                if (special != null)
                {
                    switch (special.type)
                    {
                        case TileType.Yellow:
                            int yi = yellowTiles.IndexOf(special);
                            bool active = (world.YellowMask & (1UL << yi)) != 0;
                            row += active ? "[Y]" : "[y]";
                            break;
                        case TileType.Glass:
                            int gi = glassTiles.IndexOf(special);
                            bool broken = (world.GlassMask & (1UL << gi)) != 0;
                            row += broken ? "[X]" : "[=]";
                            break;
                        case TileType.Teleport: row += "[T]"; break;
                        case TileType.TargetTeleport: row += "[t]"; break;
                        case TileType.PressurePlate: row += "[P]"; break;
                        case TileType.RotateLeft: row += "[<]"; break;
                        case TileType.RotateRight: row += "[>]"; break;
                        default: row += "[ ]"; break;
                    }
                }
                else
                {
                    row += "[ ]";
                }
            }
            sb.AppendLine(row);
        }

        sb.AppendLine("Top=" + dice.Orientation.Top + " Bot=" + dice.Orientation.Bottom +
                      " N=" + dice.Orientation.North + " S=" + dice.Orientation.South +
                      " E=" + dice.Orientation.East + " W=" + dice.Orientation.West);
        //Debug.Log(sb.ToString());
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static StateKey ToKey(WorldState w) => new StateKey
    {
        X = w.Dice.X,
        Y = w.Dice.Y,
        Top = w.Dice.Orientation.Top,
        Bottom = w.Dice.Orientation.Bottom,
        North = w.Dice.Orientation.North,
        South = w.Dice.Orientation.South,
        East = w.Dice.Orientation.East,
        West = w.Dice.Orientation.West,
        YellowMask = w.YellowMask,
        GlassMask = w.GlassMask
    };
}

[Serializable]
public class BfsStateNode
{
    public int index;
    public int x;
    public int y;
    public int top;
    public ulong yellowMask;
    public ulong glassMask;
    public int parentIndex; // -1 for the start node
}

[Serializable]
public class BfsOutputData
{
    public bool goalFound;
    public int goalStateIndex; // -1 if not found
    public int totalVisited;
    public List<BfsStateNode> states;
    public CustomLevelData levelData;
}

[Serializable]
public class CustomLevelData
{
    public int width;
    public int height;
    public Vector2Int startPosition;
    public Vector2Int goalPosition;
    public int startingDots;
    public int goalDots;
    public bool[] floorTiles;
    public List<CustomLevel.SpecialTile> specialTiles;

    public CustomLevelData(CustomLevel level)
    {
        width = level.width;
        height = level.height;
        startPosition = level.startPosition;
        goalPosition = level.goalPosition;
        startingDots = level.startingDots;
        goalDots = level.goalDots;
        floorTiles = level.floorTiles;
        specialTiles = level.specialTiles;
    }
}


[Serializable]
public class ListWrapper
{
    public CustomLevelData level;
    public List<DebugState> list;
    public ListWrapper(CustomLevel level, List<DebugState> list)
    {
        this.list = list;
        this.level = new CustomLevelData(level);
    }
}

[Serializable]
public class DebugState
{
    public int X;
    public int Y;
    public int Top;

    public DebugState(int x, int y, int top)
    {
        X = x;
        Y = y;
        Top = top;
    }
}
