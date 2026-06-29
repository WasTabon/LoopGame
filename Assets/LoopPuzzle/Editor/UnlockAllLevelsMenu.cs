using UnityEditor;
using UnityEngine;

public class UnlockAllLevelsMenu
{
    private const string DatabasePath = "Assets/LoopPuzzle/Data/LevelDatabase.asset";
    private const string UnlockedKey = "max_unlocked_level";

    [MenuItem("Tools/Loop Puzzle/Unlock All Levels")]
    public static void UnlockAll()
    {
        LevelDatabase db = AssetDatabase.LoadAssetAtPath<LevelDatabase>(DatabasePath);
        int total = db != null ? db.GetMaxLevelNumber() : 25;

        PlayerPrefs.SetInt(UnlockedKey, total);
        PlayerPrefs.Save();

        Debug.Log($"<color=#7CFC00>Unlocked all {total} levels.</color> (max_unlocked_level = {total})");
    }

    [MenuItem("Tools/Loop Puzzle/Lock All Levels (reset to 1)")]
    public static void LockAll()
    {
        LevelDatabase db = AssetDatabase.LoadAssetAtPath<LevelDatabase>(DatabasePath);
        int total = db != null ? db.GetMaxLevelNumber() : 25;

        for (int i = 1; i <= total; i++)
        {
            PlayerPrefs.DeleteKey("level_stars_" + i);
            PlayerPrefs.DeleteKey("level_best_" + i);
        }
        PlayerPrefs.SetInt(UnlockedKey, 1);
        PlayerPrefs.Save();

        Debug.Log("All levels locked and progress reset (max_unlocked_level = 1).");
    }
}
