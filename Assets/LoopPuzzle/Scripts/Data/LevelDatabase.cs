using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelDatabase", menuName = "Loop Puzzle/Level Database")]
public class LevelDatabase : ScriptableObject
{
    public List<LevelData> levels = new List<LevelData>();

    public int Count => levels.Count;

    public LevelData GetByIndex(int index)
    {
        if (index < 0 || index >= levels.Count) return null;
        return levels[index];
    }

    public LevelData GetByLevelNumber(int levelNumber)
    {
        foreach (LevelData level in levels)
        {
            if (level != null && level.levelNumber == levelNumber) return level;
        }
        return null;
    }

    public LevelData GetNext(LevelData current)
    {
        int idx = levels.IndexOf(current);
        if (idx < 0 || idx + 1 >= levels.Count) return null;
        return levels[idx + 1];
    }

    public int GetMaxLevelNumber()
    {
        int max = 0;
        foreach (LevelData level in levels)
        {
            if (level != null && level.levelNumber > max) max = level.levelNumber;
        }
        return max;
    }
}
