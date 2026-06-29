using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class Iteration1Setup
{
    private const string ScenesFolder = "Assets/LoopPuzzle/Scenes";
    private const string MenuScenePath = "Assets/LoopPuzzle/Scenes/MainMenu.unity";
    private const string GameScenePath = "Assets/LoopPuzzle/Scenes/Game.unity";

    private static readonly Color ColorBg = new Color(0.102f, 0.102f, 0.180f, 1f);
    private static readonly Color ColorPrimary = new Color(0.290f, 0.565f, 0.886f, 1f);
    private static readonly Color ColorAccent = new Color(0.961f, 0.651f, 0.137f, 1f);
    private static readonly Color ColorText = Color.white;

    [MenuItem("Tools/Loop Puzzle/Setup Iteration 1")]
    public static void Setup()
    {
        EnsureFolder();
        BuildMenuScene();
        BuildGameScene();
        AddScenesToBuildSettings();

        EditorSceneManager.OpenScene(MenuScenePath);
        Debug.Log("Iteration 1 setup complete. MainMenu and Game scenes created.");
    }

    private static void EnsureFolder()
    {
        if (!AssetDatabase.IsValidFolder(ScenesFolder))
        {
            AssetDatabase.CreateFolder("Assets/LoopPuzzle", "Scenes");
        }
    }

    private static void BuildMenuScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CreateCamera();
        CreateEventSystem();
        CreateManagers();

        GameObject canvasGo = CreateCanvas("MenuCanvas");
        GameObject safeArea = CreateSafeArea(canvasGo.transform);

        MainMenuController controller = canvasGo.AddComponent<MainMenuController>();

        RectTransform title = CreateText(safeArea.transform, "Title", "LOOP PUZZLE", 90,
            new Vector2(0.5f, 0.78f), ColorText);
        AddOutline(title.gameObject);

        RectTransform playBtn = CreateButton(safeArea.transform, "PlayButton", "PLAY",
            new Vector2(0.5f, 0.46f), new Vector2(620, 170), ColorAccent, 60);
        RectTransform levelsBtn = CreateButton(safeArea.transform, "LevelsButton", "LEVELS",
            new Vector2(0.5f, 0.32f), new Vector2(620, 150), ColorPrimary, 52);
        RectTransform settingsBtn = CreateButton(safeArea.transform, "SettingsButton", "SETTINGS",
            new Vector2(0.5f, 0.19f), new Vector2(620, 150), ColorPrimary, 52);

        controller.titleTransform = title;
        controller.playButton = playBtn;
        controller.levelsButton = levelsBtn;
        controller.settingsButton = settingsBtn;
        controller.gameSceneName = "Game";

        EditorSceneManager.SaveScene(scene, MenuScenePath);
    }

    private static void BuildGameScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CreateCamera();
        CreateEventSystem();

        GameObject canvasGo = CreateCanvas("GameCanvas");
        GameObject safeArea = CreateSafeArea(canvasGo.transform);

        GameSceneController controller = canvasGo.AddComponent<GameSceneController>();

        RectTransform label = CreateText(safeArea.transform, "PlaceholderLabel", "GAME SCENE", 70,
            new Vector2(0.5f, 0.55f), ColorText);
        AddOutline(label.gameObject);

        RectTransform backBtn = CreateButton(safeArea.transform, "BackButton", "BACK",
            new Vector2(0.5f, 0.12f), new Vector2(400, 140), ColorPrimary, 48);

        controller.backButton = backBtn.GetComponent<Button>();
        controller.menuSceneName = "MainMenu";

        EditorSceneManager.SaveScene(scene, GameScenePath);
    }

    private static void CreateCamera()
    {
        GameObject camGo = new GameObject("Main Camera");
        Camera cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = ColorBg;
        cam.orthographic = true;
        cam.tag = "MainCamera";
        camGo.transform.position = new Vector3(0, 0, -10);
    }

    private static void CreateEventSystem()
    {
        GameObject es = new GameObject("EventSystem");
        es.AddComponent<UnityEngine.EventSystems.EventSystem>();
        es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
    }

    private static void CreateManagers()
    {
        GameObject bootstrap = new GameObject("GameBootstrap");
        bootstrap.AddComponent<GameBootstrap>();

        GameObject sound = new GameObject("SoundManager");
        sound.AddComponent<SoundManager>();

        GameObject haptic = new GameObject("HapticManager");
        haptic.AddComponent<HapticManager>();

        GameObject transition = new GameObject("TransitionManager");
        transition.AddComponent<TransitionManager>();
    }

    private static GameObject CreateCanvas(string name)
    {
        GameObject canvasGo = new GameObject(name);
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasGo.AddComponent<GraphicRaycaster>();
        return canvasGo;
    }

    private static GameObject CreateSafeArea(Transform parent)
    {
        GameObject go = new GameObject("SafeArea");
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        go.AddComponent<SafeAreaFitter>();
        return go;
    }

    private static RectTransform CreateText(Transform parent, string name, string content, int fontSize,
        Vector2 anchor, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;

        RectTransform rt = tmp.rectTransform;
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(900, 200);
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
        AddOutline(textGo);

        RectTransform trt = tmp.rectTransform;
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;

        return rt;
    }

    private static void AddOutline(GameObject textGo)
    {
        TextMeshProUGUI tmp = textGo.GetComponent<TextMeshProUGUI>();
        if (tmp == null) return;
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

    private static void AddScenesToBuildSettings()
    {
        EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene(MenuScenePath, true),
            new EditorBuildSettingsScene(GameScenePath, true)
        };
        EditorBuildSettings.scenes = scenes;
    }
}
