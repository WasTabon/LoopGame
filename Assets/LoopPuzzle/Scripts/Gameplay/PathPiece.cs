using DG.Tweening;
using UnityEngine;

public class PathPiece : MonoBehaviour
{
    public PieceType pieceType;
    public int rotationSteps;
    public bool canRotate = true;
    public PieceColor pieceColor = PieceColor.Neutral;
    public int maxRotations = 0;
    public bool directional = false;

    private SpriteRenderer spriteRenderer;
    private Tween rotateTween;
    private Tween scaleTween;
    private Tween highlightTween;
    private Vector3 baseScale;
    private Color baseColor = Color.white;
    private int rotationsUsed = 0;
    private TextMesh limitRenderer;
    private GameObject limitLabel;

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

    public void ApplyColor(PieceColor color)
    {
        pieceColor = color;
        if (color == PieceColor.Neutral) return;
        if (spriteRenderer == null) return;
        baseColor = GetColorValue(color);
        spriteRenderer.color = baseColor;
    }

    public static Color GetColorValue(PieceColor color)
    {
        switch (color)
        {
            case PieceColor.Red: return new Color(0.906f, 0.337f, 0.337f, 1f);
            case PieceColor.Blue: return new Color(0.290f, 0.565f, 0.886f, 1f);
            case PieceColor.Green: return new Color(0.298f, 0.733f, 0.396f, 1f);
            case PieceColor.Yellow: return new Color(0.961f, 0.769f, 0.180f, 1f);
        }
        return Color.white;
    }

    public bool CanStillRotate()
    {
        if (!canRotate) return false;
        if (maxRotations <= 0) return true;
        return rotationsUsed < maxRotations;
    }

    public int RemainingRotations()
    {
        if (maxRotations <= 0) return -1;
        return Mathf.Max(0, maxRotations - rotationsUsed);
    }

    public void SetupRotationLimitIndicator()
    {
        if (maxRotations <= 0) return;
        if (limitLabel != null) return;

        GameObject labelGo = new GameObject("RotationLimit");
        labelGo.transform.SetParent(transform, false);
        labelGo.transform.localPosition = new Vector3(0, 0, -0.2f);
        limitLabel = labelGo;

        limitRenderer = labelGo.AddComponent<TextMesh>();
        Font font = GetBuiltinFont();
        if (font != null)
        {
            limitRenderer.font = font;
            MeshRenderer fr = labelGo.GetComponent<MeshRenderer>();
            if (fr != null) fr.sharedMaterial = font.material;
        }
        limitRenderer.text = maxRotations.ToString();
        limitRenderer.characterSize = 0.12f;
        limitRenderer.fontSize = 64;
        limitRenderer.anchor = TextAnchor.MiddleCenter;
        limitRenderer.alignment = TextAlignment.Center;
        limitRenderer.color = new Color(1f, 1f, 1f, 0.95f);

        MeshRenderer mr = labelGo.GetComponent<MeshRenderer>();
        if (mr != null) mr.sortingOrder = 12;

        float inv = baseScale.x != 0f ? 1f / baseScale.x : 1f;
        labelGo.transform.localScale = new Vector3(inv, inv, 1f) * 0.5f;

        UpdateLimitLabel();
    }

    private static Font cachedFont;
    private static Font GetBuiltinFont()
    {
        if (cachedFont != null) return cachedFont;
        cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (cachedFont == null) cachedFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return cachedFont;
    }

    private void UpdateLimitLabel()
    {
        if (limitRenderer == null) return;
        int remaining = RemainingRotations();
        limitRenderer.text = remaining.ToString();
        limitRenderer.color = remaining == 0
            ? new Color(0.95f, 0.45f, 0.45f, 1f)
            : new Color(1f, 1f, 1f, 0.95f);
    }

    public void SetScale(float worldSize)
    {
        Sprite sprite = spriteRenderer.sprite;
        float spriteWorldSize = sprite.bounds.size.x;
        float factor = worldSize / spriteWorldSize;
        baseScale = new Vector3(factor, factor, 1f);
        transform.localScale = baseScale;
    }

    public void ApplyFixedTint()
    {
        if (spriteRenderer == null) return;
        baseColor = new Color(0.62f, 0.66f, 0.78f, 1f);
        spriteRenderer.color = baseColor;
    }

    public void PlayEntrance(float delay)
    {
        scaleTween?.Kill();
        Vector3 target = baseScale;
        transform.localScale = Vector3.zero;
        scaleTween = transform.DOScale(target, 0.4f)
            .SetEase(Ease.OutBack)
            .SetDelay(delay);
    }

    public bool[] GetConnections()
    {
        return PieceConnections.GetRotatedConnections(pieceType, rotationSteps);
    }

    public void Rotate(System.Action onComplete = null)
    {
        if (!canRotate || !CanStillRotate())
        {
            onComplete?.Invoke();
            return;
        }

        rotationsUsed++;
        rotationSteps = (rotationSteps + 1) % 4;
        float targetZ = -90f * rotationSteps;

        rotateTween?.Kill();
        rotateTween = transform.DORotate(new Vector3(0, 0, targetZ), 0.22f, RotateMode.Fast)
            .SetEase(Ease.OutBack)
            .OnComplete(() => onComplete?.Invoke());

        scaleTween?.Kill();
        transform.localScale = baseScale;
        scaleTween = transform.DOPunchScale(baseScale * 0.12f, 0.22f, 6, 0.6f);

        UpdateLimitLabel();
    }

    public void RotateBack(System.Action onComplete = null)
    {
        if (rotationsUsed > 0) rotationsUsed--;
        rotationSteps = ((rotationSteps - 1) % 4 + 4) % 4;
        float fromZ = -90f * (rotationSteps + 1);
        float targetZ = -90f * rotationSteps;

        transform.localRotation = Quaternion.Euler(0, 0, fromZ);

        rotateTween?.Kill();
        rotateTween = transform.DORotate(new Vector3(0, 0, targetZ), 0.22f, RotateMode.Fast)
            .SetEase(Ease.OutBack)
            .OnComplete(() => onComplete?.Invoke());

        scaleTween?.Kill();
        transform.localScale = baseScale;
        scaleTween = transform.DOPunchScale(baseScale * 0.12f, 0.22f, 6, 0.6f);

        UpdateLimitLabel();
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

    public void Lift()
    {
        scaleTween?.Kill();
        spriteRenderer.sortingOrder = 15;
        transform.DOScale(baseScale * 1.15f, 0.12f).SetEase(Ease.OutQuad);
    }

    public void FollowTo(Vector3 worldPos)
    {
        transform.position = worldPos;
    }

    public void SnapTo(Vector3 worldPos, System.Action onComplete = null)
    {
        scaleTween?.Kill();
        transform.DOScale(baseScale, 0.18f).SetEase(Ease.OutBack);
        transform.DOMove(worldPos, 0.18f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            spriteRenderer.sortingOrder = 5;
            onComplete?.Invoke();
        });
    }

    public void ReturnTo(Vector3 worldPos, System.Action onComplete = null)
    {
        scaleTween?.Kill();
        transform.DOScale(baseScale, 0.22f).SetEase(Ease.OutBack);
        transform.DOMove(worldPos, 0.22f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            spriteRenderer.sortingOrder = 5;
            onComplete?.Invoke();
        });
    }

    private void LateUpdate()
    {
        if (limitLabel != null)
        {
            limitLabel.transform.rotation = Quaternion.identity;
        }
    }

    private void OnDestroy()
    {
        rotateTween?.Kill();
        scaleTween?.Kill();
        highlightTween?.Kill();
    }
}
