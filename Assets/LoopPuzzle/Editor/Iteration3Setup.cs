using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class Iteration3Setup
{
    private const string GameScenePath = "Assets/LoopPuzzle/Scenes/Game.unity";

    private static readonly Color ColorPrimary = new Color(0.290f, 0.565f, 0.886f, 1f);
    private static readonly Color ColorAccent = new Color(0.961f, 0.651f, 0.137f, 1f);
    private static readonly Color ColorPanel = new Color(0.094f, 0.094f, 0.149f, 0.92f);
    private static readonly Color ColorText = Color.white;

    [MenuItem("Tools/Loop Puzzle/Setup Iteration 3")]
    public static void Setup()
    {
        Scene scene = EditorSceneManager.OpenScene(GameScenePath);

        GridManager grid = Object.FindObjectOfType<GridManager>();
        Debug.Assert(grid != null, "GridManager missing. Run Setup Iteration 2 first.");

        PieceInput input = Object.FindObjectOfType<PieceInput>();
        Debug.Assert(input != null, "PieceInput missing. Run Setup Iteration 2 first.");

        GameObject canvasGo = FindGameCanvas();
        Transform safeArea = canvasGo.transform.Find("SafeArea");
        Debug.Assert(safeArea != null, "SafeArea missing on GameCanvas.");

        HUDController hud = BuildHUD(safeArea);
        WinPopup winPopup = BuildWinPopup(canvasGo.transform);

        LevelController controller = Object.FindObjectOfType<LevelController>();
        if (controller == null)
        {
            GameObject ctrlGo = new GameObject("LevelController");
            controller = ctrlGo.AddComponent<LevelController>();
        }

        controller.gridManager = grid;
        controller.pieceInput = input;
        controller.hud = hud;
        controller.winPopup = winPopup;

        input.levelController = controller;

        EditorUtility.SetDirty(controller);
        EditorUtility.SetDirty(input);
        EditorUtility.SetDirty(hud);
        EditorUtility.SetDirty(winPopup);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("Iteration 3 setup complete. HUD, win popup, and validation wired.");
    }

    private static GameObject FindGameCanvas()
    {
        Canvas[] canvases = Object.FindObjectsOfType<Canvas>();
        foreach (Canvas c in canvases)
        {
            if (c.name == "GameCanvas") return c.gameObject;
        }
        Debug.Assert(false, "GameCanvas not found. Run Setup Iteration 1 first.");
        return null;
    }

    private static HUDController BuildHUD(Transform safeArea)
    {
        Transform existing = safeArea.Find("HUD");
        if (existing != null) Object.DestroyImmediate(existing.gameObject);

        GameObject hudGo = new GameObject("HUD");
        hudGo.transform.SetParent(safeArea, false);
        RectTransform hudRt = hudGo.AddComponent<RectTransform>();
        hudRt.anchorMin = new Vector2(0f, 1f);
        hudRt.anchorMax = new Vector2(1f, 1f);
        hudRt.pivot = new Vector2(0.5f, 1f);
        hudRt.sizeDelta = new Vector2(0f, 140f);
        hudRt.anchoredPosition = Vector2.zero;

        Image bg = hudGo.AddComponent<Image>();
        bg.color = ColorPanel;
        bg.sprite = GetUISprite();
        bg.type = Image.Type.Sliced;

        HUDController hud = hudGo.AddComponent<HUDController>();

        RectTransform levelRt = CreateText(hudGo.transform, "LevelText", "Level 1", 46,
            new Vector2(0f, 0.5f), new Vector2(0.45f, 0.5f), TextAlignmentOptions.Left);
        levelRt.anchoredPosition = new Vector2(40f, 0f);

        RectTransform movesRt = CreateText(hudGo.transform, "MovesText", "Moves: 0", 46,
            new Vector2(0.55f, 0.5f), new Vector2(1f, 0.5f), TextAlignmentOptions.Right);
        movesRt.anchoredPosition = new Vector2(-40f, 0f);

        hud.levelText = levelRt.GetComponent<TextMeshProUGUI>();
        hud.movesText = movesRt.GetComponent<TextMeshProUGUI>();

        return hud;
    }

    private static WinPopup BuildWinPopup(Transform canvas)
    {
        Transform existing = canvas.Find("WinPopup");
        if (existing != null) Object.DestroyImmediate(existing.gameObject);

        GameObject popupGo = new GameObject("WinPopup");
        popupGo.transform.SetParent(canvas, false);
        RectTransform popupRt = popupGo.AddComponent<RectTransform>();
        popupRt.anchorMin = Vector2.zero;
        popupRt.anchorMax = Vector2.one;
        popupRt.offsetMin = Vector2.zero;
        popupRt.offsetMax = Vector2.zero;

        GameObject backdropGo = new GameObject("Backdrop");
        backdropGo.transform.SetParent(popupGo.transform, false);
        Image backdrop = backdropGo.AddComponent<Image>();
        backdrop.color = new Color(0f, 0f, 0f, 0f);
        RectTransform backdropRt = backdrop.rectTransform;
        backdropRt.anchorMin = Vector2.zero;
        backdropRt.anchorMax = Vector2.one;
        backdropRt.offsetMin = Vector2.zero;
        backdropRt.offsetMax = Vector2.zero;

        GameObject windowGo = new GameObject("Window");
        windowGo.transform.SetParent(popupGo.transform, false);
        Image windowBg = windowGo.AddComponent<Image>();
        windowBg.color = new Color(0.16f, 0.16f, 0.24f, 1f);
        windowBg.sprite = GetUISprite();
        windowBg.type = Image.Type.Sliced;
        RectTransform windowRt = windowBg.rectTransform;
        windowRt.anchorMin = new Vector2(0.5f, 0.5f);
        windowRt.anchorMax = new Vector2(0.5f, 0.5f);
        windowRt.pivot = new Vector2(0.5f, 0.5f);
        windowRt.sizeDelta = new Vector2(820f, 900f);
        windowRt.anchoredPosition = Vector2.zero;

        RectTransform titleRt = CreateText(windowGo.transform, "Title", "LEVEL COMPLETE", 64,
            new Vector2(0.5f, 0.82f), new Vector2(0.5f, 0.82f), TextAlignmentOptions.Center);
        titleRt.sizeDelta = new Vector2(760f, 120f);
        titleRt.GetComponent<TextMeshProUGUI>().color = ColorAccent;

        RectTransform starsRt = CreateText(windowGo.transform, "StarsPlaceholder", "★ ★ ★", 80,
            new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), TextAlignmentOptions.Center);
        starsRt.sizeDelta = new Vector2(700f, 140f);
        starsRt.GetComponent<TextMeshProUGUI>().color = new Color(1f, 1f, 1f, 0.3f);

        RectTransform movesRt = CreateText(windowGo.transform, "MovesText", "Moves: 0", 48,
            new Vector2(0.5f, 0.45f), new Vector2(0.5f, 0.45f), TextAlignmentOptions.Center);
        movesRt.sizeDelta = new Vector2(700f, 90f);

        RectTransform nextBtn = CreateButton(windowGo.transform, "NextButton", "NEXT",
            new Vector2(0.5f, 0.26f), new Vector2(560f, 150f), ColorAccent, 50);
        RectTransform restartBtn = CreateButton(windowGo.transform, "RestartButton", "RESTART",
            new Vector2(0.5f, 0.12f), new Vector2(560f, 150f), ColorPrimary, 50);

        PopupBase popupBase = popupGo.AddComponent<PopupBase>();
        popupBase.window = windowRt;
        popupBase.backdrop = backdrop;

        WinPopup winPopup = popupGo.AddComponent<WinPopup>();
        winPopup.popup = popupBase;
        winPopup.movesText = movesRt.GetComponent<TextMeshProUGUI>();
        winPopup.nextButton = nextBtn.GetComponent<Button>();
        winPopup.restartButton = restartBtn.GetComponent<Button>();

        popupGo.SetActive(false);

        return winPopup;
    }

    private static RectTransform CreateText(Transform parent, string name, string content, int fontSize,
        Vector2 anchorMin, Vector2 anchorMax, TextAlignmentOptions align)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.color = ColorText;
        tmp.alignment = align;
        tmp.fontStyle = FontStyles.Bold;

        AddShadow(go);

        RectTransform rt = tmp.rectTransform;
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(400f, 80f);
        rt.anchoredPosition = Vector2.zero;
        return rt;
    }

    private static RectTransform CreateButton(Transform parent, string name, string label,
        Vector2 anchor, Vector2 size, Color color, int fontSize)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        Image img = go.AddComponent<Image>();
        img.color = color;
        img.sprite = GetUISprite();
        img.type = Image.Type.Sliced;

        RectTransform rt = img.rectTransform;
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = Vector2.zero;

        Button btn = go.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.highlightedColor = Color.white;
        cb.pressedColor = new Color(color.r * 0.85f, color.g * 0.85f, color.b * 0.85f, 1f);
        cb.disabledColor = new Color(color.r, color.g, color.b, 0.5f);
        btn.colors = cb;

        go.AddComponent<ButtonPunch>();

        GameObject textGo = new GameObject("Label");
        textGo.transform.SetParent(go.transform, false);
        TextMeshProUGUI tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = fontSize;
        tmp.color = ColorText;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        AddShadow(textGo);

        RectTransform trt = tmp.rectTransform;
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;

        return rt;
    }

    private static void AddShadow(GameObject textGo)
    {
        Shadow shadow = textGo.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.6f);
        shadow.effectDistance = new Vector2(3f, -3f);
    }

    private static Sprite cachedUISprite;

    private static Sprite GetUISprite()
    {
        if (cachedUISprite != null) return cachedUISprite;
        cachedUISprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        return cachedUISprite;
    }
}
