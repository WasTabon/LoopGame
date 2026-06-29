using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WinPopup : MonoBehaviour
{
    public PopupBase popup;
    public TextMeshProUGUI movesText;
    public StarDisplay starDisplay;
    public Button restartButton;
    public Button nextButton;
    public Button homeButton;
    public TextMeshProUGUI nextButtonLabel;

    private System.Action onRestart;
    private System.Action onNext;
    private System.Action onHome;

    private void OnEnable()
    {
        restartButton.onClick.RemoveListener(HandleRestart);
        restartButton.onClick.AddListener(HandleRestart);
        nextButton.onClick.RemoveListener(HandleNext);
        nextButton.onClick.AddListener(HandleNext);
        if (homeButton != null)
        {
            homeButton.onClick.RemoveListener(HandleHome);
            homeButton.onClick.AddListener(HandleHome);
        }
    }

    private void OnDisable()
    {
        restartButton.onClick.RemoveListener(HandleRestart);
        nextButton.onClick.RemoveListener(HandleNext);
        if (homeButton != null) homeButton.onClick.RemoveListener(HandleHome);
    }

    public void ShowWin(int moves, int stars, bool hasNext,
        System.Action restartCallback, System.Action nextCallback, System.Action homeCallback)
    {
        ShowWin(moves, stars, hasNext, false, restartCallback, nextCallback, homeCallback);
    }

    public void ShowWin(int moves, int stars, bool hasNext, bool isFinalLevel,
        System.Action restartCallback, System.Action nextCallback, System.Action homeCallback)
    {
        onRestart = restartCallback;
        onNext = nextCallback;
        onHome = homeCallback;

        movesText.text = "Moves: " + moves;

        bool showNext = hasNext || isFinalLevel;
        nextButton.gameObject.SetActive(showNext);
        if (nextButtonLabel != null)
        {
            nextButtonLabel.text = isFinalLevel ? "FINISH" : "NEXT";
        }

        popup.Show();

        if (starDisplay != null) starDisplay.RevealStars(stars);
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

    private void HandleHome()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayClick();
        popup.Hide(() => onHome?.Invoke());
    }
}
