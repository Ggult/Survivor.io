using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public sealed class DifficultySelectionUI : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button easyButton;
    [SerializeField] private Button mediumButton;
    [SerializeField] private Button hardButton;

    private UnityAction easyAction;
    private UnityAction mediumAction;
    private UnityAction hardAction;

    public void Bind(UnityAction<Difficulty> selectAction)
    {
        Unbind();
        if (selectAction == null)
        {
            return;
        }

        easyAction = () => selectAction(Difficulty.Easy);
        mediumAction = () => selectAction(Difficulty.Medium);
        hardAction = () => selectAction(Difficulty.Hard);
        easyButton?.onClick.AddListener(easyAction);
        mediumButton?.onClick.AddListener(mediumAction);
        hardButton?.onClick.AddListener(hardAction);
    }

    public void Show()
    {
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
        Unbind();
    }

    private void Unbind()
    {
        if (easyButton != null && easyAction != null)
        {
            easyButton.onClick.RemoveListener(easyAction);
        }

        if (mediumButton != null && mediumAction != null)
        {
            mediumButton.onClick.RemoveListener(mediumAction);
        }

        if (hardButton != null && hardAction != null)
        {
            hardButton.onClick.RemoveListener(hardAction);
        }

        easyAction = null;
        mediumAction = null;
        hardAction = null;
    }
}
