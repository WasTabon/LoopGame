using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class Iteration14Setup
{
    private const string LevelsFolder = "Assets/LoopPuzzle/Levels";
    private const string DataFolder = "Assets/LoopPuzzle/Data";
    private const string JsonPath = "Assets/LoopPuzzle/Editor/LevelData/levels_export.json";

    [System.Serializable]
    private class JsonCell
    {
        public int ct;
        public int pt;
        public int rot;
        public bool start;
        public int color;
        public int portalId;
        public int maxRotations;
        public bool directional;
        public int arrowDir;
    }

    [System.Serializable]
    private class JsonSolution
    {
        public int tx;
        public int ty;
        public int pt;
        public int trot;
        public int color;
    }

    [System.Serializable]
    private class JsonLevel
    {
        public string name;
        public int world;
        public int levelNumber;
        public int width;
        public int height;
        public int requiredLoops;
        public int parMoves;
        public bool colorLoopsMode;
        public bool coverAllMode;
        public bool directionalMode;
        public JsonCell[] cells;
        public JsonSolution[] solution;
    }

    [System.Serializable]
    private class JsonLevelArray { public JsonLevel[] items; }

    [MenuItem("Tools/Loop Puzzle/Setup Iteration 14")]
    public static void Setup()
    {
        if (!File.Exists(JsonPath))
        {
            Debug.LogError("levels_export.json not found at " + JsonPath);
            return;
        }

        EnsureFolders();

        string raw = File.ReadAllText(JsonPath);
        string wrapped = "{\"items\":" + raw + "}";
        JsonLevelArray arr = JsonUtility.FromJson<JsonLevelArray>(wrapped);

        List<LevelData> levels = new List<LevelData>();
        foreach (JsonLevel jl in arr.items)
        {
            LevelData level = ImportLevel(jl);
            levels.Add(level);
        }

        int failures = 0;
        foreach (LevelData lv in levels)
        {
            LevelValidator.Result res = LevelValidator.Validate(lv);
            if (!res.ok)
            {
                failures++;
                foreach (string e in res.errors) Debug.LogError($"[{lv.name}] {e}");
            }
        }
        if (failures > 0)
        {
            Debug.LogError($"Iteration 14: {failures} level(s) failed validation. Aborting database rebuild.");
            return;
        }

        BuildDatabase(levels);
        AssetDatabase.SaveAssets();

        Debug.Log($"Iteration 14 setup complete: {levels.Count} levels imported (incl. directional levels 32-34), all validated.");
    }

    private static LevelData ImportLevel(JsonLevel jl)
    {
        string path = LevelsFolder + "/" + jl.name + ".asset";
        LevelData level = AssetDatabase.LoadAssetAtPath<LevelData>(path);
        bool isNew = level == null;
        if (isNew) level = ScriptableObject.CreateInstance<LevelData>();

        level.width = jl.width;
        level.height = jl.height;
        level.levelNumber = jl.levelNumber;
        level.world = jl.world;
        level.parMoves = jl.parMoves;
        level.requiredLoops = jl.requiredLoops;
        level.colorLoopsMode = jl.colorLoopsMode;
        level.coverAllMode = jl.coverAllMode;
        level.directionalMode = jl.directionalMode;

        level.cells = new CellDefinition[jl.cells.Length];
        for (int i = 0; i < jl.cells.Length; i++)
        {
            JsonCell jc = jl.cells[i];
            level.cells[i] = new CellDefinition
            {
                cellType = (CellType)jc.ct,
                pieceType = (PieceType)jc.pt,
                rotationSteps = jc.rot,
                isStart = jc.start,
                color = (PieceColor)jc.color,
                portalId = jc.portalId,
                maxRotations = jc.maxRotations,
                directional = jc.directional,
                arrowDir = jc.arrowDir
            };
        }

        if (jl.solution != null)
        {
            level.solution = new SolutionEntry[jl.solution.Length];
            for (int i = 0; i < jl.solution.Length; i++)
            {
                JsonSolution js = jl.solution[i];
                level.solution[i] = new SolutionEntry
                {
                    tx = js.tx,
                    ty = js.ty,
                    pieceType = (PieceType)js.pt,
                    rotationSteps = js.trot,
                    color = (PieceColor)js.color
                };
            }
        }

        if (isNew)
        {
            AssetDatabase.CreateAsset(level, path);
        }
        else
        {
            EditorUtility.SetDirty(level);
        }
        return level;
    }

    private static void BuildDatabase(List<LevelData> levels)
    {
        string path = DataFolder + "/LevelDatabase.asset";
        LevelDatabase db = AssetDatabase.LoadAssetAtPath<LevelDatabase>(path);
        if (db == null)
        {
            db = ScriptableObject.CreateInstance<LevelDatabase>();
            AssetDatabase.CreateAsset(db, path);
        }
        levels.Sort((a, b) => a.levelNumber.CompareTo(b.levelNumber));
        db.levels = new List<LevelData>(levels);
        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();
    }

    private static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder(LevelsFolder)) AssetDatabase.CreateFolder("Assets/LoopPuzzle", "Levels");
        if (!AssetDatabase.IsValidFolder(DataFolder)) AssetDatabase.CreateFolder("Assets/LoopPuzzle", "Data");
    }
}
