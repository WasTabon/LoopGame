using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameCompletePopup : MonoBehaviour
{
    public PopupBase popup;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI summaryText;
    public Button menuButton;

    private System.Action onMenu;

    private void OnEnable()
    {
        if (menuButton != null)
        {
            menuButton.onClick.RemoveListener(HandleMenu);
            menuButton.onClick.AddListener(HandleMenu);
        }
    }

    private void OnDisable()
    {
        if (menuButton != null) menuButton.onClick.RemoveListener(HandleMenu);
    }

    public void ShowComplete(int totalStars, int maxStars, System.Action menuCallback)
    {
        onMenu = menuCallback;

        if (titleText != null) titleText.text = "All Levels Complete!";
        if (summaryText != null) summaryText.text = "Stars collected: " + totalStars + " / " + maxStars;

        popup.Show();

        if (SoundManager.Instance != null) SoundManager.Instance.PlayWin();
        if (HapticManager.Instance != null) HapticManager.Instance.HeavyTap();
    }

    private void HandleMenu()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayClick();
        popup.Hide(() => onMenu?.Invoke());
    }
}
