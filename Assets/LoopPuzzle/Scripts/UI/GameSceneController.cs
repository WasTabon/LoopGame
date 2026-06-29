using UnityEngine;
using UnityEngine.UI;

public class GameSceneController : MonoBehaviour
{
    public Button backButton;
    public string menuSceneName = "MainMenu";

    private void Start()
    {
        Debug.Assert(backButton != null, "Back button not assigned!");
        backButton.onClick.AddListener(OnBackClicked);
    }

    private void OnBackClicked()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBack();
        }
        TransitionManager.Instance.LoadScene(menuSceneName);
    }
}
