using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHPView : MonoBehaviour
{
    [SerializeField] private TurnManager manager;
    [SerializeField] private Slider hpSlider;
    [SerializeField] private Text hpText;
    [SerializeField] private Text nextDamageText;
    [SerializeField] private Transform shieldPlace;
    [SerializeField] private Image shieldIcon;

    [Header("피해 숫자")]
    [SerializeField] private DamagePopup popupPrefab;
    [SerializeField] private Transform popupAnchor;
    [SerializeField] private Color damageColor = Color.red;
    [SerializeField] private Color healColor = Color.green;

    private List<Image> shieldIcons;
    private int lastHP = -1;

    private void Awake()
    {
        manager.OnStateChanged += ShowHP;
        manager.OnStateChanged += ShowShieldLeft;
        manager.OnStateChanged += ShowExpectedDamage;
        manager.OnGameOver += ResetLastHP;
        manager.OnGameReset += ResetLastHP;
        shieldIcon.gameObject.SetActive(false);
        shieldIcons = new List<Image>();
        shieldIcons.Add(shieldIcon);
    }

    private void OnDestroy()
    {
        manager.OnStateChanged -= ShowHP;
        manager.OnStateChanged -= ShowShieldLeft;
        manager.OnStateChanged -= ShowExpectedDamage;
        manager.OnGameOver -= ResetLastHP;
        manager.OnGameReset -= ResetLastHP;
    }

    private void ResetLastHP(int turn, int kills) => ResetLastHP();

    private void ResetLastHP()
    {
        lastHP = -1;
    }

    private void ShowHP()
    {
        hpSlider.value = manager.HealthNow;
        hpText.text = $"{manager.HealthNow} / {manager.MaxHPNow}";

        ShowChange(manager.HealthNow);
    }

    private void ShowChange(int hp)
    {
        if (lastHP >= 0 && popupAnchor != null)
        {
            int diff = hp - lastHP;
            if (diff < 0)
            {
                DamagePopup.Spawn(popupPrefab, popupAnchor.position, $"{diff}", damageColor);
            }
            else if (diff > 0)
            {
                DamagePopup.Spawn(popupPrefab, popupAnchor.position, $"+{diff}", healColor);
            }
        }

        lastHP = hp;
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

    private void ShowExpectedDamage()
    {
        int expected = manager.ExpectedDamage();

        nextDamageText.gameObject.SetActive(expected > 0);
        nextDamageText.text = $"(-{expected})";
    }
}