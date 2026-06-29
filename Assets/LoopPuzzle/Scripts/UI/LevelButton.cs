using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    public TextMeshProUGUI numberText;
    public StarDisplay starDisplay;
    public GameObject lockIcon;
    public Button button;

    private int levelNumber;
    private System.Action<int> onClick;

    public void Setup(int levelNumber, int stars, bool unlocked, System.Action<int> clickCallback)
    {
        this.levelNumber = levelNumber;
        this.onClick = clickCallback;

        numberText.text = levelNumber.ToString();

        if (unlocked)
        {
            lockIcon.SetActive(false);
            numberText.gameObject.SetActive(true);
            starDisplay.gameObject.SetActive(true);
            starDisplay.SetStars(stars);
            button.interactable = true;
        }
        else
        {
            lockIcon.SetActive(true);
            numberText.gameObject.SetActive(false);
            starDisplay.gameObject.SetActive(false);
            button.interactable = false;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(HandleClick);
    }

    private void HandleClick()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayClick();
        onClick?.Invoke(levelNumber);
    }
}
