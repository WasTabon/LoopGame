using System.Collections.Generic;

public static class PieceConnections
{
    public static bool[] GetBaseConnections(PieceType type)
    {
        bool[] c = new bool[4];

        switch (type)
        {
            case PieceType.Straight:
                c[(int)Direction.North] = true;
                c[(int)Direction.South] = true;
                break;
            case PieceType.Corner:
                c[(int)Direction.North] = true;
                c[(int)Direction.East] = true;
                break;
            case PieceType.Triple:
                c[(int)Direction.North] = true;
                c[(int)Direction.East] = true;
                c[(int)Direction.West] = true;
                break;
            case PieceType.Cross:
                c[(int)Direction.North] = true;
                c[(int)Direction.East] = true;
                c[(int)Direction.South] = true;
                c[(int)Direction.West] = true;
                break;
        }

        return c;
    }

    public static bool[] GetRotatedConnections(PieceType type, int rotationSteps)
    {
        bool[] baseConn = GetBaseConnections(type);
        int steps = ((rotationSteps % 4) + 4) % 4;

        bool[] rotated = new bool[4];
        for (int dir = 0; dir < 4; dir++)
        {
            int rotatedDir = (dir + steps) % 4;
            rotated[rotatedDir] = baseConn[dir];
        }

        return rotated;
    }

    public static Direction Opposite(Direction dir)
    {
        return (Direction)(((int)dir + 2) % 4);
    }

    public static Vector2IntLite GetOffset(Direction dir)
    {
        switch (dir)
        {
            case Direction.North: return new Vector2IntLite(0, 1);
            case Direction.East: return new Vector2IntLite(1, 0);
            case Direction.South: return new Vector2IntLite(0, -1);
            case Direction.West: return new Vector2IntLite(-1, 0);
        }
        return new Vector2IntLite(0, 0);
    }
}

public struct Vector2IntLite
{
    public int x;
    public int y;

    public Vector2IntLite(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}
