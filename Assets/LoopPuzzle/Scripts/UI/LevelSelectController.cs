using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectController : MonoBehaviour
{
    public LevelDatabase database;
    public RectTransform contentParent;
    public Button backButton;
    public Sprite buttonSprite;
    public string gameSceneName = "Game";
    public string menuSceneName = "MainMenu";

    private static readonly string[] WorldNames =
    {
        "", "World 1 - Learning", "World 2 - Obstacles", "World 3 - Fixed Pieces",
        "World 4 - Multiple Loops", "World 5 - Advanced"
    };

    private static readonly Color ColorPrimary = new Color(0.290f, 0.565f, 0.886f, 1f);
    private static readonly Color ColorAccent = new Color(0.961f, 0.651f, 0.137f, 1f);
    private static readonly Color ColorText = Color.white;

    private void Start()
    {
        backButton.onClick.AddListener(OnBack);
        BuildList();
    }

    private void BuildList()
    {
        Dictionary<int, List<LevelData>> byWorld = new Dictionary<int, List<LevelData>>();
        foreach (LevelData level in database.levels)
        {
            if (level == null) continue;
            if (!byWorld.ContainsKey(level.world)) byWorld[level.world] = new List<LevelData>();
            byWorld[level.world].Add(level);
        }

        List<int> worlds = new List<int>(byWorld.Keys);
        worlds.Sort();

        foreach (int world in worlds)
        {
            CreateWorldHeader(world);
            CreateWorldGrid(byWorld[world]);
        }
    }

    private void CreateWorldHeader(int world)
    {
        GameObject headerGo = new GameObject("WorldHeader_" + world);
        headerGo.transform.SetParent(contentParent, false);

        RectTransform rt = headerGo.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, 90);

        LayoutElement le = headerGo.AddComponent<LayoutElement>();
        le.preferredHeight = 90;

        TextMeshProUGUI tmp = headerGo.AddComponent<TextMeshProUGUI>();
        tmp.text = world < WorldNames.Length ? WorldNames[world] : ("World " + world);
        tmp.fontSize = 44;
        tmp.color = ColorAccent;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Left;
        tmp.margin = new Vector4(30, 0, 0, 0);
    }

    private void CreateWorldGrid(List<LevelData> levels)
    {
        GameObject gridGo = new GameObject("WorldGrid");
        gridGo.transform.SetParent(contentParent, false);

        RectTransform rt = gridGo.AddComponent<RectTransform>();

        GridLayoutGroup grid = gridGo.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(150, 180);
        grid.spacing = new Vector2(24, 24);
        grid.padding = new RectOffset(30, 30, 10, 30);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 5;

        ContentSizeFitter fitter = gridGo.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        levels.Sort((a, b) => a.levelNumber.CompareTo(b.levelNumber));

        foreach (LevelData level in levels)
        {
            CreateLevelButton(gridGo.transform, level);
        }
    }

    private void CreateLevelButton(Transform parent, LevelData level)
    {
        int stars = ProgressManager.Instance != null ? ProgressManager.Instance.GetStars(level.levelNumber) : 0;
        bool unlocked = ProgressManager.Instance == null || ProgressManager.Instance.IsLevelUnlocked(level.levelNumber);

        GameObject btnGo = new GameObject("LevelButton_" + level.levelNumber);
        btnGo.transform.SetParent(parent, false);

        Image img = btnGo.AddComponent<Image>();
        img.color = unlocked ? ColorPrimary : new Color(0.2f, 0.2f, 0.28f, 1f);
        img.sprite = buttonSprite;
        img.type = Image.Type.Sliced;

        Button button = btnGo.AddComponent<Button>();
        btnGo.AddComponent<ButtonPunch>();

        LevelButton lb = btnGo.AddComponent<LevelButton>();

        GameObject numGo = new GameObject("Number");
        numGo.transform.SetParent(btnGo.transform, false);
        TextMeshProUGUI numTmp = numGo.AddComponent<TextMeshProUGUI>();
        numTmp.text = level.levelNumber.ToString();
        numTmp.fontSize = 52;
        numTmp.color = ColorText;
        numTmp.fontStyle = FontStyles.Bold;
        numTmp.alignment = TextAlignmentOptions.Center;
        RectTransform numRt = numTmp.rectTransform;
        numRt.anchorMin = new Vector2(0, 0.35f);
        numRt.anchorMax = new Vector2(1, 1f);
        numRt.offsetMin = Vector2.zero;
        numRt.offsetMax = Vector2.zero;

        GameObject starsGo = new GameObject("Stars");
        starsGo.transform.SetParent(btnGo.transform, false);
        RectTransform starsRt = starsGo.AddComponent<RectTransform>();
        starsRt.anchorMin = new Vector2(0, 0f);
        starsRt.anchorMax = new Vector2(1, 0.35f);
        starsRt.offsetMin = Vector2.zero;
        starsRt.offsetMax = Vector2.zero;

        HorizontalLayoutGroup hlg = starsGo.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.spacing = 2;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;

        StarDisplay starDisplay = starsGo.AddComponent<StarDisplay>();
        starDisplay.starImages = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            GameObject starGo = new GameObject("Star" + i);
            starGo.transform.SetParent(starsGo.transform, false);
            Image starImg = starGo.AddComponent<Image>();
            starImg.sprite = PieceSpriteFactory.GetStarSprite(false);
            starImg.preserveAspect = true;
            RectTransform sRt = starImg.rectTransform;
            sRt.sizeDelta = new Vector2(34, 34);
            starDisplay.starImages[i] = starImg;
        }

        GameObject lockGo = new GameObject("Lock");
        lockGo.transform.SetParent(btnGo.transform, false);
        Image lockImg = lockGo.AddComponent<Image>();
        lockImg.sprite = PieceSpriteFactory.GetLockSprite();
        lockImg.preserveAspect = true;
        lockImg.color = new Color(1f, 1f, 1f, 0.85f);
        RectTransform lockRt = lockImg.rectTransform;
        lockRt.anchorMin = new Vector2(0.5f, 0.5f);
        lockRt.anchorMax = new Vector2(0.5f, 0.5f);
        lockRt.pivot = new Vector2(0.5f, 0.5f);
        lockRt.sizeDelta = new Vector2(90, 90);
        lockRt.anchoredPosition = Vector2.zero;

        lb.numberText = numTmp;
        lb.starDisplay = starDisplay;
        lb.lockIcon = lockGo;
        lb.button = button;
        lb.Setup(level.levelNumber, stars, unlocked, OnLevelChosen);
    }

    private void OnLevelChosen(int levelNumber)
    {
        GameSession.RequestedLevelNumber = levelNumber;
        TransitionManager.Instance.LoadScene(gameSceneName);
    }

    private void OnBack()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayBack();
        TransitionManager.Instance.LoadScene(menuSceneName);
    }
}
