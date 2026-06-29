using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPopup : MonoBehaviour
{
    public PopupBase popup;
    public Button closeButton;
    public Button sfxToggle;
    public Button musicToggle;
    public Button hapticsToggle;
    public Button resetButton;
    public TextMeshProUGUI sfxLabel;
    public TextMeshProUGUI musicLabel;
    public TextMeshProUGUI hapticsLabel;
    public TextMeshProUGUI resetLabel;

    public LevelDatabase database;

    private static readonly Color OnColor = new Color(0.290f, 0.565f, 0.886f, 1f);
    private static readonly Color OffColor = new Color(0.25f, 0.25f, 0.33f, 1f);

    private bool resetArmed;

    private void OnEnable()
    {
        BindButtons();
        RefreshLabels();
    }

    private void BindButtons()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(OnClose);
            closeButton.onClick.AddListener(OnClose);
        }
        if (sfxToggle != null)
        {
            sfxToggle.onClick.RemoveListener(OnToggleSfx);
            sfxToggle.onClick.AddListener(OnToggleSfx);
        }
        if (musicToggle != null)
        {
            musicToggle.onClick.RemoveListener(OnToggleMusic);
            musicToggle.onClick.AddListener(OnToggleMusic);
        }
        if (hapticsToggle != null)
        {
            hapticsToggle.onClick.RemoveListener(OnToggleHaptics);
            hapticsToggle.onClick.AddListener(OnToggleHaptics);
        }
        if (resetButton != null)
        {
            resetButton.onClick.RemoveListener(OnReset);
            resetButton.onClick.AddListener(OnReset);
        }
    }

    public void Open()
    {
        resetArmed = false;
        popup.Show();
        RefreshLabels();
    }

    private void RefreshLabels()
    {
        if (SoundManager.Instance != null)
        {
            SetToggleVisual(sfxToggle, sfxLabel, "Sound", !SoundManager.Instance.SfxMuted);
            SetToggleVisual(musicToggle, musicLabel, "Music", !SoundManager.Instance.MusicMuted);
        }
        if (HapticManager.Instance != null)
        {
            SetToggleVisual(hapticsToggle, hapticsLabel, "Vibration", HapticManager.Instance.HapticsEnabled);
        }
        if (resetLabel != null) resetLabel.text = resetArmed ? "Tap again to confirm" : "Reset Progress";
    }

    private void SetToggleVisual(Button button, TextMeshProUGUI label, string name, bool isOn)
    {
        if (button != null)
        {
            Image img = button.GetComponent<Image>();
            if (img != null) img.color = isOn ? OnColor : OffColor;
        }
        if (label != null) label.text = name + ": " + (isOn ? "On" : "Off");
    }

    private void OnToggleSfx()
    {
        if (SoundManager.Instance == null) return;
        SoundManager.Instance.SetSfxMuted(!SoundManager.Instance.SfxMuted);
        SoundManager.Instance.PlayClick();
        RefreshLabels();
    }

    private void OnToggleMusic()
    {
        if (SoundManager.Instance == null) return;
        SoundManager.Instance.SetMusicMuted(!SoundManager.Instance.MusicMuted);
        RefreshLabels();
    }

    private void OnToggleHaptics()
    {
        if (HapticManager.Instance == null) return;
        HapticManager.Instance.SetHapticsEnabled(!HapticManager.Instance.HapticsEnabled);
        HapticManager.Instance.LightTap();
        RefreshLabels();
    }

    private void OnReset()
    {
        if (!resetArmed)
        {
            resetArmed = true;
            RefreshLabels();
            return;
        }

        int total = database != null ? database.GetMaxLevelNumber() : 25;
        if (ProgressManager.Instance != null)
        {
            ProgressManager.Instance.ResetAllProgress(total);
        }
        resetArmed = false;
        RefreshLabels();
        if (SoundManager.Instance != null) SoundManager.Instance.PlayBack();
    }

    private void OnClose()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayBack();
        popup.Hide();
    }
}
