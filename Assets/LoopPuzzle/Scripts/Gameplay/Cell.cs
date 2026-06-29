using UnityEngine;

public class Cell : MonoBehaviour
{
    public int gridX;
    public int gridY;
    public CellType cellType;
    public PathPiece currentPiece;

    private SpriteRenderer background;
    private SpriteRenderer startMarker;
    private Color baseColor = Color.white;

    private static readonly Color ValidTargetColor = new Color(0.290f, 0.565f, 0.886f, 0.30f);
    private static readonly Color HoverTargetColor = new Color(0.961f, 0.651f, 0.137f, 0.50f);

    public bool IsObstacle => cellType == CellType.Obstacle;
    public bool IsEmpty => currentPiece == null && cellType != CellType.Obstacle;

    public void Init(int x, int y, CellType type, bool isStart, float worldSize)
    {
        gridX = x;
        gridY = y;
        cellType = type;

        background = GetComponent<SpriteRenderer>();
        if (background == null) background = gameObject.AddComponent<SpriteRenderer>();
        background.sprite = PieceSpriteFactory.GetCellSprite();
        background.sortingOrder = 0;

        ApplyBackgroundStyle();
        baseColor = background.color;
        SetScale(worldSize);

        if (isStart)
        {
            CreateStartMarker(worldSize);
        }
    }

    private void ApplyBackgroundStyle()
    {
        switch (cellType)
        {
            case CellType.Obstacle:
                background.color = new Color(0.094f, 0.094f, 0.149f, 1f);
                break;
            case CellType.Empty:
                background.color = new Color(1f, 1f, 1f, 0.07f);
                break;
            case CellType.Fixed:
                background.color = new Color(1f, 1f, 1f, 0.16f);
                break;
            default:
                background.color = new Color(1f, 1f, 1f, 0.10f);
                break;
        }
    }

    private void CreateStartMarker(float worldSize)
    {
        GameObject markerGo = new GameObject("StartMarker");
        markerGo.transform.SetParent(transform, false);
        startMarker = markerGo.AddComponent<SpriteRenderer>();
        startMarker.sprite = PieceSpriteFactory.GetStartMarkerSprite();
        startMarker.sortingOrder = 1;

        Sprite sprite = startMarker.sprite;
        float spriteWorldSize = sprite.bounds.size.x;
        float factor = worldSize / spriteWorldSize;
        markerGo.transform.localScale = new Vector3(factor, factor, 1f);
    }

    private void SetScale(float worldSize)
    {
        Sprite sprite = background.sprite;
        float spriteWorldSize = sprite.bounds.size.x;
        float factor = (worldSize * 0.92f) / spriteWorldSize;
        transform.localScale = new Vector3(factor, factor, 1f);
    }

    public void SetPiece(PathPiece piece)
    {
        currentPiece = piece;
    }

    public void ShowAsValidTarget()
    {
        background.color = ValidTargetColor;
    }

    public void ShowAsHoverTarget()
    {
        background.color = HoverTargetColor;
    }

    public void ClearHighlight()
    {
        background.color = baseColor;
    }
}
