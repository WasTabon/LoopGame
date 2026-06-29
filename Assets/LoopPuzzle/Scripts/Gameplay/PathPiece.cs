using DG.Tweening;
using UnityEngine;

public class PathPiece : MonoBehaviour
{
    public PieceType pieceType;
    public int rotationSteps;
    public bool canRotate = true;

    private SpriteRenderer spriteRenderer;
    private Tween rotateTween;
    private Tween scaleTween;
    private Vector3 baseScale;

    public void Init(PieceType type, int startRotation, bool rotatable)
    {
        pieceType = type;
        rotationSteps = ((startRotation % 4) + 4) % 4;
        canRotate = rotatable;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = PieceSpriteFactory.GetPieceSprite(type);
        spriteRenderer.sortingOrder = 5;

        baseScale = transform.localScale;
        transform.localRotation = Quaternion.Euler(0, 0, -90f * rotationSteps);
    }

    public void SetScale(float worldSize)
    {
        Sprite sprite = spriteRenderer.sprite;
        float spriteWorldSize = sprite.bounds.size.x;
        float factor = worldSize / spriteWorldSize;
        baseScale = new Vector3(factor, factor, 1f);
        transform.localScale = baseScale;
    }

    public bool[] GetConnections()
    {
        return PieceConnections.GetRotatedConnections(pieceType, rotationSteps);
    }

    public void Rotate()
    {
        if (!canRotate) return;

        rotationSteps = (rotationSteps + 1) % 4;
        float targetZ = -90f * rotationSteps;

        rotateTween?.Kill();
        rotateTween = transform.DORotate(new Vector3(0, 0, targetZ), 0.22f, RotateMode.Fast)
            .SetEase(Ease.OutBack);

        scaleTween?.Kill();
        transform.localScale = baseScale;
        scaleTween = transform.DOPunchScale(baseScale * 0.12f, 0.22f, 6, 0.6f);
    }

    public void PlayInvalidFeedback()
    {
        transform.DOShakeRotation(0.3f, new Vector3(0, 0, 12f), 12, 60f);
    }
}
