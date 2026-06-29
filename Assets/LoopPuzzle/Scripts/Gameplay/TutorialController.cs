using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialController : MonoBehaviour
{
    public LevelController levelController;
    public GridManager gridManager;
    public TutorialBanner banner;

    private GameObject pointerObject;
    private SpriteRenderer pointerRenderer;
    private Coroutine activeRoutine;
    private bool anyRotationHappened;

    private void Start()
    {
        if (levelController != null)
        {
            levelController.OnLevelLoaded += HandleLevelLoaded;
            levelController.OnPieceRotated += HandlePieceRotated;
            levelController.OnPieceDragged += HandlePieceDragged;

            if (levelController.CurrentLevel != null)
            {
                HandleLevelLoaded(levelController.CurrentLevel);
            }
        }
    }

    private void OnDestroy()
    {
        if (levelController != null)
        {
            levelController.OnLevelLoaded -= HandleLevelLoaded;
            levelController.OnPieceRotated -= HandlePieceRotated;
            levelController.OnPieceDragged -= HandlePieceDragged;
        }
    }

    private void HandleLevelLoaded(LevelData level)
    {
        StopActive();
        HidePointer();
        if (banner != null) banner.Hide();

        if (level == null) return;
        if (ProgressManager.Instance == null) return;

        if (level.levelNumber == 1 && !ProgressManager.Instance.IsTutorialSeen("rotate"))
        {
            activeRoutine = StartCoroutine(RotateTutorial(level));
        }
        else if (level.levelNumber == 6 && !ProgressManager.Instance.IsTutorialSeen("drag"))
        {
            activeRoutine = StartCoroutine(DragTutorial(level));
        }
        else
        {
            ShowWorldBanner(level);
        }
    }

    private void ShowWorldBanner(LevelData level)
    {
        if (banner == null) return;

        string message = null;
        if (level.levelNumber == 11) message = "New: some pieces are fixed and cannot move";
        else if (level.levelNumber == 16) message = "New: form two separate loops";
        else if (level.levelNumber == 21) message = "Advanced: bigger boards and junctions";

        if (message != null)
        {
            banner.ShowTimed(message, 3.5f);
        }
    }

    private IEnumerator RotateTutorial(LevelData level)
    {
        if (banner != null) banner.Show("Tap a piece to rotate it");

        yield return new WaitForSeconds(0.6f);

        int safety = 200;
        while (safety-- > 0)
        {
            Cell wrong = FindWrongRotationCell(level);
            if (wrong == null) break;

            ShowPointerAt(wrong);
            yield return WaitForRotationOf(wrong);
            HidePointer();
            yield return new WaitForSeconds(0.15f);
        }

        HidePointer();
        if (banner != null) banner.ShowTimed("Great! Connect every piece into a loop", 3f);
        ProgressManager.Instance.MarkTutorialSeen("rotate");
    }

    private IEnumerator DragTutorial(LevelData level)
    {
        if (banner != null) banner.Show("Drag the highlighted piece to the empty cell");

        yield return new WaitForSeconds(0.6f);

        Cell displaced = FindDisplacedPiece(level);
        Cell target = FindEmptySolutionCell(level);

        if (displaced != null && target != null)
        {
            displaced.currentPiece.HighlightPulse();
            target.ShowAsValidTarget();
            yield return AnimatePointerDrag(displaced, target);
            target.ClearHighlight();
        }

        HidePointer();
        if (banner != null) banner.ShowTimed("Now rotate pieces to finish the loop", 3f);
        ProgressManager.Instance.MarkTutorialSeen("drag");
    }

    private Cell FindWrongRotationCell(LevelData level)
    {
        if (level.solution == null) return null;
        foreach (SolutionEntry entry in level.solution)
        {
            Cell cell = gridManager.GetCell(entry.tx, entry.ty);
            if (cell == null || cell.currentPiece == null) continue;
            PathPiece piece = cell.currentPiece;
            if (piece.pieceType != entry.pieceType) continue;
            if (!piece.canRotate) continue;
            if (piece.rotationSteps != entry.rotationSteps) return cell;
        }
        return null;
    }

    private Cell FindDisplacedPiece(LevelData level)
    {
        HashSet<Vector2Int> solutionCells = new HashSet<Vector2Int>();
        if (level.solution != null)
        {
            foreach (SolutionEntry entry in level.solution)
            {
                solutionCells.Add(new Vector2Int(entry.tx, entry.ty));
            }
        }

        Cell found = null;
        gridManager.ForEachCell(cell =>
        {
            if (found != null) return;
            if (cell.currentPiece == null) return;
            if (!cell.currentPiece.canRotate) return;
            if (!solutionCells.Contains(new Vector2Int(cell.gridX, cell.gridY)))
            {
                found = cell;
            }
        });
        return found;
    }

    private Cell FindEmptySolutionCell(LevelData level)
    {
        if (level.solution == null) return null;
        foreach (SolutionEntry entry in level.solution)
        {
            Cell cell = gridManager.GetCell(entry.tx, entry.ty);
            if (cell != null && cell.currentPiece == null) return cell;
        }
        return null;
    }

    private IEnumerator WaitForRotationOf(Cell cell)
    {
        anyRotationHappened = false;
        int startSteps = cell.currentPiece != null ? cell.currentPiece.rotationSteps : -1;
        float timeout = 12f;
        while (timeout > 0f)
        {
            if (cell.currentPiece == null) yield break;
            if (cell.currentPiece.rotationSteps != startSteps) yield break;
            if (anyRotationHappened) yield break;
            timeout -= Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator AnimatePointerDrag(Cell from, Cell to)
    {
        EnsurePointer();
        Vector3 start = gridManager.GridToWorld(from.gridX, from.gridY) + new Vector3(0, 0, -1f);
        Vector3 end = gridManager.GridToWorld(to.gridX, to.gridY) + new Vector3(0, 0, -1f);

        pointerObject.transform.localScale = Vector3.one * gridManager.CellWorldSize * 0.5f;
        pointerObject.SetActive(true);

        float elapsed = 0f;
        bool done = false;
        while (!done)
        {
            if (to.currentPiece != null) { done = true; break; }

            elapsed += Time.deltaTime;
            float t = Mathf.PingPong(elapsed * 0.6f, 1f);
            float eased = Mathf.SmoothStep(0f, 1f, t);
            pointerObject.transform.position = Vector3.Lerp(start, end, eased);
            yield return null;
        }
    }

    private void ShowPointerAt(Cell cell)
    {
        EnsurePointer();
        Vector3 pos = gridManager.GridToWorld(cell.gridX, cell.gridY) + new Vector3(0, 0, -1f);
        pointerObject.transform.position = pos;
        pointerObject.transform.localScale = Vector3.one * gridManager.CellWorldSize * 0.5f;
        pointerObject.SetActive(true);

        StopPointerPulse();
        pointerPulse = StartCoroutine(PulsePointer());
    }

    private Coroutine pointerPulse;

    private IEnumerator PulsePointer()
    {
        float t = 0f;
        while (true)
        {
            t += Time.deltaTime;
            if (pointerObject != null && pointerObject.activeSelf)
            {
                float s = gridManager.CellWorldSize * (0.45f + 0.08f * Mathf.Sin(t * 7f));
                pointerObject.transform.localScale = Vector3.one * s;
            }
            yield return null;
        }
    }

    private void StopPointerPulse()
    {
        if (pointerPulse != null)
        {
            StopCoroutine(pointerPulse);
            pointerPulse = null;
        }
    }

    private void HidePointer()
    {
        StopPointerPulse();
        if (pointerObject != null) pointerObject.SetActive(false);
    }

    private void EnsurePointer()
    {
        if (pointerObject != null) return;
        pointerObject = new GameObject("TutorialPointer");
        pointerRenderer = pointerObject.AddComponent<SpriteRenderer>();
        pointerRenderer.sprite = PieceSpriteFactory.GetTapPointerSprite();
        pointerRenderer.sortingOrder = 25;
        pointerObject.SetActive(false);
    }

    private void HandlePieceRotated(int x, int y)
    {
        anyRotationHappened = true;
    }

    private void HandlePieceDragged(int fromX, int fromY, int toX, int toY)
    {
    }

    private void StopActive()
    {
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
            activeRoutine = null;
        }
        StopPointerPulse();
    }
}
