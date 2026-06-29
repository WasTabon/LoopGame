using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintSystem : MonoBehaviour
{
    public GridManager gridManager;

    private GameObject arrowObject;
    private SpriteRenderer arrowRenderer;
    private Coroutine activeHint;

    public bool ShowHint(LevelData level)
    {
        if (level == null || level.solution == null || level.solution.Length == 0) return false;

        Cell targetCell = FindWrongRotationCell(level, out int neededSteps);
        if (targetCell != null)
        {
            ShowRotationHint(targetCell, neededSteps);
            return true;
        }

        Cell misplaced = FindMisplacedCell(level);
        if (misplaced != null)
        {
            ShowMoveHint(misplaced, level);
            return true;
        }

        return false;
    }

    private Cell FindWrongRotationCell(LevelData level, out int neededSteps)
    {
        neededSteps = 0;
        foreach (SolutionEntry entry in level.solution)
        {
            Cell cell = gridManager.GetCell(entry.tx, entry.ty);
            if (cell == null || cell.currentPiece == null) continue;

            PathPiece piece = cell.currentPiece;
            if (piece.pieceType != entry.pieceType) continue;
            if (!piece.canRotate) continue;

            if (piece.rotationSteps != entry.rotationSteps)
            {
                int diff = ((entry.rotationSteps - piece.rotationSteps) % 4 + 4) % 4;
                neededSteps = diff;
                return cell;
            }
        }
        return null;
    }

    private Cell FindMisplacedCell(LevelData level)
    {
        HashSet<Vector2Int> solutionCells = new HashSet<Vector2Int>();
        foreach (SolutionEntry entry in level.solution)
        {
            solutionCells.Add(new Vector2Int(entry.tx, entry.ty));
        }

        Cell misplaced = null;
        gridManager.ForEachCell(cell =>
        {
            if (misplaced != null) return;
            if (cell.currentPiece == null) return;
            if (!cell.currentPiece.canRotate) return;
            Vector2Int pos = new Vector2Int(cell.gridX, cell.gridY);
            if (!solutionCells.Contains(pos))
            {
                misplaced = cell;
            }
        });
        return misplaced;
    }

    private void ShowRotationHint(Cell cell, int steps)
    {
        StopActiveHint();
        cell.currentPiece.HighlightPulse();

        EnsureArrow();
        Vector3 pos = gridManager.GridToWorld(cell.gridX, cell.gridY);
        arrowObject.transform.position = pos + new Vector3(0, 0, -0.5f);
        arrowObject.transform.localScale = Vector3.one * gridManager.CellWorldSize * 0.7f;
        arrowObject.SetActive(true);
        arrowRenderer.color = new Color(0.961f, 0.651f, 0.137f, 1f);

        activeHint = StartCoroutine(PulseArrow());
    }

    private void ShowMoveHint(Cell cell, LevelData level)
    {
        StopActiveHint();
        cell.currentPiece.HighlightPulse();

        Cell emptyTarget = FindEmptySolutionCell(level);
        if (emptyTarget != null)
        {
            emptyTarget.ShowAsValidTarget();
        }

        activeHint = StartCoroutine(ClearMoveHintAfter(emptyTarget));
    }

    private Cell FindEmptySolutionCell(LevelData level)
    {
        foreach (SolutionEntry entry in level.solution)
        {
            Cell cell = gridManager.GetCell(entry.tx, entry.ty);
            if (cell != null && cell.currentPiece == null)
            {
                return cell;
            }
        }
        return null;
    }

    private IEnumerator PulseArrow()
    {
        float t = 0f;
        while (t < 2f)
        {
            t += Time.deltaTime;
            if (arrowObject != null)
            {
                float s = gridManager.CellWorldSize * (0.65f + 0.08f * Mathf.Sin(t * 8f));
                arrowObject.transform.localScale = Vector3.one * s;
            }
            yield return null;
        }
        if (arrowObject != null) arrowObject.SetActive(false);
        activeHint = null;
    }

    private IEnumerator ClearMoveHintAfter(Cell target)
    {
        yield return new WaitForSeconds(2f);
        if (target != null) target.ClearHighlight();
        activeHint = null;
    }

    private void StopActiveHint()
    {
        if (activeHint != null)
        {
            StopCoroutine(activeHint);
            activeHint = null;
        }
        if (arrowObject != null) arrowObject.SetActive(false);
    }

    private void EnsureArrow()
    {
        if (arrowObject != null) return;
        arrowObject = new GameObject("HintArrow");
        arrowRenderer = arrowObject.AddComponent<SpriteRenderer>();
        arrowRenderer.sprite = PieceSpriteFactory.GetRotationArrowSprite();
        arrowRenderer.sortingOrder = 20;
        arrowObject.SetActive(false);
    }
}
