using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialBanner : MonoBehaviour
{
    public RectTransform panel;
    public TextMeshProUGUI label;
    public CanvasGroup canvasGroup;

    private Coroutine timedRoutine;

    private void Awake()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }
    }

    public void Show(string message)
    {
        if (timedRoutine != null)
        {
            StopCoroutine(timedRoutine);
            timedRoutine = null;
        }
        ShowInternal(message);
    }

    public void ShowTimed(string message, float seconds)
    {
        if (timedRoutine != null) StopCoroutine(timedRoutine);
        timedRoutine = StartCoroutine(TimedRoutine(message, seconds));
    }

    private IEnumerator TimedRoutine(string message, float seconds)
    {
        ShowInternal(message);
        yield return new WaitForSeconds(seconds);
        Hide();
        timedRoutine = null;
    }

    private void ShowInternal(string message)
    {
        if (label != null) label.text = message;

        if (canvasGroup != null)
        {
            canvasGroup.DOKill();
            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, 0.3f).SetEase(Ease.OutQuad);
        }

        if (panel != null)
        {
            panel.DOKill();
            panel.localScale = Vector3.one * 0.9f;
            panel.DOScale(1f, 0.35f).SetEase(Ease.OutBack);
        }
    }

    public void Hide()
    {
        if (canvasGroup != null)
        {
            canvasGroup.DOKill();
            canvasGroup.DOFade(0f, 0.25f).SetEase(Ease.InQuad);
        }
    }
}
