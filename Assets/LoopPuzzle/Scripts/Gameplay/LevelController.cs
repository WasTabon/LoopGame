using DG.Tweening;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    public GridManager gridManager;
    public PieceInput pieceInput;
    public HUDController hud;
    public WinPopup winPopup;

    private int moves;
    private bool levelComplete;

    private void Awake()
    {
        if (gridManager != null)
        {
            gridManager.buildOnStart = false;
        }
    }

    private void Start()
    {
        LoadCurrentLevel();
    }

    public void LoadCurrentLevel()
    {
        Debug.Assert(gridManager.currentLevel != null, "No current level assigned to GridManager!");

        gridManager.BuildLevel(gridManager.currentLevel);

        moves = 0;
        levelComplete = false;

        hud.SetLevel(gridManager.currentLevel.levelNumber);
        hud.SetMovesInstant(0);

        pieceInput.SetInputLocked(false);
    }

    public void RegisterMove()
    {
        if (levelComplete) return;
        moves++;
        hud.AnimateMoves(moves);
    }

    public void CheckForWin()
    {
        if (levelComplete) return;
        if (gridManager.StartCell == null) return;

        LoopResult result = LoopValidator.Validate(gridManager, gridManager.StartCell);
        if (result.isLoopClosed)
        {
            HandleWin();
        }
    }

    private void HandleWin()
    {
        levelComplete = true;
        pieceInput.SetInputLocked(true);

        if (SoundManager.Instance != null) SoundManager.Instance.PlayWin();
        if (HapticManager.Instance != null) HapticManager.Instance.MediumTap();

        DOVirtual.DelayedCall(0.4f, () =>
        {
            winPopup.ShowWin(moves, OnRestart, OnNext);
        });
    }

    private void OnRestart()
    {
        LoadCurrentLevel();
    }

    private void OnNext()
    {
        LoadCurrentLevel();
    }

    public void RestartLevel()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayBack();
        LoadCurrentLevel();
    }
}
