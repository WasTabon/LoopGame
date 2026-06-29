using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class Iteration9Setup
{
    private const string GameScenePath = "Assets/LoopPuzzle/Scenes/Game.unity";

    [MenuItem("Tools/Loop Puzzle/Setup Iteration 9")]
    public static void Setup()
    {
        Scene scene = EditorSceneManager.OpenScene(GameScenePath);

        WinPopup winPopup = FindInSceneIncludingInactive<WinPopup>(scene);
        if (winPopup == null)
        {
            Debug.LogError("WinPopup not found in Game scene. Run earlier setups first.");
            return;
        }

        UpgradeWinPopupStars(winPopup);

        EditorUtility.SetDirty(winPopup);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("Iteration 9 setup complete: win popup stars upgraded to sprite images.");
    }

    private static T FindInSceneIncludingInactive<T>(Scene scene) where T : Component
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

    private static void UpgradeWinPopupStars(WinPopup winPopup)
    {
        StarDisplay sd = winPopup.starDisplay;
        if (sd == null)
        {
            Debug.LogWarning("WinPopup has no StarDisplay; skipping star upgrade.");
            return;
        }

        if (sd.starImages != null && sd.starImages.Length > 0 && sd.starImages[0] != null)
        {
            for (int i = 0; i < sd.starImages.Length; i++)
            {
                if (sd.starImages[i] != null)
                {
                    sd.starImages[i].sprite = PieceSpriteFactory.GetStarSprite(false);
                    sd.starImages[i].preserveAspect = true;
                }
            }
            return;
        }

        Transform row = sd.transform;

        if (sd.stars != null)
        {
            foreach (TextMeshProUGUI t in sd.stars)
            {
                if (t != null) Object.DestroyImmediate(t.gameObject);
            }
        }

        sd.starImages = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            GameObject starGo = new GameObject("StarImage" + i);
            starGo.transform.SetParent(row, false);
            Image img = starGo.AddComponent<Image>();
            img.sprite = PieceSpriteFactory.GetStarSprite(false);
            img.preserveAspect = true;
            RectTransform rt = img.rectTransform;
            rt.sizeDelta = new Vector2(110, 110);
            sd.starImages[i] = img;
        }
        sd.stars = new TextMeshProUGUI[0];
    }
}
