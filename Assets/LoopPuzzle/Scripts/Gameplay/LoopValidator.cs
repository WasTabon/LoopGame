using System.Collections.Generic;
using UnityEngine;

public class LoopResult
{
    public bool isLoopClosed;
    public List<Cell> loopCells = new List<Cell>();
    public List<Cell> openEndCells = new List<Cell>();
}

public static class LoopValidator
{
    public static LoopResult Validate(GridManager grid, Cell startCell)
    {
        LoopResult result = new LoopResult();

        if (startCell == null || startCell.currentPiece == null)
        {
            return result;
        }

        HashSet<Cell> component = new HashSet<Cell>();
        Stack<Cell> stack = new Stack<Cell>();
        stack.Push(startCell);

        bool hasOpenEnd = false;

        while (stack.Count > 0)
        {
            Cell current = stack.Pop();
            if (component.Contains(current)) continue;
            component.Add(current);

            bool[] conn = current.currentPiece.GetConnections();
            for (int d = 0; d < 4; d++)
            {
                if (!conn[d]) continue;

                if (!IsMutual(grid, current, (Direction)d))
                {
                    hasOpenEnd = true;
                    if (!result.openEndCells.Contains(current))
                    {
                        result.openEndCells.Add(current);
                    }
                    continue;
                }

                Cell neighbor = GetNeighbor(grid, current, (Direction)d);
                if (neighbor != null && !component.Contains(neighbor))
                {
                    stack.Push(neighbor);
                }
            }
        }

        if (hasOpenEnd)
        {
            return result;
        }

        foreach (Cell cell in component)
        {
            int degree = CountConnections(cell.currentPiece.GetConnections());
            if (degree < 2)
            {
                return result;
            }
        }

        result.isLoopClosed = true;
        result.loopCells.AddRange(component);
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
