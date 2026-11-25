using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenBuilder : MonoBehaviour
{
    [SerializeField] private TurnManager manager;

    [SerializeField] private Text turnText;
    [SerializeField] private Text ironText;
    [SerializeField] private Text scoreText;

    private void Awake()
    {
        manager.OnStateChanged += ChangeTexts;
    }

    private void OnDestroy()
    {
        manager.OnStateChanged -= ChangeTexts;
    }

    private void ChangeTexts()
    {
        turnText.text = $"턴: {manager.TurnNow}";
        ironText.text = $"철괴: {manager.IronNow}";
        scoreText.text = $"점수: {manager.ScoreNow}";
    }
}
