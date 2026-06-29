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
        return neighborConn[(int)opposite];
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
