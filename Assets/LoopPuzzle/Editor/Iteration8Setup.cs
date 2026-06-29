using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class Iteration8Setup
{
    private const string GameScenePath = "Assets/LoopPuzzle/Scenes/Game.unity";

    private static readonly Color ColorPanel = new Color(0.094f, 0.094f, 0.149f, 0.92f);
    private static readonly Color ColorText = Color.white;
    private static readonly Color ColorAccent = new Color(0.961f, 0.651f, 0.137f, 1f);

    [MenuItem("Tools/Loop Puzzle/Setup Iteration 8")]
    public static void Setup()
    {
        Scene scene = EditorSceneManager.OpenScene(GameScenePath);

        LevelController controller = Object.FindObjectOfType<LevelController>();
        GridManager grid = Object.FindObjectOfType<GridManager>();
        if (controller == null || grid == null)
        {
            Debug.LogError("Game scene missing LevelController/GridManager. Run earlier setups first.");
            return;
        }

        Canvas canvas = FindHudCanvas();
        if (canvas == null)
        {
            Debug.LogError("No canvas found in Game scene.");
            return;
        }

        TutorialBanner banner = Object.FindObjectOfType<TutorialBanner>();
        if (banner == null)
        {
            banner = BuildBanner(canvas.transform);
        }

        TutorialController tutorial = Object.FindObjectOfType<TutorialController>();
        if (tutorial == null)
        {
            GameObject tutGo = new GameObject("TutorialController");
            tutorial = tutGo.AddComponent<TutorialController>();
        }
        tutorial.levelController = controller;
        tutorial.gridManager = grid;
        tutorial.banner = banner;

        EditorUtility.SetDirty(tutorial);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("Iteration 8 setup complete: tutorial controller and banner wired into Game scene.");
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

    private static TutorialBanner BuildBanner(Transform canvas)
    {
        GameObject panelGo = new GameObject("TutorialBanner");
        panelGo.transform.SetParent(canvas, false);

        RectTransform panelRt = panelGo.AddComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.5f, 1f);
        panelRt.anchorMax = new Vector2(0.5f, 1f);
        panelRt.pivot = new Vector2(0.5f, 1f);
        panelRt.sizeDelta = new Vector2(900, 150);
        panelRt.anchoredPosition = new Vector2(0, -220);

        CanvasGroup cg = panelGo.AddComponent<CanvasGroup>();

        Image bg = panelGo.AddComponent<Image>();
        bg.color = ColorPanel;
        bg.sprite = GetUISprite();
        bg.type = Image.Type.Sliced;

        GameObject labelGo = new GameObject("Label");
        labelGo.transform.SetParent(panelGo.transform, false);
        TextMeshProUGUI label = labelGo.AddComponent<TextMeshProUGUI>();
        label.text = "";
        label.fontSize = 40;
        label.color = ColorText;
        label.fontStyle = FontStyles.Bold;
        label.alignment = TextAlignmentOptions.Center;
        RectTransform labelRt = label.rectTransform;
        labelRt.anchorMin = Vector2.zero;
        labelRt.anchorMax = Vector2.one;
        labelRt.offsetMin = new Vector2(30, 10);
        labelRt.offsetMax = new Vector2(-30, -10);

        TutorialBanner banner = panelGo.AddComponent<TutorialBanner>();
        banner.panel = panelRt;
        banner.label = label;
        banner.canvasGroup = cg;

        return banner;
    }

    private static Sprite cachedUISprite;
    private static Sprite GetUISprite()
    {
        if (cachedUISprite != null) return cachedUISprite;
        cachedUISprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        return cachedUISprite;
    }
}
