using UnityEngine;

public sealed class GameFlowUI : MonoBehaviour
{
    [SerializeField] private GameHUDUI gameHUD;
    [SerializeField] private ResultPanelUI resultPanel;
    [SerializeField] private DifficultySelectionUI difficultySelection;
    [SerializeField] private GameObject joystickRoot;
    [SerializeField] private GameObject timerRoot;

    public void Bind(GameFlow gameFlow)
    {
        SetJoystickVisible(false);
        SetTimerVisible(false);

        if (resultPanel != null)
        {
            resultPanel.BindReplay(gameFlow.RestartRun);
        }

        if (difficultySelection != null)
        {
            difficultySelection.Bind(gameFlow.SelectDifficulty);
        }
    }

    public void SetGameState(GameState state)
    {
        SetJoystickVisible(state == GameState.Playing);
        SetTimerVisible(state == GameState.Playing);
    }

    public void ShowDifficultySelection()
    {
        if (difficultySelection != null)
        {
            difficultySelection.Show();
        }
    }

    public void HideDifficultySelection()
    {
        if (difficultySelection != null)
        {
            difficultySelection.Hide();
        }
    }

    public void SetTimer(int remainingSeconds)
    {
        if (gameHUD != null)
        {
            gameHUD.SetTimer(remainingSeconds);
        }
    }

    public void ShowGameOver(int killCount, int totalKillCount)
    {
        if (resultPanel != null)
        {
            resultPanel.Show("GAME OVER", killCount, totalKillCount);
        }
    }

    public void ShowVictory(int killCount, int totalKillCount)
    {
        if (resultPanel != null)
        {
            resultPanel.Show("VICTORY", killCount, totalKillCount);
        }
    }

    public void HideResult()
    {
        if (resultPanel != null)
        {
            resultPanel.Hide();
        }
    }

    private void SetJoystickVisible(bool isVisible)
    {
        if (joystickRoot != null)
        {
            joystickRoot.SetActive(isVisible);
        }
    }

    private void SetTimerVisible(bool isVisible)
    {
        if (timerRoot != null)
        {
            timerRoot.SetActive(isVisible);
        }
    }
}
