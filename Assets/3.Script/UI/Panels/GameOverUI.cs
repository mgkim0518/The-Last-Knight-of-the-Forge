using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TurnManager manager;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Text calcText;
    [SerializeField] private Text scoreText;
    [SerializeField] private GameObject NewRecordLabel;
    [SerializeField] private GameObject DebugUsedLabel;

    private void Awake()
    {
        manager.OnGameOver += WriteScoreboard;
    }

    private void OnDestroy()
    {
        manager.OnGameOver -= WriteScoreboard;
    }

    private void WriteScoreboard(int turn, int kills)
    {
        gameOverPanel.SetActive(true);
        calcText.text = $"{turn} X {manager.ScorePerTurnNow} =\n{kills} X {manager.ScorePerKillNow} =";
        scoreText.text = $"{manager.TurnScoreNow}\n{manager.KillScoreNow}" +
            $"\n\n{manager.ScoreNow}\n{manager.HighScoreNow}";
        NewRecordLabel.SetActive(manager.IsNewBestNow && !manager.IsDebugUsedNow);
        DebugUsedLabel.SetActive(manager.IsDebugUsedNow);
    }

    public void RestartButton()
    {
        gameOverPanel.SetActive(false);
        manager.InitGame();
    }
}
