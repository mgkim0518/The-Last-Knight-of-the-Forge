using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHPView : MonoBehaviour
{
    [SerializeField] private TurnManager manager;
    [SerializeField] private Slider hpSlider;
    [SerializeField] private Text hpText;
    [SerializeField] private Transform shieldPlace;
    [SerializeField] private Image shieldIcon;

    private List<Image> shieldIcons;

    private void Awake()
    {
        manager.OnStateChanged += ShowHP;
        manager.OnStateChanged += ShowShieldLeft;
        shieldIcon.gameObject.SetActive(false);
        shieldIcons = new List<Image>(); 
        shieldIcons.Add(shieldIcon);
    }

    private void OnDestroy()
    {
        manager.OnStateChanged -= ShowHP;
        manager.OnStateChanged -= ShowShieldLeft;
    }

    private void ShowHP()
    {
        hpSlider.value = manager.HealthNow;
        hpText.text = $"체력: {manager.HealthNow} / {manager.MaxHPNow}";
    }

    private void ShowShieldLeft()
    {
        int shieldLeft = manager.ShieldNow;

        while (shieldIcons.Count < shieldLeft)
        {
            Image icon = Instantiate(shieldIcon, shieldPlace);
            shieldIcons.Add(icon);
        }

        for (int i = 0; i < shieldIcons.Count; i++)
        {
            shieldIcons[i].gameObject.SetActive(i < shieldLeft);
        }
    }
}
