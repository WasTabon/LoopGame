using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class Iteration6Setup
{
    private const string LevelsFolder = "Assets/LoopPuzzle/Levels";
    private const string DataFolder = "Assets/LoopPuzzle/Data";
    private const string JsonPath = "Assets/LoopPuzzle/Editor/LevelData/levels_export.json";
    private const string MenuScenePath = "Assets/LoopPuzzle/Scenes/MainMenu.unity";
    private const string GameScenePath = "Assets/LoopPuzzle/Scenes/Game.unity";
    private const string LevelSelectScenePath = "Assets/LoopPuzzle/Scenes/LevelSelect.unity";

    private static readonly Color ColorBg = new Color(0.102f, 0.102f, 0.180f, 1f);
    private static readonly Color ColorPrimary = new Color(0.290f, 0.565f, 0.886f, 1f);
    private static readonly Color ColorAccent = new Color(0.961f, 0.651f, 0.137f, 1f);
    private static readonly Color ColorPanel = new Color(0.094f, 0.094f, 0.149f, 0.92f);
    private static readonly Color ColorText = Color.white;

    [System.Serializable]
    private class JsonCell { public int ct; public int pt; public int rot; public bool start; }
    [System.Serializable]
    private class JsonLevel
    {
        public string name; public int world; public int levelNumber;
        public int width; public int height; public int requiredLoops; public int parMoves;
        public JsonCell[] cells;
    }
    [System.Serializable]
    private class JsonLevelArray { public JsonLevel[] items; }

    [MenuItem("Tools/Loop Puzzle/Setup Iteration 6")]
    public static void Setup()
    {
        EnsureFolders();
        List<LevelData> levels = ImportLevels();
        if (levels == null || levels.Count == 0)
        {
            Debug.LogError("Iteration 6: no levels imported. Aborting.");
            return;
        }

        int failures = 0;
        foreach (LevelData lv in levels)
        {
            LevelValidator.Result res = LevelValidator.Validate(lv);
            if (!res.ok)
            {
                failures++;
                foreach (string e in res.errors) Debug.LogError($"[Level {lv.name}] {e}");
            }
        }
        if (failures > 0)
        {
            Debug.LogError($"Iteration 6: {failures} level(s) failed validation. Fix before continuing.");
            return;
        }
        Debug.Log($"Iteration 6: all {levels.Count} levels passed in-Unity validation.");

        LevelDatabase db = BuildDatabase(levels);
        AddProgressManagerToMenu();
        BuildLevelSelectScene(db);
        WireGameScene(db);

        AssetDatabase.SaveAssets();
        AddScenesToBuild();

        EditorSceneManager.OpenScene(MenuScenePath);
        Debug.Log("Iteration 6 setup complete: 25 levels, database, level select, progression wired.");
    }

    private static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder(LevelsFolder)) AssetDatabase.CreateFolder("Assets/LoopPuzzle", "Levels");
        if (!AssetDatabase.IsValidFolder(DataFolder)) AssetDatabase.CreateFolder("Assets/LoopPuzzle", "Data");
    }

    private static List<LevelData> ImportLevels()
    {
        if (!File.Exists(JsonPath))
        {
            Debug.LogError("levels_export.json not found at " + JsonPath);
            return null;
        }

        string raw = File.ReadAllText(JsonPath);
        string wrapped = "{\"items\":" + raw + "}";
        JsonLevelArray arr = JsonUtility.FromJson<JsonLevelArray>(wrapped);

        List<LevelData> result = new List<LevelData>();
        foreach (JsonLevel jl in arr.items)
        {
            LevelData level = ScriptableObject.CreateInstance<LevelData>();
            level.width = jl.width;
            level.height = jl.height;
            level.levelNumber = jl.levelNumber;
            level.world = jl.world;
            level.parMoves = jl.parMoves;
            level.requiredLoops = jl.requiredLoops;
            level.cells = new CellDefinition[jl.cells.Length];
            for (int i = 0; i < jl.cells.Length; i++)
            {
                JsonCell jc = jl.cells[i];
                level.cells[i] = new CellDefinition
                {
                    cellType = (CellType)jc.ct,
                    pieceType = (PieceType)jc.pt,
                    rotationSteps = jc.rot,
                    isStart = jc.start
                };
            }

            string path = LevelsFolder + "/" + jl.name + ".asset";
            LevelData existing = AssetDatabase.LoadAssetAtPath<LevelData>(path);
            if (existing != null)
            {
                existing.width = level.width; existing.height = level.height;
                existing.levelNumber = level.levelNumber; existing.world = level.world;
                existing.parMoves = level.parMoves; existing.requiredLoops = level.requiredLoops;
                existing.cells = level.cells;
                EditorUtility.SetDirty(existing);
                Object.DestroyImmediate(level);
                result.Add(existing);
            }
            else
            {
                AssetDatabase.CreateAsset(level, path);
                result.Add(level);
            }
        }
        AssetDatabase.SaveAssets();
        return result;
    }

    private static LevelDatabase BuildDatabase(List<LevelData> levels)
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
        return db;
    }

    private static void AddProgressManagerToMenu()
    {
        Scene scene = EditorSceneManager.OpenScene(MenuScenePath);
        if (Object.FindObjectOfType<ProgressManager>() == null)
        {
            GameObject go = new GameObject("ProgressManager");
            go.AddComponent<ProgressManager>();
        }

        MainMenuController menu = Object.FindObjectOfType<MainMenuController>();
        if (menu != null)
        {
            menu.gameSceneName = "Game";
            menu.levelSelectSceneName = "LevelSelect";
            EditorUtility.SetDirty(menu);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void BuildLevelSelectScene(LevelDatabase db)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject camGo = new GameObject("Main Camera");
        Camera cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = ColorBg;
        cam.orthographic = true;
        cam.tag = "MainCamera";
        camGo.transform.position = new Vector3(0, 0, -10);

        GameObject es = new GameObject("EventSystem");
        es.AddComponent<UnityEngine.EventSystems.EventSystem>();
        es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        GameObject canvasGo = new GameObject("LevelSelectCanvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        GameObject safeArea = new GameObject("SafeArea");
        safeArea.transform.SetParent(canvasGo.transform, false);
        RectTransform saRt = safeArea.AddComponent<RectTransform>();
        saRt.anchorMin = Vector2.zero; saRt.anchorMax = Vector2.one;
        saRt.offsetMin = Vector2.zero; saRt.offsetMax = Vector2.zero;
        safeArea.AddComponent<SafeAreaFitter>();

        GameObject titleGo = new GameObject("Title");
        titleGo.transform.SetParent(safeArea.transform, false);
        TextMeshProUGUI title = titleGo.AddComponent<TextMeshProUGUI>();
        title.text = "SELECT LEVEL";
        title.fontSize = 60; title.color = ColorText; title.fontStyle = FontStyles.Bold;
        title.alignment = TextAlignmentOptions.Center;
        RectTransform titleRt = title.rectTransform;
        titleRt.anchorMin = new Vector2(0, 1); titleRt.anchorMax = new Vector2(1, 1);
        titleRt.pivot = new Vector2(0.5f, 1f);
        titleRt.sizeDelta = new Vector2(0, 110);
        titleRt.anchoredPosition = new Vector2(0, -20);

        GameObject scrollGo = new GameObject("ScrollView");
        scrollGo.transform.SetParent(safeArea.transform, false);
        RectTransform scrollRt = scrollGo.AddComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0, 0); scrollRt.anchorMax = new Vector2(1, 1);
        scrollRt.offsetMin = new Vector2(0, 150);
        scrollRt.offsetMax = new Vector2(0, -140);
        Image scrollBg = scrollGo.AddComponent<Image>();
        scrollBg.color = new Color(0, 0, 0, 0.001f);
        ScrollRect scroll = scrollGo.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        RectMask2D mask = scrollGo.AddComponent<RectMask2D>();

        GameObject content = new GameObject("Content");
        content.transform.SetParent(scrollGo.transform, false);
        RectTransform contentRt = content.AddComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0, 1); contentRt.anchorMax = new Vector2(1, 1);
        contentRt.pivot = new Vector2(0.5f, 1f);
        contentRt.anchoredPosition = Vector2.zero;
        VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
        vlg.childControlWidth = true; vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;
        vlg.spacing = 10;
        ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scroll.content = contentRt;

        GameObject backGo = new GameObject("BackButton");
        backGo.transform.SetParent(safeArea.transform, false);
        Image backImg = backGo.AddComponent<Image>();
        backImg.color = ColorPrimary;
        backImg.sprite = GetUISprite();
        backImg.type = Image.Type.Sliced;
        RectTransform backRt = backImg.rectTransform;
        backRt.anchorMin = new Vector2(0.5f, 0); backRt.anchorMax = new Vector2(0.5f, 0);
        backRt.pivot = new Vector2(0.5f, 0);
        backRt.sizeDelta = new Vector2(400, 120);
        backRt.anchoredPosition = new Vector2(0, 20);
        Button backBtn = backGo.AddComponent<Button>();
        backGo.AddComponent<ButtonPunch>();
        GameObject backLabelGo = new GameObject("Label");
        backLabelGo.transform.SetParent(backGo.transform, false);
        TextMeshProUGUI backLabel = backLabelGo.AddComponent<TextMeshProUGUI>();
        backLabel.text = "BACK"; backLabel.fontSize = 44; backLabel.color = ColorText;
        backLabel.alignment = TextAlignmentOptions.Center; backLabel.fontStyle = FontStyles.Bold;
        RectTransform blRt = backLabel.rectTransform;
        blRt.anchorMin = Vector2.zero; blRt.anchorMax = Vector2.one;
        blRt.offsetMin = Vector2.zero; blRt.offsetMax = Vector2.zero;

        LevelSelectController controller = canvasGo.AddComponent<LevelSelectController>();
        controller.database = db;
        controller.contentParent = contentRt;
        controller.backButton = backBtn;
        controller.buttonSprite = GetUISprite();
        controller.gameSceneName = "Game";
        controller.menuSceneName = "MainMenu";

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, LevelSelectScenePath);
    }

    private static void WireGameScene(LevelDatabase db)
    {
        Scene scene = EditorSceneManager.OpenScene(GameScenePath);

        LevelController controller = Object.FindObjectOfType<LevelController>();
        if (controller == null)
        {
            Debug.LogError("LevelController missing on Game scene. Run earlier setups first.");
            return;
        }
        controller.database = db;
        controller.menuSceneName = "MainMenu";

        WinPopup winPopup = Object.FindObjectOfType<WinPopup>();
        if (winPopup != null)
        {
            EnsureWinPopupExtras(winPopup);
        }

        EditorUtility.SetDirty(controller);
        if (winPopup != null) EditorUtility.SetDirty(winPopup);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void EnsureWinPopupExtras(WinPopup winPopup)
    {
        Transform window = winPopup.popup.window;

        if (winPopup.starDisplay == null)
        {
            Transform oldStars = window.Find("StarsPlaceholder");
            if (oldStars != null) Object.DestroyImmediate(oldStars.gameObject);

            GameObject starsGo = new GameObject("StarRow");
            starsGo.transform.SetParent(window, false);
            RectTransform starsRt = starsGo.AddComponent<RectTransform>();
            starsRt.anchorMin = new Vector2(0.5f, 0.6f);
            starsRt.anchorMax = new Vector2(0.5f, 0.6f);
            starsRt.pivot = new Vector2(0.5f, 0.5f);
            starsRt.sizeDelta = new Vector2(420, 130);
            HorizontalLayoutGroup hlg = starsGo.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.spacing = 20;
            hlg.childControlWidth = false; hlg.childControlHeight = false;

            StarDisplay sd = starsGo.AddComponent<StarDisplay>();
            sd.stars = new TextMeshProUGUI[3];
            for (int i = 0; i < 3; i++)
            {
                GameObject sGo = new GameObject("Star" + i);
                sGo.transform.SetParent(starsGo.transform, false);
                TextMeshProUGUI sTmp = sGo.AddComponent<TextMeshProUGUI>();
                sTmp.text = "*"; sTmp.fontSize = 90; sTmp.alignment = TextAlignmentOptions.Center;
                sTmp.fontStyle = FontStyles.Bold;
                RectTransform sRt = sTmp.rectTransform;
                sRt.sizeDelta = new Vector2(110, 110);
                sd.stars[i] = sTmp;
            }
            winPopup.starDisplay = sd;
        }

        if (winPopup.homeButton == null)
        {
            GameObject homeGo = new GameObject("HomeButton");
            homeGo.transform.SetParent(window, false);
            Image img = homeGo.AddComponent<Image>();
            img.color = new Color(0.4f, 0.4f, 0.48f, 1f);
            img.sprite = GetUISprite();
            img.type = Image.Type.Sliced;
            RectTransform rt = img.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 0.02f);
            rt.anchorMax = new Vector2(0.5f, 0.02f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.sizeDelta = new Vector2(560, 120);
            rt.anchoredPosition = new Vector2(0, 10);
            Button btn = homeGo.AddComponent<Button>();
            homeGo.AddComponent<ButtonPunch>();
            GameObject lblGo = new GameObject("Label");
            lblGo.transform.SetParent(homeGo.transform, false);
            TextMeshProUGUI lbl = lblGo.AddComponent<TextMeshProUGUI>();
            lbl.text = "LEVELS"; lbl.fontSize = 44; lbl.color = ColorText;
            lbl.alignment = TextAlignmentOptions.Center; lbl.fontStyle = FontStyles.Bold;
            RectTransform lblRt = lbl.rectTransform;
            lblRt.anchorMin = Vector2.zero; lblRt.anchorMax = Vector2.one;
            lblRt.offsetMin = Vector2.zero; lblRt.offsetMax = Vector2.zero;
            winPopup.homeButton = btn;
        }
    }

    private static void AddScenesToBuild()
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>
        {
            new EditorBuildSettingsScene(MenuScenePath, true),
            new EditorBuildSettingsScene(LevelSelectScenePath, true),
            new EditorBuildSettingsScene(GameScenePath, true)
        };
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    private static Sprite cachedUISprite;
    private static Sprite GetUISprite()
    {
        if (cachedUISprite != null) return cachedUISprite;
        cachedUISprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        return cachedUISprite;
    }
}
