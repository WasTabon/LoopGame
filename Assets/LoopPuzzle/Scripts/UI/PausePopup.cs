using UnityEngine;
using UnityEngine.UI;

public class PausePopup : MonoBehaviour
{
    public PopupBase popup;
    public Button resumeButton;
    public Button restartButton;
    public Button menuButton;

    private System.Action onResume;
    private System.Action onRestart;
    private System.Action onMenu;

    private void OnEnable()
    {
        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveListener(HandleResume);
            resumeButton.onClick.AddListener(HandleResume);
        }
        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(HandleRestart);
            restartButton.onClick.AddListener(HandleRestart);
        }
        if (menuButton != null)
        {
            menuButton.onClick.RemoveListener(HandleMenu);
            menuButton.onClick.AddListener(HandleMenu);
        }
    }

    private void OnDisable()
    {
        if (resumeButton != null) resumeButton.onClick.RemoveListener(HandleResume);
        if (restartButton != null) restartButton.onClick.RemoveListener(HandleRestart);
        if (menuButton != null) menuButton.onClick.RemoveListener(HandleMenu);
    }

    public void Open(System.Action resumeCallback, System.Action restartCallback, System.Action menuCallback)
    {
        onResume = resumeCallback;
        onRestart = restartCallback;
        onMenu = menuCallback;
        popup.Show();
        if (SoundManager.Instance != null) SoundManager.Instance.PlayClick();
    }

    private void HandleResume()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayBack();
        popup.Hide(() => onResume?.Invoke());
    }

    private void HandleRestart()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayClick();
        popup.Hide(() => onRestart?.Invoke());
    }

    private void HandleMenu()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayBack();
        popup.Hide(() => onMenu?.Invoke());
    }
}
