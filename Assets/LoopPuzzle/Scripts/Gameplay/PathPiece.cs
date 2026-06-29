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
    private Tween highlightTween;
    private Vector3 baseScale;
    private Color baseColor = Color.white;

    private static readonly Color HighlightColor = new Color(1f, 0.95f, 0.7f, 1f);

    public void Init(PieceType type, int startRotation, bool rotatable)
    {
        pieceType = type;
        rotationSteps = ((startRotation % 4) + 4) % 4;
        canRotate = rotatable;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = PieceSpriteFactory.GetPieceSprite(type);
        spriteRenderer.sortingOrder = 5;
        baseColor = spriteRenderer.color;

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

    public void Rotate(System.Action onComplete = null)
    {
        if (!canRotate)
        {
            onComplete?.Invoke();
            return;
        }

        rotationSteps = (rotationSteps + 1) % 4;
        float targetZ = -90f * rotationSteps;

        rotateTween?.Kill();
        rotateTween = transform.DORotate(new Vector3(0, 0, targetZ), 0.22f, RotateMode.Fast)
            .SetEase(Ease.OutBack)
            .OnComplete(() => onComplete?.Invoke());

        scaleTween?.Kill();
        transform.localScale = baseScale;
        scaleTween = transform.DOPunchScale(baseScale * 0.12f, 0.22f, 6, 0.6f);
    }

    public void PlayInvalidFeedback()
    {
        transform.DOShakeRotation(0.3f, new Vector3(0, 0, 12f), 12, 60f);
    }

    public void HighlightPulse()
    {
        highlightTween?.Kill();
        spriteRenderer.color = HighlightColor;
        spriteRenderer.sortingOrder = 6;
        highlightTween = spriteRenderer.DOColor(baseColor, 0.5f).SetEase(Ease.OutQuad)
            .OnComplete(() => spriteRenderer.sortingOrder = 5);

        scaleTween?.Kill();
        transform.localScale = baseScale;
        scaleTween = transform.DOPunchScale(baseScale * 0.22f, 0.4f, 7, 0.7f);
    }

    public void ResetVisual()
    {
        highlightTween?.Kill();
        scaleTween?.Kill();
        spriteRenderer.color = baseColor;
        spriteRenderer.sortingOrder = 5;
        transform.localScale = baseScale;
    }

    private void OnDestroy()
    {
        rotateTween?.Kill();
        scaleTween?.Kill();
        highlightTween?.Kill();
    }
}
