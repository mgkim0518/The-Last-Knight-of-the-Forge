using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenBuilder : MonoBehaviour
{
    [SerializeField] private TurnManager manager;

    [SerializeField] private Text turnText;
    [SerializeField] private Text ironText;
    [SerializeField] private Text nextDamageText;

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
        nextDamageText.text = $"예상 데미지: {manager.ExpectedDamage()}";
    }
}
