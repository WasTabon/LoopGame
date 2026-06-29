using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class StarDisplay : MonoBehaviour
{
    public TextMeshProUGUI[] stars = new TextMeshProUGUI[3];

    private static readonly Color FilledColor = new Color(0.961f, 0.651f, 0.137f, 1f);
    private static readonly Color EmptyColor = new Color(1f, 1f, 1f, 0.18f);

    public void SetStars(int count)
    {
        for (int i = 0; i < stars.Length; i++)
        {
            if (stars[i] == null) continue;
            stars[i].color = i < count ? FilledColor : EmptyColor;
            stars[i].transform.localScale = Vector3.one;
        }
    }

    public void RevealStars(int count)
    {
        StartCoroutine(RevealRoutine(count));
    }

    private IEnumerator RevealRoutine(int count)
    {
        for (int i = 0; i < stars.Length; i++)
        {
            if (stars[i] == null) continue;
            stars[i].color = EmptyColor;
            stars[i].transform.localScale = Vector3.one;
        }

        yield return new WaitForSeconds(0.2f);

        for (int i = 0; i < count; i++)
        {
            if (stars[i] == null) continue;
            stars[i].color = FilledColor;
            stars[i].transform.localScale = Vector3.zero;
            stars[i].transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
            if (SoundManager.Instance != null) SoundManager.Instance.PlayClick();
            yield return new WaitForSeconds(0.25f);
        }
    }
}
