using UnityEngine;

public sealed class GameFlowUI : MonoBehaviour
{
    [SerializeField] private GameHUDUI gameHUD;
    [SerializeField] private ResultPanelUI resultPanel;

    public void Bind(GameFlow gameFlow)
    {
        if (resultPanel != null)
        {
            resultPanel.BindReplay(gameFlow.RestartRun);
        }
    }

    public void SetTimer(int remainingSeconds)
    {
        if (gameHUD != null)
        {
            gameHUD.SetTimer(remainingSeconds);
        }
    }

    public void ShowGameOver(int killCount)
    {
        if (resultPanel != null)
        {
            resultPanel.Show("GAME OVER", killCount);
        }
    }

    public void ShowVictory(int killCount)
    {
        if (resultPanel != null)
        {
            resultPanel.Show("VICTORY", killCount);
        }
    }

    public void HideResult()
    {
        if (resultPanel != null)
        {
            resultPanel.Hide();
        }
    }
}
