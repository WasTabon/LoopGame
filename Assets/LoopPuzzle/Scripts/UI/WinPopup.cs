using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WinPopup : MonoBehaviour
{
    public PopupBase popup;
    public TextMeshProUGUI movesText;
    public Button restartButton;
    public Button nextButton;

    private System.Action onRestart;
    private System.Action onNext;

    private void OnEnable()
    {
        restartButton.onClick.RemoveListener(HandleRestart);
        restartButton.onClick.AddListener(HandleRestart);
        nextButton.onClick.RemoveListener(HandleNext);
        nextButton.onClick.AddListener(HandleNext);
    }

    private void OnDisable()
    {
        restartButton.onClick.RemoveListener(HandleRestart);
        nextButton.onClick.RemoveListener(HandleNext);
    }

    public void ShowWin(int moves, System.Action restartCallback, System.Action nextCallback)
    {
        onRestart = restartCallback;
        onNext = nextCallback;
        movesText.text = "Moves: " + moves;
        popup.Show();
    }

    private void HandleRestart()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayClick();
        popup.Hide(() => onRestart?.Invoke());
    }

    private void HandleNext()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayClick();
        popup.Hide(() => onNext?.Invoke());
    }
}
