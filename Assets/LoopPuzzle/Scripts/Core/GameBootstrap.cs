using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    private void Awake()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
        Input.multiTouchEnabled = true;
    }
}
