using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI movesText;
    public Button undoButton;
    public Button restartButton;
    public Button hintButton;
    public Button pauseButton;
    public LevelController levelController;

    private int displayedMoves;
    private Tween counterTween;

    private void Start()
    {
        if (undoButton != null) undoButton.onClick.AddListener(OnUndo);
        if (restartButton != null) restartButton.onClick.AddListener(OnRestart);
        if (hintButton != null) hintButton.onClick.AddListener(OnHint);
        if (pauseButton != null) pauseButton.onClick.AddListener(OnPause);
    }

    public void SetLevel(int levelNumber)
    {
        levelText.text = "Level " + levelNumber;
    }

    public void SetMovesInstant(int moves)
    {
        displayedMoves = moves;
        movesText.text = "Moves: " + moves;
    }

    public void AnimateMoves(int newMoves)
    {
        counterTween?.Kill();
        int from = displayedMoves;
        counterTween = DOTween.To(() => from, v =>
        {
            displayedMoves = v;
            movesText.text = "Moves: " + v;
        }, newMoves, 0.25f).SetEase(Ease.OutQuad);

        movesText.transform.DOKill();
        movesText.transform.localScale = Vector3.one;
        movesText.transform.DOPunchScale(Vector3.one * 0.15f, 0.25f, 6, 0.6f);
        displayedMoves = newMoves;
    }

    public void SetUndoEnabled(bool enabled)
    {
        if (undoButton != null) undoButton.interactable = enabled;
    }

    private void OnUndo()
    {
        if (levelController != null) levelController.Undo();
    }

    private void OnRestart()
    {
        if (levelController != null) levelController.RestartLevel();
    }

    private void OnHint()
    {
        if (levelController != null) levelController.RequestHint();
    }

    private void OnPause()
    {
        if (levelController != null) levelController.TogglePause();
    }
}
