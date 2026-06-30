using System.Collections.Generic;
using UnityEngine;

public class LoopResult
{
    public bool isLoopClosed;
    public int closedLoopCount;
    public List<Cell> loopCells = new List<Cell>();
    public List<Cell> openEndCells = new List<Cell>();
}

public static class LoopValidator
{
    public static LoopResult Validate(GridManager grid, Cell startCell)
    {
        return Validate(grid, startCell, 1);
    }

    public static LoopResult Validate(GridManager grid, Cell startCell, int requiredLoops)
    {
        LoopResult result = ValidateAllLoops(grid);
        result.isLoopClosed = result.closedLoopCount >= requiredLoops;

        if (startCell != null && startCell.currentPiece != null && !result.isLoopClosed)
        {
        }
        return result;
    }

    public static bool ValidateDirectional(GridManager grid)
    {
        List<Cell> directionalCells = new List<Cell>();
        grid.ForEachCell(cell =>
        {
            if (cell.currentPiece != null && cell.currentPiece.directional)
            {
                directionalCells.Add(cell);
            }
        });

        if (directionalCells.Count == 0)
        {
            return ValidateAllLoops(grid).closedLoopCount >= 1;
        }

        LoopResult all = ValidateAllLoops(grid);
        if (all.closedLoopCount == 0) return false;

        Dictionary<Cell, int> exitDir = TraceExitDirections(grid);
        if (exitDir == null) return false;

        bool orientationA = true;
        bool orientationB = true;
        foreach (Cell dc in directionalCells)
        {
            if (!exitDir.ContainsKey(dc)) return false;
            int traced = exitDir[dc];
            int arrow = dc.currentPiece.CurrentArrowDir();
            if (arrow != traced) orientationA = false;
            if (arrow != (int)PieceConnections.Opposite((Direction)traced)) orientationB = false;
        }
        return orientationA || orientationB;
    }

    private static Dictionary<Cell, int> TraceExitDirections(GridManager grid)
    {
        Dictionary<Cell, int> result = new Dictionary<Cell, int>();
        Cell start = null;
        grid.ForEachCell(cell =>
        {
            if (start == null && cell.currentPiece != null) start = cell;
        });
        if (start == null) return null;

        bool[] startConn = start.currentPiece.GetConnections();
        int firstDir = -1;
        for (int d = 0; d < 4; d++)
        {
            if (startConn[d]) { firstDir = d; break; }
        }
        if (firstDir == -1) return null;

        Cell cell = start;
        int curOut = firstDir;
        int guard = 0;
        while (guard++ < 10000)
        {
            result[cell] = curOut;
            Cell next = GetNeighbor(grid, cell, (Direction)curOut);
            if (next == null || next.currentPiece == null) return null;

            int enter = (int)PieceConnections.Opposite((Direction)curOut);
            bool[] nc = next.currentPiece.GetConnections();
            int exit = -1;
            int exitCount = 0;
            for (int d = 0; d < 4; d++)
            {
                if (nc[d] && d != enter) { exit = d; exitCount++; }
            }
            if (exitCount != 1) return null;

            cell = next;
            curOut = exit;
            if (cell == start && curOut == firstDir) break;
        }
        return result;
    }

    public static bool ValidateColorLoops(GridManager grid)
    {
        HashSet<PieceColor> colorsPresent = new HashSet<PieceColor>();
        int totalPieces = 0;
        grid.ForEachCell(cell =>
        {
            if (cell.currentPiece != null)
            {
                totalPieces++;
                if (cell.currentPiece.pieceColor != PieceColor.Neutral)
                {
                    colorsPresent.Add(cell.currentPiece.pieceColor);
                }
            }
        });

        if (colorsPresent.Count == 0)
        {
            return ValidateAllLoops(grid).closedLoopCount >= 1;
        }

        LoopResult all = ValidateAllLoops(grid);
        if (all.closedLoopCount == 0) return false;
        if (all.openEndCells.Count > 0) return false;
        if (all.loopCells.Count != totalPieces) return false;

        HashSet<PieceColor> colorsWithLoop = new HashSet<PieceColor>();
        foreach (Cell c in all.loopCells)
        {
            if (c.currentPiece != null)
            {
                colorsWithLoop.Add(c.currentPiece.pieceColor);
            }
        }

        foreach (PieceColor needed in colorsPresent)
        {
            if (!colorsWithLoop.Contains(needed)) return false;
        }
        return true;
    }

    public static LoopResult ValidateAllLoops(GridManager grid)
    {
        LoopResult result = new LoopResult();
        HashSet<Cell> globalSeen = new HashSet<Cell>();

        List<Cell> allPieceCells = new List<Cell>();
        grid.ForEachCell(cell =>
        {
            if (cell.currentPiece != null) allPieceCells.Add(cell);
        });

        foreach (Cell seed in allPieceCells)
        {
            if (globalSeen.Contains(seed)) continue;

            HashSet<Cell> component = new HashSet<Cell>();
            Stack<Cell> stack = new Stack<Cell>();
            stack.Push(seed);
            bool componentHasOpenEnd = false;

            while (stack.Count > 0)
            {
                Cell current = stack.Pop();
                if (component.Contains(current)) continue;
                component.Add(current);
                globalSeen.Add(current);

                bool[] conn = current.currentPiece.GetConnections();
                for (int d = 0; d < 4; d++)
                {
                    if (!conn[d]) continue;
                    if (!IsMutual(grid, current, (Direction)d))
                    {
                        componentHasOpenEnd = true;
                        if (!result.openEndCells.Contains(current)) result.openEndCells.Add(current);
                        continue;
                    }
                    Cell neighbor = GetNeighbor(grid, current, (Direction)d);
                    if (neighbor != null && !component.Contains(neighbor))
                    {
                        stack.Push(neighbor);
                    }
                }
            }

            if (componentHasOpenEnd) continue;

            bool allDegreeOk = true;
            foreach (Cell cell in component)
            {
                if (CountConnections(cell.currentPiece.GetConnections()) < 2)
                {
                    allDegreeOk = false;
                    break;
                }
            }

            if (allDegreeOk && component.Count >= 4)
            {
                result.closedLoopCount++;
                result.loopCells.AddRange(component);
            }
        }

        return result;
    }

    private static bool IsMutual(GridManager grid, Cell cell, Direction dir)
    {
        bool[] conn = cell.currentPiece.GetConnections();
        if (!conn[(int)dir]) return false;

        Cell neighbor = GetNeighbor(grid, cell, dir);
        if (neighbor == null || neighbor.currentPiece == null) return false;

        bool[] neighborConn = neighbor.currentPiece.GetConnections();
        Direction opposite = PieceConnections.Opposite(dir);
        if (!neighborConn[(int)opposite]) return false;

        return ColorsCompatible(cell.currentPiece.pieceColor, neighbor.currentPiece.pieceColor);
    }

    private static bool ColorsCompatible(PieceColor a, PieceColor b)
    {
        if (a == PieceColor.Neutral || b == PieceColor.Neutral) return true;
        return a == b;
    }

    private static Cell GetNeighbor(GridManager grid, Cell cell, Direction dir)
    {
        Vector2IntLite offset = PieceConnections.GetOffset(dir);
        return grid.GetCell(cell.gridX + offset.x, cell.gridY + offset.y);
    }

    private static int CountConnections(bool[] conn)
    {
        int count = 0;
        for (int i = 0; i < 4; i++)
        {
            if (conn[i]) count++;
        }
        return count;
    }
}
