using UnityEditor;
using UnityEngine;

public class Iteration5Setup
{
    private const string LevelsFolder = "Assets/LoopPuzzle/Levels";

    [MenuItem("Tools/Loop Puzzle/Setup Iteration 5")]
    public static void Setup()
    {
        EnsureFolder();
        CreateLevel3();
        CreateLevel4();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Iteration 5 setup complete. World 2 levels (Level_03, Level_04) created with displaced pieces. " +
                  "To test, set GridManager.Current Level to Level_03 or Level_04 on the Game scene.");
    }

    private static void EnsureFolder()
    {
        if (!AssetDatabase.IsValidFolder(LevelsFolder))
        {
            AssetDatabase.CreateFolder("Assets/LoopPuzzle", "Levels");
        }
    }

    private static void CreateLevel3()
    {
        int w = 4, h = 4;
        LevelData level = ScriptableObject.CreateInstance<LevelData>();
        level.width = w;
        level.height = h;
        level.levelNumber = 3;
        level.cells = new CellDefinition[w * h];
        FillEmpty(level);

        // Solution loop: (1,1)(2,1)(2,2)(1,2). Piece for (2,2) starts displaced at (3,3).
        SetCell(level, 1, 1, CellType.Movable, PieceType.Corner, 2, true);
        SetCell(level, 2, 1, CellType.Movable, PieceType.Corner, 1, false);
        SetCell(level, 1, 2, CellType.Movable, PieceType.Corner, 3, false);
        // (2,2) intentionally left EMPTY - target cell for the displaced piece
        SetCell(level, 3, 3, CellType.Movable, PieceType.Corner, 0, false); // belongs at (2,2)

        SaveLevel(level, "Level_03");
    }

    private static void CreateLevel4()
    {
        int w = 5, h = 5;
        LevelData level = ScriptableObject.CreateInstance<LevelData>();
        level.width = w;
        level.height = h;
        level.levelNumber = 4;
        level.cells = new CellDefinition[w * h];
        FillEmpty(level);

        // Solution rectangle loop on the L2 footprint.
        // Two pieces displaced: (3,3) corner parked at (0,4); (2,1) straight parked at (4,0).
        SetCell(level, 1, 1, CellType.Movable, PieceType.Corner, 1, true);
        SetCell(level, 3, 1, CellType.Movable, PieceType.Corner, 0, false);
        SetCell(level, 3, 2, CellType.Movable, PieceType.Straight, 1, false);
        SetCell(level, 2, 3, CellType.Movable, PieceType.Straight, 0, false);
        SetCell(level, 1, 3, CellType.Movable, PieceType.Corner, 2, false);
        SetCell(level, 1, 2, CellType.Movable, PieceType.Straight, 1, false);
        // (3,3) and (2,1) left EMPTY - targets
        SetCell(level, 0, 4, CellType.Movable, PieceType.Corner, 0, false);   // belongs at (3,3)
        SetCell(level, 4, 0, CellType.Movable, PieceType.Straight, 0, false); // belongs at (2,1)

        SaveLevel(level, "Level_04");
    }

    private static void FillEmpty(LevelData level)
    {
        for (int i = 0; i < level.cells.Length; i++)
        {
            level.cells[i] = new CellDefinition { cellType = CellType.Empty, pieceType = PieceType.None };
        }
    }

    private static void SetCell(LevelData level, int x, int y, CellType cellType, PieceType pieceType, int rotation, bool isStart)
    {
        int index = y * level.width + x;
        level.cells[index] = new CellDefinition
        {
            cellType = cellType,
            pieceType = pieceType,
            rotationSteps = rotation,
            isStart = isStart
        };
    }

    private static void SaveLevel(LevelData level, string fileName)
    {
        string path = LevelsFolder + "/" + fileName + ".asset";
        LevelData existing = AssetDatabase.LoadAssetAtPath<LevelData>(path);
        if (existing != null)
        {
            existing.width = level.width;
            existing.height = level.height;
            existing.levelNumber = level.levelNumber;
            existing.cells = level.cells;
            EditorUtility.SetDirty(existing);
            Object.DestroyImmediate(level);
            return;
        }

        AssetDatabase.CreateAsset(level, path);
    }
}
