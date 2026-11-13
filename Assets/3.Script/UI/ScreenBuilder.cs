using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenBuilder : MonoBehaviour
{
    [SerializeField] private TurnManager manager;

    [SerializeField] private Text turnText;
    [SerializeField] private Text healthText;
    [SerializeField] private Text ironText;
    [SerializeField] private Text shieldText;
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
        healthText.text = $"체력: {manager.HealthNow} / {manager.MaxHPNow}";
        ironText.text = $"철괴: {manager.IronNow}";
        shieldText.text = $"보호막: {manager.ShieldNow}";
        nextDamageText.text = $"예상 데미지: {manager.ExpectedDamage()}";
    }
}
