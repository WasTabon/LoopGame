using System.Collections.Generic;
using UnityEngine;

public static class LevelValidator
{
    public struct Result
    {
        public bool ok;
        public List<string> errors;
    }

    private static readonly int[] OffX = { 0, 1, 0, -1 };
    private static readonly int[] OffY = { 1, 0, -1, 0 };

    public static Result Validate(LevelData level)
    {
        Result r = new Result { ok = true, errors = new List<string>() };

        if (level == null)
        {
            r.ok = false; r.errors.Add("Level is null"); return r;
        }
        if (!level.IsValid())
        {
            r.ok = false; r.errors.Add($"cells length {(level.cells == null ? 0 : level.cells.Length)} != {level.width * level.height}");
            return r;
        }

        int startMarkers = 0;
        foreach (CellDefinition c in level.cells)
        {
            if (c != null && c.isStart) startMarkers++;
        }
        if (startMarkers != 1)
        {
            r.ok = false; r.errors.Add($"expected exactly 1 start marker, found {startMarkers}");
        }

        for (int y = 0; y < level.height; y++)
        {
            for (int x = 0; x < level.width; x++)
            {
                CellDefinition def = level.GetCell(x, y);
                if (def == null) continue;
                if (def.cellType == CellType.Obstacle && def.pieceType != PieceType.None)
                {
                    r.ok = false; r.errors.Add($"obstacle at ({x},{y}) also has a piece");
                }
            }
        }

        int loops = CountClosedLoops(level, useStart: true);
        if (loops >= level.requiredLoops && level.requiredLoops > 0)
        {
            r.ok = false; r.errors.Add($"START state already satisfies requiredLoops ({loops} >= {level.requiredLoops}) - level pre-solved");
        }

        if (r.errors.Count > 0) r.ok = false;
        return r;
    }

    private static int CountClosedLoops(LevelData level, bool useStart)
    {
        Dictionary<int, bool[]> conn = new Dictionary<int, bool[]>();
        for (int y = 0; y < level.height; y++)
        {
            for (int x = 0; x < level.width; x++)
            {
                CellDefinition def = level.GetCell(x, y);
                if (def == null || def.pieceType == PieceType.None || def.cellType == CellType.Obstacle) continue;
                conn[Key(x, y, level.width)] = RotatedConnections(def.pieceType, def.rotationSteps);
            }
        }

        HashSet<int> seen = new HashSet<int>();
        int loopCount = 0;

        foreach (var kv in conn)
        {
            int startKey = kv.Key;
            if (seen.Contains(startKey)) continue;

            HashSet<int> component = new HashSet<int>();
            Stack<int> stack = new Stack<int>();
            stack.Push(startKey);
            bool openEnd = false;

            while (stack.Count > 0)
            {
                int cur = stack.Pop();
                if (component.Contains(cur)) continue;
                component.Add(cur);
                seen.Add(cur);

                int cx = cur % level.width;
                int cy = cur / level.width;
                bool[] c = conn[cur];

                for (int d = 0; d < 4; d++)
                {
                    if (!c[d]) continue;
                    int nx = cx + OffX[d];
                    int ny = cy + OffY[d];
                    if (nx < 0 || nx >= level.width || ny < 0 || ny >= level.height)
                    {
                        openEnd = true; continue;
                    }
                    int nKey = Key(nx, ny, level.width);
                    if (!conn.ContainsKey(nKey)) { openEnd = true; continue; }
                    if (!conn[nKey][(d + 2) % 4]) { openEnd = true; continue; }
                    if (!component.Contains(nKey)) stack.Push(nKey);
                }
            }

            if (openEnd) continue;

            bool degOk = true;
            foreach (int k in component)
            {
                if (CountTrue(conn[k]) < 2) { degOk = false; break; }
            }
            if (degOk && component.Count >= 4) loopCount++;
        }

        return loopCount;
    }

    private static bool[] RotatedConnections(PieceType type, int steps)
    {
        bool[] baseC = new bool[4];
        switch (type)
        {
            case PieceType.Straight: baseC[0] = true; baseC[2] = true; break;
            case PieceType.Corner: baseC[0] = true; baseC[1] = true; break;
            case PieceType.Triple: baseC[0] = true; baseC[1] = true; baseC[3] = true; break;
            case PieceType.Cross: baseC[0] = true; baseC[1] = true; baseC[2] = true; baseC[3] = true; break;
            case PieceType.Bridge: baseC[0] = true; baseC[1] = true; baseC[2] = true; baseC[3] = true; break;
        }
        int s = ((steps % 4) + 4) % 4;
        bool[] rot = new bool[4];
        for (int d = 0; d < 4; d++) rot[(d + s) % 4] = baseC[d];
        return rot;
    }

    private static int Key(int x, int y, int w) => y * w + x;

    private static int CountTrue(bool[] a)
    {
        int n = 0;
        for (int i = 0; i < a.Length; i++) if (a[i]) n++;
        return n;
    }
}
