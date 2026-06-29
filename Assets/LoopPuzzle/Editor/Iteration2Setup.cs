using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Iteration2Setup
{
    private const string LevelsFolder = "Assets/LoopPuzzle/Levels";
    private const string GameScenePath = "Assets/LoopPuzzle/Scenes/Game.unity";

    [MenuItem("Tools/Loop Puzzle/Setup Iteration 2")]
    public static void Setup()
    {
        EnsureFolder();
        LevelData level1 = CreateLevel1();
        LevelData level2 = CreateLevel2();
        AugmentGameScene(level1);

        Debug.Log("Iteration 2 setup complete. Levels created, Game scene has a grid.");
    }

    private static void EnsureFolder()
    {
        if (!AssetDatabase.IsValidFolder(LevelsFolder))
        {
            AssetDatabase.CreateFolder("Assets/LoopPuzzle", "Levels");
        }
    }

    private static LevelData CreateLevel1()
    {
        int w = 4, h = 4;
        LevelData level = ScriptableObject.CreateInstance<LevelData>();
        level.width = w;
        level.height = h;
        level.levelNumber = 1;
        level.cells = new CellDefinition[w * h];

        for (int i = 0; i < level.cells.Length; i++)
        {
            level.cells[i] = new CellDefinition { cellType = CellType.Empty, pieceType = PieceType.None };
        }

        SetCell(level, 1, 1, CellType.Movable, PieceType.Corner, 2, true);
        SetCell(level, 2, 1, CellType.Movable, PieceType.Corner, 1, false);
        SetCell(level, 2, 2, CellType.Movable, PieceType.Corner, 0, false);
        SetCell(level, 1, 2, CellType.Movable, PieceType.Corner, 3, false);

        SaveLevel(level, "Level_01");
        return level;
    }

    private static LevelData CreateLevel2()
    {
        int w = 5, h = 5;
        LevelData level = ScriptableObject.CreateInstance<LevelData>();
        level.width = w;
        level.height = h;
        level.levelNumber = 2;
        level.cells = new CellDefinition[w * h];

        for (int i = 0; i < level.cells.Length; i++)
        {
            level.cells[i] = new CellDefinition { cellType = CellType.Empty, pieceType = PieceType.None };
        }

        SetCell(level, 1, 1, CellType.Movable, PieceType.Corner, 2, true);
        SetCell(level, 2, 1, CellType.Movable, PieceType.Straight, 0, false);
        SetCell(level, 3, 1, CellType.Movable, PieceType.Corner, 1, false);
        SetCell(level, 3, 2, CellType.Movable, PieceType.Straight, 1, false);
        SetCell(level, 3, 3, CellType.Movable, PieceType.Corner, 0, false);
        SetCell(level, 2, 3, CellType.Movable, PieceType.Straight, 3, false);
        SetCell(level, 1, 3, CellType.Movable, PieceType.Corner, 3, false);
        SetCell(level, 1, 2, CellType.Movable, PieceType.Straight, 1, false);

        SaveLevel(level, "Level_02");
        return level;
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
            AssetDatabase.SaveAssets();
            Object.DestroyImmediate(level);
            return;
        }

        AssetDatabase.CreateAsset(level, path);
        AssetDatabase.SaveAssets();
    }

    private static void AugmentGameScene(LevelData firstLevel)
    {
        Scene scene = EditorSceneManager.OpenScene(GameScenePath);

        GridManager grid = Object.FindObjectOfType<GridManager>();
        if (grid == null)
        {
            GameObject gridGo = new GameObject("GridManager");
            grid = gridGo.AddComponent<GridManager>();
        }
        grid.currentLevel = firstLevel;

        PieceInput input = Object.FindObjectOfType<PieceInput>();
        if (input == null)
        {
            GameObject inputGo = new GameObject("PieceInput");
            input = inputGo.AddComponent<PieceInput>();
        }
        input.gridManager = grid;

        EditorUtility.SetDirty(grid);
        EditorUtility.SetDirty(input);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }
}
