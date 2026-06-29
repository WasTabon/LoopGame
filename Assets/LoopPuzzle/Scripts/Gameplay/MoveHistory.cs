using System.Collections.Generic;

public enum MoveType
{
    Rotation,
    Drag
}

public class MoveRecord
{
    public MoveType type;
    public int fromX;
    public int fromY;
    public int toX;
    public int toY;
}

public class MoveHistory
{
    private readonly Stack<MoveRecord> records = new Stack<MoveRecord>();

    public int Count => records.Count;

    public void Clear()
    {
        records.Clear();
    }

    public void RecordRotation(int x, int y)
    {
        records.Push(new MoveRecord { type = MoveType.Rotation, fromX = x, fromY = y, toX = x, toY = y });
    }

    public void RecordDrag(int fromX, int fromY, int toX, int toY)
    {
        records.Push(new MoveRecord { type = MoveType.Drag, fromX = fromX, fromY = fromY, toX = toX, toY = toY });
    }

    public MoveRecord Pop()
    {
        if (records.Count == 0) return null;
        return records.Pop();
    }

    public bool HasMoves => records.Count > 0;
}
