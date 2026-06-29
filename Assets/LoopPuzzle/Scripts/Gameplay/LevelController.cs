using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    public GridManager gridManager;
    public PieceInput pieceInput;
    public HUDController hud;
    public WinPopup winPopup;
    public LoopFlowAnimator flowAnimator;
    public LevelDatabase database;
    public HintSystem hintSystem;
    public string menuSceneName = "MainMenu";

    private LevelData currentLevel;
    private int moves;
    private bool levelComplete;
    private bool hintUsed;
    private readonly MoveHistory history = new MoveHistory();

    public LevelData CurrentLevel => currentLevel;

    public System.Action<LevelData> OnLevelLoaded;
    public System.Action<int, int> OnPieceRotated;
    public System.Action<int, int, int, int> OnPieceDragged;

    private void Awake()
    {
        if (gridManager != null)
        {
            gridManager.buildOnStart = false;
        }
    }

    private void Start()
    {
        ResolveStartingLevel();
        LoadCurrentLevel();
    }

    private void ResolveStartingLevel()
    {
        if (database != null && database.Count > 0)
        {
            LevelData requested = database.GetByLevelNumber(GameSession.RequestedLevelNumber);
            currentLevel = requested != null ? requested : database.GetByIndex(0);
        }
        else
        {
            currentLevel = gridManager.currentLevel;
        }
    }

    public void LoadCurrentLevel()
    {
        Debug.Assert(currentLevel != null, "No level resolved to load!");

        gridManager.currentLevel = currentLevel;
        gridManager.BuildLevel(currentLevel);

        moves = 0;
        levelComplete = false;
        hintUsed = false;
        history.Clear();

        hud.SetLevel(currentLevel.levelNumber);
        hud.SetMovesInstant(0);
        hud.SetUndoEnabled(false);

        pieceInput.SetInputLocked(false);

        OnLevelLoaded?.Invoke(currentLevel);
    }

    public void RegisterMove()
    {
        if (levelComplete) return;
        moves++;
        hud.AnimateMoves(moves);
    }

    public void RecordRotation(int x, int y)
    {
        if (levelComplete) return;
        history.RecordRotation(x, y);
        hud.SetUndoEnabled(history.HasMoves);
        OnPieceRotated?.Invoke(x, y);
    }

    public void RecordDrag(int fromX, int fromY, int toX, int toY)
    {
        if (levelComplete) return;
        history.RecordDrag(fromX, fromY, toX, toY);
        hud.SetUndoEnabled(history.HasMoves);
        OnPieceDragged?.Invoke(fromX, fromY, toX, toY);
    }

    public void Undo()
    {
        if (levelComplete) return;
        MoveRecord record = history.Pop();
        if (record == null) return;

        if (SoundManager.Instance != null) SoundManager.Instance.PlayBack();
        if (HapticManager.Instance != null) HapticManager.Instance.LightTap();

        if (record.type == MoveType.Rotation)
        {
            Cell cell = gridManager.GetCell(record.fromX, record.fromY);
            if (cell != null && cell.currentPiece != null)
            {
                cell.currentPiece.RotateBack();
            }
        }
        else
        {
            Cell toCell = gridManager.GetCell(record.toX, record.toY);
            Cell fromCell = gridManager.GetCell(record.fromX, record.fromY);
            if (toCell != null && fromCell != null && toCell.currentPiece != null)
            {
                gridManager.MovePiece(toCell, fromCell);
                Vector3 dest = gridManager.GridToWorld(fromCell.gridX, fromCell.gridY);
                fromCell.currentPiece.SnapTo(dest);
            }
        }

        if (moves > 0)
        {
            moves--;
            hud.AnimateMoves(moves);
        }

        hud.SetUndoEnabled(history.HasMoves);
    }

    public void RequestHint()
    {
        if (levelComplete) return;
        if (hintSystem == null) return;

        bool shown = hintSystem.ShowHint(currentLevel);
        if (shown)
        {
            hintUsed = true;
            if (SoundManager.Instance != null) SoundManager.Instance.PlayClick();
        }
    }

    public void MarkHintUsed()
    {
        hintUsed = true;
    }

    public void CheckForWin()
    {
        if (levelComplete) return;

        int required = currentLevel != null ? currentLevel.requiredLoops : 1;
        LoopResult result = LoopValidator.Validate(gridManager, gridManager.StartCell, required);
        if (result.isLoopClosed)
        {
            HandleWin();
        }
    }

    private int CalculateStars()
    {
        int stars = 1;
        if (!hintUsed) stars++;
        if (currentLevel != null && moves <= currentLevel.parMoves) stars++;
        return Mathf.Clamp(stars, 1, 3);
    }

    private void HandleWin()
    {
        levelComplete = true;
        pieceInput.SetInputLocked(true);
        hud.SetUndoEnabled(false);

        int stars = CalculateStars();

        if (ProgressManager.Instance != null)
        {
            ProgressManager.Instance.RecordResult(currentLevel.levelNumber, stars, moves);
        }

        if (SoundManager.Instance != null) SoundManager.Instance.PlayWin();
        if (HapticManager.Instance != null) HapticManager.Instance.MediumTap();

        if (flowAnimator != null)
        {
            List<Cell> orderedCells = LoopPathBuilder.BuildOrderedCells(gridManager, gridManager.StartCell);
            flowAnimator.Play(orderedCells, () => ShowWinPopup(stars));
        }
        else
        {
            DOVirtual.DelayedCall(0.4f, () => ShowWinPopup(stars));
        }
    }

    private void ShowWinPopup(int stars)
    {
        bool hasNext = database != null && database.GetNext(currentLevel) != null;
        winPopup.ShowWin(moves, stars, hasNext, OnRestart, OnNext, OnHome);
    }

    private void OnRestart()
    {
        LoadCurrentLevel();
    }

    private void OnNext()
    {
        if (database != null)
        {
            LevelData next = database.GetNext(currentLevel);
            if (next != null)
            {
                currentLevel = next;
                GameSession.RequestedLevelNumber = next.levelNumber;
                LoadCurrentLevel();
                return;
            }
        }
        OnHome();
    }

    private void OnHome()
    {
        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.LoadScene(menuSceneName);
        }
    }

    public void RestartLevel()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayBack();
        LoadCurrentLevel();
    }
}
