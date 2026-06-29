using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class ButtonPunch : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    public bool playClickSound = true;

    private RectTransform rectTransform;
    private Vector3 baseScale;
    private Tween activeTween;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        baseScale = rectTransform.localScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        activeTween?.Kill();
        activeTween = rectTransform.DOScale(baseScale * 0.95f, 0.08f).SetEase(Ease.OutQuad);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        activeTween?.Kill();
        activeTween = rectTransform.DOScale(baseScale, 0.18f).SetEase(Ease.OutBack);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (playClickSound && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayClick();
        }

        if (HapticManager.Instance != null)
        {
            HapticManager.Instance.LightTap();
        }
    }
}
