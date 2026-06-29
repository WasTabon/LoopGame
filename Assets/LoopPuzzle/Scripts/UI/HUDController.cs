using DG.Tweening;
using TMPro;
using UnityEngine;

public class HUDController : MonoBehaviour
{
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI movesText;

    private int displayedMoves;
    private Tween counterTween;

    public void SetLevel(int levelNumber)
    {
        levelText.text = "Level " + levelNumber;
    }

    public void SetMovesInstant(int moves)
    {
        displayedMoves = moves;
        movesText.text = "Moves: " + moves;
    }

    public void AnimateMoves(int newMoves)
    {
        counterTween?.Kill();
        int from = displayedMoves;
        counterTween = DOTween.To(() => from, v =>
        {
            displayedMoves = v;
            movesText.text = "Moves: " + v;
        }, newMoves, 0.25f).SetEase(Ease.OutQuad);

        movesText.transform.DOKill();
        movesText.transform.localScale = Vector3.one;
        movesText.transform.DOPunchScale(Vector3.one * 0.15f, 0.25f, 6, 0.6f);
        displayedMoves = newMoves;
    }
}
