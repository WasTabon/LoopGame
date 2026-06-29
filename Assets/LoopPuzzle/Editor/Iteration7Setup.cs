using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class Iteration7Setup
{
    private const string LevelsFolder = "Assets/LoopPuzzle/Levels";
    private const string DataFolder = "Assets/LoopPuzzle/Data";
    private const string JsonPath = "Assets/LoopPuzzle/Editor/LevelData/levels_export.json";
    private const string MenuScenePath = "Assets/LoopPuzzle/Scenes/MainMenu.unity";
    private const string GameScenePath = "Assets/LoopPuzzle/Scenes/Game.unity";

    private static readonly Color ColorPrimary = new Color(0.290f, 0.565f, 0.886f, 1f);
    private static readonly Color ColorAccent = new Color(0.961f, 0.651f, 0.137f, 1f);
    private static readonly Color ColorPanel = new Color(0.16f, 0.16f, 0.24f, 1f);
    private static readonly Color ColorText = Color.white;
    private static readonly Color ColorDanger = new Color(0.78f, 0.30f, 0.30f, 1f);

    [System.Serializable]
    private class JsonCell { public int ct; public int pt; public int rot; public bool start; }
    [System.Serializable]
    private class JsonSolution { public int tx; public int ty; public int pt; public int trot; }
    [System.Serializable]
    private class JsonLevel
    {
        public string name; public int world; public int levelNumber;
        public int width; public int height; public int requiredLoops; public int parMoves;
        public JsonCell[] cells; public JsonSolution[] solution;
    }
    [System.Serializable]
    private class JsonLevelArray { public JsonLevel[] items; }

    [MenuItem("Tools/Loop Puzzle/Setup Iteration 7")]
    public static void Setup()
    {
        ReimportSolutions();
        WireGameSceneHud();
        BuildSettingsPopup();

        AssetDatabase.SaveAssets();
        Debug.Log("Iteration 7 setup complete: solutions imported, HUD buttons, hint, settings wired.");
    }

    private static void ReimportSolutions()
    {
        if (!File.Exists(JsonPath))
        {
            Debug.LogError("levels_export.json not found at " + JsonPath);
            return;
        }

        string raw = File.ReadAllText(JsonPath);
        string wrapped = "{\"items\":" + raw + "}";
        JsonLevelArray arr = JsonUtility.FromJson<JsonLevelArray>(wrapped);

        int updated = 0;
        foreach (JsonLevel jl in arr.items)
        {
            string path = LevelsFolder + "/" + jl.name + ".asset";
            LevelData level = AssetDatabase.LoadAssetAtPath<LevelData>(path);
            if (level == null)
            {
                Debug.LogWarning("Level asset missing (run Setup Iteration 6 first): " + path);
                continue;
            }

            if (jl.solution != null)
            {
                level.solution = new SolutionEntry[jl.solution.Length];
                for (int i = 0; i < jl.solution.Length; i++)
                {
                    JsonSolution js = jl.solution[i];
                    level.solution[i] = new SolutionEntry
                    {
                        tx = js.tx, ty = js.ty,
                        pieceType = (PieceType)js.pt,
                        rotationSteps = js.trot
                    };
                }
                EditorUtility.SetDirty(level);
                updated++;
            }
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"Iteration 7: solution data imported into {updated} levels.");
    }

    private static void WireGameSceneHud()
    {
        Scene scene = EditorSceneManager.OpenScene(GameScenePath);

        LevelController controller = Object.FindObjectOfType<LevelController>();
        HUDController hud = Object.FindObjectOfType<HUDController>();
        GridManager grid = Object.FindObjectOfType<GridManager>();
        if (controller == null || hud == null || grid == null)
        {
            Debug.LogError("Game scene missing LevelController/HUD/Grid. Run earlier setups first.");
            return;
        }

        HintSystem hint = Object.FindObjectOfType<HintSystem>();
        if (hint == null)
        {
            GameObject hintGo = new GameObject("HintSystem");
            hint = hintGo.AddComponent<HintSystem>();
        }
        hint.gridManager = grid;
        controller.hintSystem = hint;

        Canvas canvas = FindHudCanvas();
        if (canvas == null)
        {
            Debug.LogError("No canvas found in Game scene for HUD buttons.");
            return;
        }

        Transform buttonRow = canvas.transform.Find("ActionButtonRow");
        if (buttonRow == null)
        {
            buttonRow = CreateButtonRow(canvas.transform);
        }

        Button undo = EnsureActionButton(buttonRow, "UndoButton", "UNDO", 0);
        Button restart = EnsureActionButton(buttonRow, "RestartButton", "RESTART", 1);
        Button hintBtn = EnsureActionButton(buttonRow, "HintButton", "HINT", 2);

        hud.undoButton = undo;
        hud.restartButton = restart;
        hud.hintButton = hintBtn;
        hud.levelController = controller;

        EditorUtility.SetDirty(hud);
        EditorUtility.SetDirty(controller);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static Canvas FindHudCanvas()
    {
        Canvas[] canvases = Object.FindObjectsOfType<Canvas>();
        foreach (Canvas c in canvases)
        {
            if (c.renderMode == RenderMode.ScreenSpaceOverlay) return c;
        }
        return canvases.Length > 0 ? canvases[0] : null;
    }

    private static Transform CreateButtonRow(Transform canvas)
    {
        GameObject row = new GameObject("ActionButtonRow");
        row.transform.SetParent(canvas, false);
        RectTransform rt = row.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.sizeDelta = new Vector2(900, 130);
        rt.anchoredPosition = new Vector2(0, 40);

        HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 30;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;
        hlg.childForceExpandWidth = false;

        return row.transform;
    }

    private static Button EnsureActionButton(Transform parent, string name, string label, int index)
    {
        Transform existing = parent.Find(name);
        if (existing != null)
        {
            return existing.GetComponent<Button>();
        }

        GameObject btnGo = new GameObject(name);
        btnGo.transform.SetParent(parent, false);

        RectTransform rt = btnGo.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(270, 120);

        LayoutElement le = btnGo.AddComponent<LayoutElement>();
        le.preferredWidth = 270;
        le.preferredHeight = 120;

        Image img = btnGo.AddComponent<Image>();
        img.color = name == "HintButton" ? ColorAccent : ColorPrimary;
        img.sprite = GetUISprite();
        img.type = Image.Type.Sliced;

        Button btn = btnGo.AddComponent<Button>();
        btnGo.AddComponent<ButtonPunch>();

        GameObject lblGo = new GameObject("Label");
        lblGo.transform.SetParent(btnGo.transform, false);
        TextMeshProUGUI tmp = lblGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 38;
        tmp.color = ColorText;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        RectTransform lblRt = tmp.rectTransform;
        lblRt.anchorMin = Vector2.zero; lblRt.anchorMax = Vector2.one;
        lblRt.offsetMin = Vector2.zero; lblRt.offsetMax = Vector2.zero;

        return btn;
    }

    private static void BuildSettingsPopup()
    {
        Scene scene = EditorSceneManager.OpenScene(MenuScenePath);

        LevelDatabase db = AssetDatabase.LoadAssetAtPath<LevelDatabase>(DataFolder + "/LevelDatabase.asset");
        MainMenuController menu = Object.FindObjectOfType<MainMenuController>();
        if (menu == null)
        {
            Debug.LogError("MainMenuController not found in menu scene.");
            return;
        }

        SettingsPopup existing = Object.FindObjectOfType<SettingsPopup>();
        if (existing != null)
        {
            existing.database = db;
            menu.settingsPopup = existing;
            EditorUtility.SetDirty(existing);
            EditorUtility.SetDirty(menu);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            return;
        }

        Canvas canvas = FindHudCanvas();
        if (canvas == null)
        {
            Debug.LogError("No canvas in menu scene for settings popup.");
            return;
        }

        GameObject popupGo = new GameObject("SettingsPopup");
        popupGo.transform.SetParent(canvas.transform, false);
        RectTransform popupRt = popupGo.AddComponent<RectTransform>();
        popupRt.anchorMin = Vector2.zero; popupRt.anchorMax = Vector2.one;
        popupRt.offsetMin = Vector2.zero; popupRt.offsetMax = Vector2.zero;

        Image dim = popupGo.AddComponent<Image>();
        dim.color = new Color(0, 0, 0, 0.6f);

        PopupBase popupBase = popupGo.AddComponent<PopupBase>();
        popupBase.backdrop = dim;
        SettingsPopup settings = popupGo.AddComponent<SettingsPopup>();

        GameObject windowGo = new GameObject("Window");
        windowGo.transform.SetParent(popupGo.transform, false);
        Image windowBg = windowGo.AddComponent<Image>();
        windowBg.color = ColorPanel;
        windowBg.sprite = GetUISprite();
        windowBg.type = Image.Type.Sliced;
        RectTransform windowRt = windowBg.rectTransform;
        windowRt.anchorMin = new Vector2(0.5f, 0.5f);
        windowRt.anchorMax = new Vector2(0.5f, 0.5f);
        windowRt.pivot = new Vector2(0.5f, 0.5f);
        windowRt.sizeDelta = new Vector2(820, 1000);

        GameObject titleGo = new GameObject("Title");
        titleGo.transform.SetParent(windowGo.transform, false);
        TextMeshProUGUI title = titleGo.AddComponent<TextMeshProUGUI>();
        title.text = "SETTINGS";
        title.fontSize = 60; title.color = ColorText; title.fontStyle = FontStyles.Bold;
        title.alignment = TextAlignmentOptions.Center;
        RectTransform titleRt = title.rectTransform;
        titleRt.anchorMin = new Vector2(0, 1); titleRt.anchorMax = new Vector2(1, 1);
        titleRt.pivot = new Vector2(0.5f, 1f);
        titleRt.sizeDelta = new Vector2(0, 130);
        titleRt.anchoredPosition = new Vector2(0, -40);

        Button sfxBtn; TextMeshProUGUI sfxLbl;
        CreateSettingRow(windowGo.transform, "SfxRow", "Sound", 0.66f, out sfxBtn, out sfxLbl);
        Button musicBtn; TextMeshProUGUI musicLbl;
        CreateSettingRow(windowGo.transform, "MusicRow", "Music", 0.52f, out musicBtn, out musicLbl);
        Button hapticBtn; TextMeshProUGUI hapticLbl;
        CreateSettingRow(windowGo.transform, "HapticRow", "Vibration", 0.38f, out hapticBtn, out hapticLbl);

        Button resetBtn; TextMeshProUGUI resetLbl;
        CreateWideButton(windowGo.transform, "ResetButton", "Reset Progress", 0.18f, ColorDanger,
            out resetBtn, out resetLbl);

        Button closeBtn; TextMeshProUGUI closeLbl;
        CreateWideButton(windowGo.transform, "CloseButton", "CLOSE", 0.04f, ColorPrimary,
            out closeBtn, out closeLbl);

        settings.popup = popupBase;
        settings.closeButton = closeBtn;
        settings.sfxToggle = sfxBtn;
        settings.musicToggle = musicBtn;
        settings.hapticsToggle = hapticBtn;
        settings.resetButton = resetBtn;
        settings.sfxLabel = sfxLbl;
        settings.musicLabel = musicLbl;
        settings.hapticsLabel = hapticLbl;
        settings.resetLabel = resetLbl;
        settings.database = db;

        popupBase.window = windowRt;

        menu.settingsPopup = settings;

        popupGo.SetActive(false);

        EditorUtility.SetDirty(menu);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void CreateSettingRow(Transform parent, string name, string label, float anchorY,
        out Button toggle, out TextMeshProUGUI labelTmp)
    {
        GameObject rowGo = new GameObject(name);
        rowGo.transform.SetParent(parent, false);
        RectTransform rowRt = rowGo.AddComponent<RectTransform>();
        rowRt.anchorMin = new Vector2(0.5f, anchorY);
        rowRt.anchorMax = new Vector2(0.5f, anchorY);
        rowRt.pivot = new Vector2(0.5f, 0.5f);
        rowRt.sizeDelta = new Vector2(680, 120);

        Image img = rowGo.AddComponent<Image>();
        img.color = ColorPrimary;
        img.sprite = GetUISprite();
        img.type = Image.Type.Sliced;

        toggle = rowGo.AddComponent<Button>();
        rowGo.AddComponent<ButtonPunch>();

        GameObject lblGo = new GameObject("Label");
        lblGo.transform.SetParent(rowGo.transform, false);
        labelTmp = lblGo.AddComponent<TextMeshProUGUI>();
        labelTmp.text = label + ": On";
        labelTmp.fontSize = 44;
        labelTmp.color = ColorText;
        labelTmp.fontStyle = FontStyles.Bold;
        labelTmp.alignment = TextAlignmentOptions.Center;
        RectTransform lblRt = labelTmp.rectTransform;
        lblRt.anchorMin = Vector2.zero; lblRt.anchorMax = Vector2.one;
        lblRt.offsetMin = Vector2.zero; lblRt.offsetMax = Vector2.zero;
    }

    private static void CreateWideButton(Transform parent, string name, string label, float anchorY,
        Color color, out Button button, out TextMeshProUGUI labelTmp)
    {
        GameObject btnGo = new GameObject(name);
        btnGo.transform.SetParent(parent, false);
        RectTransform rt = btnGo.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, anchorY);
        rt.anchorMax = new Vector2(0.5f, anchorY);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(680, 120);

        Image img = btnGo.AddComponent<Image>();
        img.color = color;
        img.sprite = GetUISprite();
        img.type = Image.Type.Sliced;

        button = btnGo.AddComponent<Button>();
        btnGo.AddComponent<ButtonPunch>();

        GameObject lblGo = new GameObject("Label");
        lblGo.transform.SetParent(btnGo.transform, false);
        labelTmp = lblGo.AddComponent<TextMeshProUGUI>();
        labelTmp.text = label;
        labelTmp.fontSize = 44;
        labelTmp.color = ColorText;
        labelTmp.fontStyle = FontStyles.Bold;
        labelTmp.alignment = TextAlignmentOptions.Center;
        RectTransform lblRt = labelTmp.rectTransform;
        lblRt.anchorMin = Vector2.zero; lblRt.anchorMax = Vector2.one;
        lblRt.offsetMin = Vector2.zero; lblRt.offsetMax = Vector2.zero;
    }

    private static Sprite cachedUISprite;
    private static Sprite GetUISprite()
    {
        if (cachedUISprite != null) return cachedUISprite;
        cachedUISprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        return cachedUISprite;
    }
}
