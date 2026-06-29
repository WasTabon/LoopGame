using UnityEngine;
using UnityEngine.EventSystems;

public class PieceInput : MonoBehaviour
{
    public GridManager gridManager;
    public LevelController levelController;

    private Camera cam;
    private Vector2 pressPosition;
    private bool pressing;
    private bool inputLocked;
    private const float DragThreshold = 24f;

    private bool dragging;
    private PathPiece draggedPiece;
    private Cell sourceCell;
    private Cell hoverCell;

    private void Start()
    {
        cam = Camera.main;
        Debug.Assert(gridManager != null, "PieceInput has no GridManager assigned!");
    }

    public void SetInputLocked(bool locked)
    {
        inputLocked = locked;
        if (locked && dragging)
        {
            CancelDrag();
        }
    }

    private void Update()
    {
        if (inputLocked) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (IsPointerOverUI()) return;
            pressing = true;
            pressPosition = Input.mousePosition;
            sourceCell = FindCellAt(ScreenToWorld(Input.mousePosition));
        }
        else if (Input.GetMouseButton(0) && pressing)
        {
            float moved = Vector2.Distance(Input.mousePosition, pressPosition);

            if (!dragging && moved > DragThreshold)
            {
                TryBeginDrag();
            }

            if (dragging)
            {
                UpdateDrag();
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (!pressing) return;
            pressing = false;

            if (dragging)
            {
                EndDrag();
            }
            else
            {
                float moved = Vector2.Distance(Input.mousePosition, pressPosition);
                if (moved <= DragThreshold)
                {
                    HandleTap(ScreenToWorld(Input.mousePosition));
                }
            }
        }
    }

    private void TryBeginDrag()
    {
        if (sourceCell == null || sourceCell.currentPiece == null) return;

        PathPiece piece = sourceCell.currentPiece;
        if (!piece.canRotate) return;

        dragging = true;
        draggedPiece = piece;
        draggedPiece.Lift();

        if (SoundManager.Instance != null) SoundManager.Instance.PlayPickup();
        if (HapticManager.Instance != null) HapticManager.Instance.LightTap();

        HighlightValidTargets(true);
    }

    private void UpdateDrag()
    {
        Vector3 worldPos = ScreenToWorld(Input.mousePosition);
        draggedPiece.FollowTo(worldPos);

        Cell newHover = gridManager.GetCellAtWorld(worldPos);
        if (newHover != hoverCell)
        {
            RefreshHover(newHover);
        }
    }

    private void RefreshHover(Cell newHover)
    {
        if (hoverCell != null && IsValidTarget(hoverCell))
        {
            hoverCell.ShowAsValidTarget();
        }

        hoverCell = newHover;

        if (hoverCell != null && IsValidTarget(hoverCell))
        {
            hoverCell.ShowAsHoverTarget();
        }
    }

    private void EndDrag()
    {
        HighlightValidTargets(false);

        Cell target = hoverCell;

        if (target != null && IsValidTarget(target))
        {
            gridManager.MovePiece(sourceCell, target);
            Vector3 dest = gridManager.GridToWorld(target.gridX, target.gridY);
            draggedPiece.SnapTo(dest, OnMoveComplete);

            if (SoundManager.Instance != null) SoundManager.Instance.PlayDrop();
            if (HapticManager.Instance != null) HapticManager.Instance.LightTap();

            if (levelController != null) levelController.RegisterMove();
        }
        else
        {
            Vector3 home = gridManager.GridToWorld(sourceCell.gridX, sourceCell.gridY);
            draggedPiece.ReturnTo(home);

            if (SoundManager.Instance != null) SoundManager.Instance.PlayBack();
        }

        ClearDragState();
    }

    private void OnMoveComplete()
    {
        if (levelController != null)
        {
            levelController.CheckForWin();
        }
    }

    private void CancelDrag()
    {
        HighlightValidTargets(false);
        if (draggedPiece != null && sourceCell != null)
        {
            Vector3 home = gridManager.GridToWorld(sourceCell.gridX, sourceCell.gridY);
            draggedPiece.ReturnTo(home);
        }
        ClearDragState();
    }

    private void ClearDragState()
    {
        dragging = false;
        draggedPiece = null;
        hoverCell = null;
    }

    private bool IsValidTarget(Cell cell)
    {
        if (cell == null) return false;
        if (cell == sourceCell) return false;
        return cell.IsEmpty;
    }

    private void HighlightValidTargets(bool show)
    {
        gridManager.ForEachCell(cell =>
        {
            if (show && IsValidTarget(cell))
            {
                cell.ShowAsValidTarget();
            }
            else
            {
                cell.ClearHighlight();
            }
        });
    }

    private void HandleTap(Vector3 worldPos)
    {
        PathPiece piece = FindPieceAt(worldPos);
        if (piece == null) return;

        if (piece.canRotate)
        {
            piece.Rotate(OnRotationComplete);
            if (SoundManager.Instance != null) SoundManager.Instance.PlayClick();
            if (HapticManager.Instance != null) HapticManager.Instance.LightTap();

            if (levelController != null)
            {
                levelController.RegisterMove();
            }
        }
        else
        {
            piece.PlayInvalidFeedback();
            if (SoundManager.Instance != null) SoundManager.Instance.PlayBack();
        }
    }

    private void OnRotationComplete()
    {
        if (levelController != null)
        {
            levelController.CheckForWin();
        }
    }

    private Vector3 ScreenToWorld(Vector2 screenPos)
    {
        Vector3 worldPos = cam.ScreenToWorldPoint(screenPos);
        worldPos.z = 0f;
        return worldPos;
    }

    private Cell FindCellAt(Vector3 worldPos)
    {
        float half = gridManager.CellWorldSize * 0.5f;
        Cell cell = gridManager.GetCellAtWorld(worldPos);
        if (cell == null) return null;

        Vector3 cellPos = gridManager.GridToWorld(cell.gridX, cell.gridY);
        if (Mathf.Abs(worldPos.x - cellPos.x) <= half &&
            Mathf.Abs(worldPos.y - cellPos.y) <= half)
        {
            return cell;
        }
        return null;
    }

    private PathPiece FindPieceAt(Vector3 worldPos)
    {
        Cell cell = FindCellAt(worldPos);
        if (cell == null) return null;
        return cell.currentPiece;
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

        if (EventSystem.current.IsPointerOverGameObject())
            return true;

        if (Input.touchCount > 0)
        {
            return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
        }

        return false;
    }
}
