using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHPView : MonoBehaviour
{
    [SerializeField] private TurnManager manager;
    [SerializeField] private Slider hpSlider;
    [SerializeField] private Text hpText;

    private void Awake()
    {
        manager.OnStateChanged += ShowHP;
    }

    private void OnDestroy()
    {
        manager.OnStateChanged -= ShowHP;
    }

    private void ShowHP()
    {
        hpSlider.value = manager.HealthNow;
        hpText.text = $"체력: {manager.HealthNow} / {manager.MaxHPNow}";
    }
}
