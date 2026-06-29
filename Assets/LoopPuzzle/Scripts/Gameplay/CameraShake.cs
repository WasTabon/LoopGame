using DG.Tweening;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    private Vector3 basePosition;
    private Tween shakeTween;

    private void Awake()
    {
        Instance = this;
        basePosition = transform.localPosition;
    }

    public void Shake(float duration = 0.4f, float strength = 0.3f, int vibrato = 12)
    {
        shakeTween?.Kill();
        transform.localPosition = basePosition;
        shakeTween = transform.DOShakePosition(duration, strength, vibrato, 90f, false, true)
            .OnComplete(() => transform.localPosition = basePosition);
    }
}
