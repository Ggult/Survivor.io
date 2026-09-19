using TMPro;
using UnityEngine;

public sealed class GameHUDUI : MonoBehaviour
{
    [SerializeField] private TMP_Text timerText;

    public void SetTimer(int seconds)
    {
        if (timerText == null)
        {
            return;
        }

        int clampedSeconds = Mathf.Max(0, seconds);
        timerText.text = string.Format("{0:00}:{1:00}", clampedSeconds / 60, clampedSeconds % 60);
    }
}
