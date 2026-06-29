using UnityEngine;

public class HapticManager : MonoBehaviour
{
    public static HapticManager Instance;

    private const string HapticKey = "haptics_enabled";

    private bool hapticsEnabled;
    public bool HapticsEnabled => hapticsEnabled;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        hapticsEnabled = PlayerPrefs.GetInt(HapticKey, 1) == 1;
    }

    public void LightTap()
    {
        if (!hapticsEnabled) return;
#if UNITY_IOS || UNITY_ANDROID
        Handheld.Vibrate();
#endif
    }

    public void MediumTap()
    {
        if (!hapticsEnabled) return;
#if UNITY_IOS || UNITY_ANDROID
        Handheld.Vibrate();
#endif
    }

    public void HeavyTap()
    {
        if (!hapticsEnabled) return;
#if UNITY_IOS || UNITY_ANDROID
        Handheld.Vibrate();
#endif
    }

    public void SetHapticsEnabled(bool enabled)
    {
        hapticsEnabled = enabled;
        PlayerPrefs.SetInt(HapticKey, enabled ? 1 : 0);
        PlayerPrefs.Save();
    }
}
