using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class Iteration10Setup
{
    private const string GameScenePath = "Assets/LoopPuzzle/Scenes/Game.unity";

    private static readonly Color ColorPrimary = new Color(0.290f, 0.565f, 0.886f, 1f);
    private static readonly Color ColorAccent = new Color(0.961f, 0.651f, 0.137f, 1f);
    private static readonly Color ColorPanel = new Color(0.16f, 0.16f, 0.24f, 1f);
    private static readonly Color ColorText = Color.white;
    private static readonly Color ColorNeutral = new Color(0.4f, 0.4f, 0.48f, 1f);

    [MenuItem("Tools/Loop Puzzle/Setup Iteration 10")]
    public static void Setup()
    {
        Scene scene = EditorSceneManager.OpenScene(GameScenePath);

        LevelController controller = FindInScene<LevelController>(scene);
        HUDController hud = FindInScene<HUDController>(scene);
        if (controller == null || hud == null)
        {
            Debug.LogError("Game scene missing LevelController/HUD. Run earlier setups first.");
            return;
        }

        Canvas canvas = FindHudCanvas(scene);
        if (canvas == null)
        {
            Debug.LogError("No screen-space canvas in Game scene.");
            return;
        }

        Button pauseButton = EnsurePauseButton(canvas.transform, hud);
        hud.pauseButton = pauseButton;
        hud.levelController = controller;

        PausePopup pause = EnsurePausePopup(canvas.transform);
        GameCompletePopup complete = EnsureGameCompletePopup(canvas.transform);

        controller.pausePopup = pause;
        controller.gameCompletePopup = complete;

        WireWinPopupNextLabel(scene);

        EditorUtility.SetDirty(hud);
        EditorUtility.SetDirty(controller);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("Iteration 10 setup complete: pause button, pause popup, and game-complete popup wired.");
    }

    private static void WireWinPopupNextLabel(Scene scene)
    {
        WinPopup winPopup = FindInSceneAll<WinPopup>(scene);
        if (winPopup == null || winPopup.nextButton == null) return;
        if (winPopup.nextButtonLabel != null) return;

        TextMeshProUGUI label = winPopup.nextButton.GetComponentInChildren<TextMeshProUGUI>(true);
        if (label != null)
        {
            winPopup.nextButtonLabel = label;
            EditorUtility.SetDirty(winPopup);
        }
    }

    private static Button EnsurePauseButton(Transform canvas, HUDController hud)
    {
        Transform existing = canvas.Find("PauseButton");
        if (existing != null) return existing.GetComponent<Button>();

        GameObject btnGo = new GameObject("PauseButton");
        btnGo.transform.SetParent(canvas, false);

        Image img = btnGo.AddComponent<Image>();
        img.color = ColorNeutral;
        img.sprite = GetUISprite();
        img.type = Image.Type.Sliced;

        RectTransform rt = img.rectTransform;
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.sizeDelta = new Vector2(110, 110);
        rt.anchoredPosition = new Vector2(-30, -30);

        Button btn = btnGo.AddComponent<Button>();
        btnGo.AddComponent<ButtonPunch>();

        GameObject lblGo = new GameObject("Label");
        lblGo.transform.SetParent(btnGo.transform, false);
        TextMeshProUGUI tmp = lblGo.AddComponent<TextMeshProUGUI>();
        tmp.text = "II";
        tmp.fontSize = 46;
        tmp.color = ColorText;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        RectTransform lblRt = tmp.rectTransform;
        lblRt.anchorMin = Vector2.zero;
        lblRt.anchorMax = Vector2.one;
        lblRt.offsetMin = Vector2.zero;
        lblRt.offsetMax = Vector2.zero;

        return btn;
    }

    private static PausePopup EnsurePausePopup(Transform canvas)
    {
        PausePopup existing = FindInSceneAll<PausePopup>(canvas.gameObject.scene);
        if (existing != null) return existing;

        GameObject popupGo = CreatePopupRoot(canvas, "PausePopup", out PopupBase popupBase, out Transform window);

        PausePopup pause = popupGo.AddComponent<PausePopup>();
        pause.popup = popupBase;

        AddTitle(window, "PAUSED");

        pause.resumeButton = AddWideButton(window, "ResumeButton", "RESUME", 0.62f, ColorPrimary);
        pause.restartButton = AddWideButton(window, "RestartButton", "RESTART", 0.42f, ColorAccent);
        pause.menuButton = AddWideButton(window, "MenuButton", "MAIN MENU", 0.22f, ColorNeutral);

        popupGo.SetActive(false);
        return pause;
    }

    private static GameCompletePopup EnsureGameCompletePopup(Transform canvas)
    {
        GameCompletePopup existing = FindInSceneAll<GameCompletePopup>(canvas.gameObject.scene);
        if (existing != null) return existing;

        GameObject popupGo = CreatePopupRoot(canvas, "GameCompletePopup", out PopupBase popupBase, out Transform window);

        GameCompletePopup complete = popupGo.AddComponent<GameCompletePopup>();
        complete.popup = popupBase;

        complete.titleText = AddTitle(window, "All Levels Complete!");

        GameObject summaryGo = new GameObject("Summary");
        summaryGo.transform.SetParent(window, false);
        TextMeshProUGUI summary = summaryGo.AddComponent<TextMeshProUGUI>();
        summary.text = "Stars collected: 0 / 0";
        summary.fontSize = 44;
        summary.color = ColorText;
        summary.alignment = TextAlignmentOptions.Center;
        RectTransform sumRt = summary.rectTransform;
        sumRt.anchorMin = new Vector2(0.5f, 0.5f);
        sumRt.anchorMax = new Vector2(0.5f, 0.5f);
        sumRt.pivot = new Vector2(0.5f, 0.5f);
        sumRt.sizeDelta = new Vector2(700, 120);
        sumRt.anchoredPosition = new Vector2(0, 40);
        complete.summaryText = summary;

        complete.menuButton = AddWideButton(window, "MenuButton", "MAIN MENU", 0.16f, ColorPrimary);

        popupGo.SetActive(false);
        return complete;
    }

    private static GameObject CreatePopupRoot(Transform canvas, string name, out PopupBase popupBase, out Transform window)
    {
        GameObject popupGo = new GameObject(name);
        popupGo.transform.SetParent(canvas, false);
        RectTransform popupRt = popupGo.AddComponent<RectTransform>();
        popupRt.anchorMin = Vector2.zero;
        popupRt.anchorMax = Vector2.one;
        popupRt.offsetMin = Vector2.zero;
        popupRt.offsetMax = Vector2.zero;

        Image dim = popupGo.AddComponent<Image>();
        dim.color = new Color(0, 0, 0, 0.6f);

        popupBase = popupGo.AddComponent<PopupBase>();
        popupBase.backdrop = dim;

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
        windowRt.sizeDelta = new Vector2(820, 900);

        popupBase.window = windowRt;
        window = windowGo.transform;
        return popupGo;
    }

    private static TextMeshProUGUI AddTitle(Transform window, string text)
    {
        GameObject titleGo = new GameObject("Title");
        titleGo.transform.SetParent(window, false);
        TextMeshProUGUI title = titleGo.AddComponent<TextMeshProUGUI>();
        title.text = text;
        title.fontSize = 56;
        title.color = ColorText;
        title.fontStyle = FontStyles.Bold;
        title.alignment = TextAlignmentOptions.Center;
        title.enableWordWrapping = true;
        RectTransform titleRt = title.rectTransform;
        titleRt.anchorMin = new Vector2(0, 1);
        titleRt.anchorMax = new Vector2(1, 1);
        titleRt.pivot = new Vector2(0.5f, 1f);
        titleRt.sizeDelta = new Vector2(0, 180);
        titleRt.anchoredPosition = new Vector2(0, -50);
        return title;
    }

    private static Button AddWideButton(Transform window, string name, string label, float anchorY, Color color)
    {
        GameObject btnGo = new GameObject(name);
        btnGo.transform.SetParent(window, false);
        RectTransform rt = btnGo.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, anchorY);
        rt.anchorMax = new Vector2(0.5f, anchorY);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(620, 120);

        Image img = btnGo.AddComponent<Image>();
        img.color = color;
        img.sprite = GetUISprite();
        img.type = Image.Type.Sliced;

        Button btn = btnGo.AddComponent<Button>();
        btnGo.AddComponent<ButtonPunch>();

        GameObject lblGo = new GameObject("Label");
        lblGo.transform.SetParent(btnGo.transform, false);
        TextMeshProUGUI tmp = lblGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 44;
        tmp.color = ColorText;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        RectTransform lblRt = tmp.rectTransform;
        lblRt.anchorMin = Vector2.zero;
        lblRt.anchorMax = Vector2.one;
        lblRt.offsetMin = Vector2.zero;
        lblRt.offsetMax = Vector2.zero;

        return btn;
    }

    private static Canvas FindHudCanvas(Scene scene)
    {
        Canvas[] canvases = Resources.FindObjectsOfTypeAll<Canvas>();
        foreach (Canvas c in canvases)
        {
            if (EditorUtility.IsPersistent(c)) continue;
            if (c.hideFlags != HideFlags.None) continue;
            if (c.gameObject.scene != scene) continue;
            if (c.renderMode == RenderMode.ScreenSpaceOverlay) return c;
        }
        foreach (Canvas c in canvases)
        {
            if (EditorUtility.IsPersistent(c)) continue;
            if (c.gameObject.scene != scene) continue;
            return c;
        }
        return null;
    }

    private static T FindInScene<T>(Scene scene) where T : Component
    {
        return FindInSceneAll<T>(scene);
    }

    private static T FindInSceneAll<T>(Scene scene) where T : Component
    {
        T[] all = Resources.FindObjectsOfTypeAll<T>();
        foreach (T item in all)
        {
            if (item == null) continue;
            if (EditorUtility.IsPersistent(item)) continue;
            if (item.hideFlags != HideFlags.None) continue;
            if (!item.gameObject.scene.IsValid()) continue;
            if (item.gameObject.scene != scene) continue;
            return item;
        }
        return null;
    }

    private static Sprite cachedUISprite;
    private static Sprite GetUISprite()
    {
        if (cachedUISprite != null) return cachedUISprite;
        cachedUISprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        return cachedUISprite;
    }
}
