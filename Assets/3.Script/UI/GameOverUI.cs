using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TurnManager manager;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Text turnText;

    private void Awake()
    {
        manager.OnGameOver += WriteSurvivalTurn;
    }

    private void OnDestroy()
    {
        manager.OnGameOver -= WriteSurvivalTurn;
    }

    private void WriteSurvivalTurn(int turn)
    {
        gameOverPanel.SetActive(true);
        turnText.text = $"당신은 {turn}턴까지 생존했습니다.";
    }

    public void RestartButton()
    {
        gameOverPanel.SetActive(false);
        manager.Restart();
    }
}
