using System.Collections.Generic;
using UnityEngine;

public static class LoopPathBuilder
{
    public static List<Cell> BuildOrderedCells(GridManager grid, Cell startCell)
    {
        List<Cell> path = new List<Cell>();
        if (startCell == null || startCell.currentPiece == null) return path;

        path.Add(startCell);
        Cell current = startCell;
        int cameFrom = -1;
        HashSet<long> visitedEdges = new HashSet<long>();

        int safety = grid.Width * grid.Height + 4;

        while (safety-- > 0)
        {
            bool[] conn = current.currentPiece.GetConnections();
            bool moved = false;

            for (int d = 0; d < 4; d++)
            {
                if (!conn[d]) continue;
                if (cameFrom != -1 && d == cameFrom) continue;

                Cell neighbor = GetNeighbor(grid, current, (Direction)d);
                if (neighbor == null || neighbor.currentPiece == null) continue;

                bool[] neighborConn = neighbor.currentPiece.GetConnections();
                Direction opposite = PieceConnections.Opposite((Direction)d);
                if (!neighborConn[(int)opposite]) continue;

                long edgeKey = EdgeKey(current, neighbor);
                if (visitedEdges.Contains(edgeKey)) continue;
                visitedEdges.Add(edgeKey);

                current = neighbor;
                cameFrom = (int)opposite;
                moved = true;
                break;
            }

            if (!moved) break;
            if (current == startCell) break;
            path.Add(current);
        }

        return path;
    }

    public static List<Vector3> BuildWorldPath(GridManager grid, Cell startCell)
    {
        List<Cell> cells = BuildOrderedCells(grid, startCell);
        List<Vector3> points = new List<Vector3>();
        foreach (Cell cell in cells)
        {
            points.Add(grid.GridToWorld(cell.gridX, cell.gridY));
        }
        return points;
    }

    private static Cell GetNeighbor(GridManager grid, Cell cell, Direction dir)
    {
        Vector2IntLite offset = PieceConnections.GetOffset(dir);
        return grid.GetCell(cell.gridX + offset.x, cell.gridY + offset.y);
    }

    private static long EdgeKey(Cell a, Cell b)
    {
        int idA = a.gridY * 1000 + a.gridX;
        int idB = b.gridY * 1000 + b.gridX;
        int lo = Mathf.Min(idA, idB);
        int hi = Mathf.Max(idA, idB);
        return (long)lo * 1000000L + hi;
    }
}
