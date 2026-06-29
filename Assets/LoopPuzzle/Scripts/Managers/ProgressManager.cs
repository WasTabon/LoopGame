using UnityEngine;

public class ProgressManager : MonoBehaviour
{
    public static ProgressManager Instance;

    private const string StarsKeyPrefix = "level_stars_";
    private const string BestKeyPrefix = "level_best_";
    private const string UnlockedKey = "max_unlocked_level";
    private const string TutorialKeyPrefix = "tutorial_seen_";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public int GetStars(int levelNumber)
    {
        return PlayerPrefs.GetInt(StarsKeyPrefix + levelNumber, 0);
    }

    public int GetTotalStars(int totalLevels)
    {
        int sum = 0;
        for (int i = 1; i <= totalLevels; i++)
        {
            sum += GetStars(i);
        }
        return sum;
    }

    public bool IsTutorialSeen(string tutorialId)
    {
        return PlayerPrefs.GetInt(TutorialKeyPrefix + tutorialId, 0) == 1;
    }

    public void MarkTutorialSeen(string tutorialId)
    {
        PlayerPrefs.SetInt(TutorialKeyPrefix + tutorialId, 1);
        PlayerPrefs.Save();
    }

    public int GetBestMoves(int levelNumber)
    {
        return PlayerPrefs.GetInt(BestKeyPrefix + levelNumber, -1);
    }

    public int GetMaxUnlockedLevel()
    {
        return PlayerPrefs.GetInt(UnlockedKey, 1);
    }

    public bool IsLevelUnlocked(int levelNumber)
    {
        return levelNumber <= GetMaxUnlockedLevel();
    }

    public void RecordResult(int levelNumber, int stars, int moves)
    {
        int prevStars = GetStars(levelNumber);
        if (stars > prevStars)
        {
            PlayerPrefs.SetInt(StarsKeyPrefix + levelNumber, stars);
        }

        int prevBest = GetBestMoves(levelNumber);
        if (prevBest < 0 || moves < prevBest)
        {
            PlayerPrefs.SetInt(BestKeyPrefix + levelNumber, moves);
        }

        int nextLevel = levelNumber + 1;
        if (nextLevel > GetMaxUnlockedLevel())
        {
            PlayerPrefs.SetInt(UnlockedKey, nextLevel);
        }

        PlayerPrefs.Save();
    }

    public void ResetAllProgress(int totalLevels)
    {
        for (int i = 1; i <= totalLevels; i++)
        {
            PlayerPrefs.DeleteKey(StarsKeyPrefix + i);
            PlayerPrefs.DeleteKey(BestKeyPrefix + i);
        }
        PlayerPrefs.DeleteKey(TutorialKeyPrefix + "rotate");
        PlayerPrefs.DeleteKey(TutorialKeyPrefix + "drag");
        PlayerPrefs.SetInt(UnlockedKey, 1);
        PlayerPrefs.Save();
    }
}
