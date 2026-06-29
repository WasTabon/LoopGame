using UnityEngine;
using UnityEngine.EventSystems;

public class PieceInput : MonoBehaviour
{
    public GridManager gridManager;

    private Camera cam;
    private Vector2 pressPosition;
    private bool pressing;
    private const float TapMoveThreshold = 20f;

    private void Start()
    {
        cam = Camera.main;
        Debug.Assert(gridManager != null, "PieceInput has no GridManager assigned!");
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (IsPointerOverUI()) return;
            pressing = true;
            pressPosition = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (!pressing) return;
            pressing = false;

            float moved = Vector2.Distance(Input.mousePosition, pressPosition);
            if (moved <= TapMoveThreshold)
            {
                HandleTap(Input.mousePosition);
            }
        }
    }

    private void HandleTap(Vector2 screenPos)
    {
        Vector3 worldPos = cam.ScreenToWorldPoint(screenPos);
        worldPos.z = 0f;

        PathPiece piece = FindPieceAt(worldPos);
        if (piece == null) return;

        if (piece.canRotate)
        {
            piece.Rotate();
            if (SoundManager.Instance != null) SoundManager.Instance.PlayClick();
            if (HapticManager.Instance != null) HapticManager.Instance.LightTap();
        }
        else
        {
            piece.PlayInvalidFeedback();
            if (SoundManager.Instance != null) SoundManager.Instance.PlayBack();
        }
    }

    private PathPiece FindPieceAt(Vector3 worldPos)
    {
        float half = gridManager.CellWorldSize * 0.5f;

        for (int y = 0; y < gridManager.Height; y++)
        {
            for (int x = 0; x < gridManager.Width; x++)
            {
                Cell cell = gridManager.GetCell(x, y);
                if (cell == null || cell.currentPiece == null) continue;

                Vector3 cellPos = gridManager.GridToWorld(x, y);
                if (Mathf.Abs(worldPos.x - cellPos.x) <= half &&
                    Mathf.Abs(worldPos.y - cellPos.y) <= half)
                {
                    return cell.currentPiece;
                }
            }
        }

        return null;
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
