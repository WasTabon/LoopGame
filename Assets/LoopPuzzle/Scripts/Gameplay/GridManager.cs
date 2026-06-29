using UnityEngine;

public class GridManager : MonoBehaviour
{
    public LevelData currentLevel;
    public float screenFillFraction = 0.86f;
    public float verticalOffset = 0f;
    public bool buildOnStart = true;

    private Cell[,] cells;
    private float cellWorldSize;
    private int width;
    private int height;
    private Vector3 originPosition;
    private Cell startCell;

    private Transform cellContainer;
    private Transform pieceContainer;

    public float CellWorldSize => cellWorldSize;
    public int Width => width;
    public int Height => height;
    public Cell StartCell => startCell;

    private void Start()
    {
        if (!buildOnStart) return;
        Debug.Assert(currentLevel != null, "GridManager has no level assigned!");
        BuildLevel(currentLevel);
    }

    public void BuildLevel(LevelData level)
    {
        if (!level.IsValid())
        {
            Debug.LogError("Level data invalid: cell count does not match width*height for " + level.name);
            return;
        }

        ClearExisting();

        width = level.width;
        height = level.height;
        cells = new Cell[width, height];
        startCell = null;

        ComputeLayout();
        CreateContainers();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                CellDefinition def = level.GetCell(x, y);
                CreateCell(x, y, def);
            }
        }

        if (startCell == null)
        {
            Debug.LogWarning("Level " + level.name + " has no start cell flagged. Loop check will not run.");
        }
    }

    private void ComputeLayout()
    {
        Camera cam = Camera.main;
        float worldHeight = cam.orthographicSize * 2f;
        float worldWidth = worldHeight * cam.aspect;

        float available = Mathf.Min(worldWidth, worldHeight) * screenFillFraction;
        int maxDimension = Mathf.Max(width, height);
        cellWorldSize = available / maxDimension;

        float totalWidth = cellWorldSize * width;
        float totalHeight = cellWorldSize * height;

        originPosition = new Vector3(
            -totalWidth * 0.5f + cellWorldSize * 0.5f,
            -totalHeight * 0.5f + cellWorldSize * 0.5f + verticalOffset,
            0f);
    }

    private void CreateContainers()
    {
        GameObject cellGo = new GameObject("Cells");
        cellGo.transform.SetParent(transform, false);
        cellContainer = cellGo.transform;

        GameObject pieceGo = new GameObject("Pieces");
        pieceGo.transform.SetParent(transform, false);
        pieceContainer = pieceGo.transform;
    }

    private void CreateCell(int x, int y, CellDefinition def)
    {
        Vector3 pos = GridToWorld(x, y);

        GameObject cellGo = new GameObject("Cell_" + x + "_" + y);
        cellGo.transform.SetParent(cellContainer, false);
        cellGo.transform.position = pos;

        Cell cell = cellGo.AddComponent<Cell>();
        cell.Init(x, y, def.cellType, def.isStart, cellWorldSize);
        cells[x, y] = cell;

        if (def.isStart)
        {
            startCell = cell;
        }

        if (def.pieceType != PieceType.None && def.cellType != CellType.Obstacle)
        {
            CreatePiece(cell, def, pos);
        }
    }

    private void CreatePiece(Cell cell, CellDefinition def, Vector3 pos)
    {
        GameObject pieceGo = new GameObject("Piece_" + cell.gridX + "_" + cell.gridY);
        pieceGo.transform.SetParent(pieceContainer, false);
        pieceGo.transform.position = pos;

        bool rotatable = def.cellType == CellType.Movable;

        PathPiece piece = pieceGo.AddComponent<PathPiece>();
        piece.Init(def.pieceType, def.rotationSteps, rotatable);
        piece.SetScale(cellWorldSize * 0.92f);

        cell.SetPiece(piece);
    }

    public Vector3 GridToWorld(int x, int y)
    {
        return originPosition + new Vector3(x * cellWorldSize, y * cellWorldSize, 0f);
    }

    public Cell GetCell(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height) return null;
        return cells[x, y];
    }

    private void ClearExisting()
    {
        if (cellContainer != null) Destroy(cellContainer.gameObject);
        if (pieceContainer != null) Destroy(pieceContainer.gameObject);
    }
}
