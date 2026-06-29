using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public RectTransform titleTransform;
    public RectTransform playButton;
    public RectTransform levelsButton;
    public RectTransform settingsButton;
    public SettingsPopup settingsPopup;

    public string gameSceneName = "Game";
    public string levelSelectSceneName = "LevelSelect";

    private void Start()
    {
        AnimateIn();

        GetButton(playButton).onClick.AddListener(OnPlayClicked);
        GetButton(levelsButton).onClick.AddListener(OnLevelsClicked);
        GetButton(settingsButton).onClick.AddListener(OnSettingsClicked);
    }

    private Button GetButton(RectTransform rt)
    {
        Button btn = rt.GetComponent<Button>();
        Debug.Assert(btn != null, "Button component missing on " + rt.name);
        return btn;
    }

    private void AnimateIn()
    {
        titleTransform.localScale = Vector3.zero;
        playButton.localScale = Vector3.zero;
        levelsButton.localScale = Vector3.zero;
        settingsButton.localScale = Vector3.zero;

        Sequence seq = DOTween.Sequence();
        seq.Append(titleTransform.DOScale(1f, 0.5f).SetEase(Ease.OutBack));
        seq.Append(playButton.DOScale(1f, 0.35f).SetEase(Ease.OutBack));
        seq.Append(levelsButton.DOScale(1f, 0.35f).SetEase(Ease.OutBack).SetDelay(-0.2f));
        seq.Append(settingsButton.DOScale(1f, 0.35f).SetEase(Ease.OutBack).SetDelay(-0.2f));
    }

    private void OnPlayClicked()
    {
        int resume = ProgressManager.Instance != null ? ProgressManager.Instance.GetMaxUnlockedLevel() : 1;
        GameSession.RequestedLevelNumber = resume;
        TransitionManager.Instance.LoadScene(gameSceneName);
    }

    private void OnLevelsClicked()
    {
        TransitionManager.Instance.LoadScene(levelSelectSceneName);
    }

    private void OnSettingsClicked()
    {
        if (settingsPopup != null)
        {
            settingsPopup.Open();
        }
    }
}
