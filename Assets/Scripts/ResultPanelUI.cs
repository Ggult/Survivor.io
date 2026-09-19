using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public sealed class ResultPanelUI : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text resultTitle;
    [SerializeField] private TMP_Text killCountText;
    [SerializeField] private Button replayButton;

    private UnityAction replayAction;

    public void BindReplay(UnityAction action)
    {
        if (replayButton == null)
        {
            return;
        }

        if (replayAction != null)
        {
            replayButton.onClick.RemoveListener(replayAction);
        }

        replayAction = action;
        if (replayAction != null)
        {
            replayButton.onClick.AddListener(replayAction);
        }
    }

    public void Show(string title, int killCount)
    {
        if (resultTitle != null)
        {
            resultTitle.text = title;
        }

        if (killCountText != null)
        {
            killCountText.text = string.Format("ENEMIES DEFEATED: {0}", killCount);
        }

        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }
    }

    public void Hide()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (replayButton != null && replayAction != null)
        {
            replayButton.onClick.RemoveListener(replayAction);
        }
    }
}
