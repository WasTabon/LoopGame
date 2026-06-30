using UnityEngine;

[System.Serializable]
public class CellDefinition
{
    public CellType cellType = CellType.Empty;
    public PieceType pieceType = PieceType.None;
    public int rotationSteps = 0;
    public bool isStart = false;

    public PieceColor color = PieceColor.Neutral;
    public int portalId = 0;
    public int portalDir = 0;
    public int maxRotations = 0;
    public bool directional = false;
    public int arrowDir = 0;
}

[System.Serializable]
public class SolutionEntry
{
    public int tx;
    public int ty;
    public PieceType pieceType = PieceType.None;
    public int rotationSteps = 0;
    public PieceColor color = PieceColor.Neutral;
}

[CreateAssetMenu(fileName = "Level", menuName = "Loop Puzzle/Level Data")]
public class LevelData : ScriptableObject
{
    public int width = 4;
    public int height = 4;

    public CellDefinition[] cells;

    public int levelNumber = 1;
    public int world = 1;
    public int parMoves = 10;
    public int requiredLoops = 1;

    public bool colorLoopsMode = false;
    public bool coverAllMode = false;
    public bool directionalMode = false;
    public bool portalsMode = false;

    public SolutionEntry[] solution;

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
