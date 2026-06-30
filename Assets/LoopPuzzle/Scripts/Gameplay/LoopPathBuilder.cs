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

            int preferredDir = -1;
            if (current.currentPiece.pieceType == PieceType.Bridge && cameFrom != -1)
            {
                preferredDir = (int)PieceConnections.Opposite((Direction)cameFrom);
            }

            for (int attempt = 0; attempt < 4 && !moved; attempt++)
            {
                int d = preferredDir >= 0 && attempt == 0 ? preferredDir : attempt;
                if (preferredDir >= 0 && attempt > 0 && d == preferredDir) continue;
                if (!conn[d]) continue;
                if (cameFrom != -1 && d == cameFrom) continue;

                Cell neighbor = GetNeighbor(grid, current, (Direction)d);
                if (neighbor == null) continue;
                Direction opposite = PieceConnections.Opposite((Direction)d);

                if (neighbor.IsPortal)
                {
                    if (neighbor.portalDir != (int)opposite) continue;
                    Cell pair = grid.GetPortalPair(neighbor);
                    if (pair == null) continue;

                    long portalEdge = EdgeKey(current, neighbor);
                    if (visitedEdges.Contains(portalEdge)) continue;
                    visitedEdges.Add(portalEdge);

                    Cell pairNeighbor = GetNeighbor(grid, pair, (Direction)pair.portalDir);
                    if (pairNeighbor == null || pairNeighbor.currentPiece == null) continue;

                    path.Add(neighbor);
                    path.Add(pair);
                    path.Add(pairNeighbor);

                    current = pairNeighbor;
                    cameFrom = (int)PieceConnections.Opposite((Direction)pair.portalDir);
                    moved = true;
                    continue;
                }

                if (neighbor.currentPiece == null) continue;

                bool[] neighborConn = neighbor.currentPiece.GetConnections();
                if (!neighborConn[(int)opposite]) continue;

                long edgeKey = EdgeKey(current, neighbor);
                if (visitedEdges.Contains(edgeKey)) continue;
                visitedEdges.Add(edgeKey);

                current = neighbor;
                cameFrom = (int)opposite;
                moved = true;
            }

            if (!moved) break;
            if (current == startCell) break;
            if (current.currentPiece != null) path.Add(current);
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
