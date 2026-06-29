using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance;

    private Canvas canvas;
    private Image fadeImage;
    private bool isTransitioning;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        BuildOverlay();
    }

    private void Start()
    {
        fadeImage.color = new Color(0f, 0f, 0f, 1f);
        fadeImage.DOFade(0f, 0.4f).SetEase(Ease.OutQuad);
    }

    private void BuildOverlay()
    {
        GameObject canvasGo = new GameObject("TransitionCanvas");
        canvasGo.transform.SetParent(transform);
        canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        canvasGo.AddComponent<GraphicRaycaster>();

        GameObject imageGo = new GameObject("FadeImage");
        imageGo.transform.SetParent(canvasGo.transform, false);
        fadeImage = imageGo.AddComponent<Image>();
        fadeImage.color = new Color(0f, 0f, 0f, 0f);
        fadeImage.raycastTarget = false;

        RectTransform rt = fadeImage.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    public void LoadScene(string sceneName)
    {
        if (isTransitioning) return;
        isTransitioning = true;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayTransition();
        }
        else
        {
            Debug.LogWarning("SoundManager is null during transition!");
        }

        fadeImage.raycastTarget = true;
        fadeImage.DOFade(1f, 0.35f).SetEase(Ease.InQuad).OnComplete(() =>
        {
            SceneManager.LoadScene(sceneName);
            fadeImage.DOFade(0f, 0.35f).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                fadeImage.raycastTarget = false;
                isTransitioning = false;
            });
        });
    }
}
