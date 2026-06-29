using UnityEngine;

[System.Serializable]
public class CellDefinition
{
    public CellType cellType = CellType.Empty;
    public PieceType pieceType = PieceType.None;
    public int rotationSteps = 0;
    public bool isStart = false;
}

[CreateAssetMenu(fileName = "Level", menuName = "Loop Puzzle/Level Data")]
public class LevelData : ScriptableObject
{
    public int width = 4;
    public int height = 4;

    public CellDefinition[] cells;

    public int levelNumber = 1;

    public CellDefinition GetCell(int x, int y)
    {
        int index = y * width + x;
        if (index < 0 || index >= cells.Length) return null;
        return cells[index];
    }

    public bool IsValid()
    {
        return cells != null && cells.Length == width * height;
    }
}
