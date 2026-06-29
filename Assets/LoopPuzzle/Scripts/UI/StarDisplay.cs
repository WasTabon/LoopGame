using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StarDisplay : MonoBehaviour
{
    public TextMeshProUGUI[] stars = new TextMeshProUGUI[3];
    public Image[] starImages = new Image[3];

    private static readonly Color FilledColor = new Color(0.984f, 0.741f, 0.243f, 1f);
    private static readonly Color EmptyColor = new Color(1f, 1f, 1f, 0.18f);

    private bool UsesImages => starImages != null && starImages.Length > 0 && starImages[0] != null;

    public void SetStars(int count)
    {
        if (UsesImages)
        {
            for (int i = 0; i < starImages.Length; i++)
            {
                if (starImages[i] == null) continue;
                ApplyImageState(starImages[i], i < count);
                starImages[i].transform.localScale = Vector3.one;
            }
            return;
        }

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
        if (UsesImages)
        {
            for (int i = 0; i < starImages.Length; i++)
            {
                if (starImages[i] == null) continue;
                ApplyImageState(starImages[i], false);
                starImages[i].transform.localScale = Vector3.one;
            }

            yield return new WaitForSeconds(0.2f);

            for (int i = 0; i < count; i++)
            {
                if (starImages[i] == null) continue;
                ApplyImageState(starImages[i], true);
                starImages[i].transform.localScale = Vector3.zero;
                starImages[i].transform.DOScale(1f, 0.45f).SetEase(Ease.OutBack);
                if (SoundManager.Instance != null) SoundManager.Instance.PlayClick();
                if (HapticManager.Instance != null) HapticManager.Instance.LightTap();
                yield return new WaitForSeconds(0.28f);
            }
            yield break;
        }

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

    private void ApplyImageState(Image img, bool filled)
    {
        img.sprite = PieceSpriteFactory.GetStarSprite(filled);
        img.color = Color.white;
    }
}
