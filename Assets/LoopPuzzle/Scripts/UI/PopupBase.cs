using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PopupBase : MonoBehaviour
{
    public RectTransform window;
    public Image backdrop;

    private const float BackdropAlpha = 0.6f;

    public void Show()
    {
        gameObject.SetActive(true);

        backdrop.raycastTarget = true;
        Color c = backdrop.color;
        c.a = 0f;
        backdrop.color = c;
        backdrop.DOFade(BackdropAlpha, 0.25f).SetEase(Ease.OutQuad);

        window.localScale = Vector3.zero;
        window.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
    }

    public void Hide(System.Action onComplete = null)
    {
        backdrop.DOFade(0f, 0.22f).SetEase(Ease.InQuad);
        window.DOScale(0f, 0.28f).SetEase(Ease.InBack).OnComplete(() =>
        {
            backdrop.raycastTarget = false;
            gameObject.SetActive(false);
            onComplete?.Invoke();
        });
    }
}
