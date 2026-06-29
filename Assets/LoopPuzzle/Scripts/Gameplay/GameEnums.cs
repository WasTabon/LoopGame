public enum CellType
{
    Empty,
    Fixed,
    Movable,
    Obstacle,
    Start,
    Portal,
    Breakable
}

public enum PieceType
{
    None,
    Straight,
    Corner,
    Triple,
    Cross,
    Bridge
}

public enum Direction
{
    North = 0,
    East = 1,
    South = 2,
    West = 3
}

public enum PieceColor
{
    Neutral = 0,
    Red = 1,
    Blue = 2,
    Green = 3,
    Yellow = 4
}
