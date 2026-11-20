using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealPanel : MonoBehaviour
{
    [SerializeField] private TurnManager manager;
    [SerializeField] private GameObject healPanel;
    [SerializeField] private Text healLabel;
    [SerializeField] private Slider healBar;
    [SerializeField] private RectTransform currentHP;
    [SerializeField] private Text healAmount;
    [SerializeField] private Text healPrice;
    [SerializeField] private Button healButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button[] openButtons;

    [Header("안내 문구")]
    [SerializeField] private string defaultText = "회복";
    [SerializeField] private string upgradedText = "이번 턴엔 강화만 할 수 있어요";
    [SerializeField] private string expandedText = "이번 턴엔 이미 슬롯을 늘렸어요";
    [SerializeField] private string fullHPText = "체력이 가득 차 있어요";
    [SerializeField] private string ironShortText = "철괴가 {0}개 부족해요";

    private int curHP;

    private void Awake()
    {
        healBar.onValueChanged.AddListener(CantLowerHP);
        healButton.onClick.AddListener(PanelHeal);
        closeButton.onClick.AddListener(ClosePanel);
        foreach (Button button in openButtons)
        {
            button.onClick.AddListener(OpenPanel);
        }
        healPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        healBar.onValueChanged.RemoveListener(CantLowerHP);
        healButton.onClick.RemoveListener(PanelHeal);
        closeButton.onClick.RemoveListener(ClosePanel);
        foreach (Button button in openButtons)
        {
            button.onClick.RemoveListener(OpenPanel);
        }
    }

    public void OpenPanel()
    {
        if (!manager.IsForgeOpenable)
        {
            return;
        }

        curHP = manager.HealthNow;
        int maxHP = manager.MaxHPNow;
        healBar.maxValue = maxHP;
        healBar.SetValueWithoutNotify(curHP);
        Vector2 anchor = currentHP.anchorMax;
        anchor.x = (float)curHP / maxHP;
        currentHP.anchorMax = anchor;

        bool isLocked = manager.ForgeStateNow == ForgeState.Upgrade
           || manager.ForgeStateNow == ForgeState.Expand
           || curHP >= maxHP;
        healBar.interactable = !isLocked;

        healPanel.SetActive(true);
        manager.OpenForge(ForgeState.Heal);

        Notify();
    }

    public void ClosePanel()
    {
        healPanel.SetActive(false);
        manager.CloseForge();
    }

    public void CantLowerHP(float hpVal)
    {
        if (hpVal < curHP)
        {
            healBar.SetValueWithoutNotify(curHP);
        }

        Notify();
    }

    public void PanelHeal()
    {
        int target = Mathf.RoundToInt(healBar.value);

        if (manager.TryHeal(target))
        {
            ClosePanel();
        }
    }

   public void Notify()
    {
        int target = Mathf.RoundToInt(healBar.value);
        int price = manager.HealPriceNow(target);
        int iron = manager.IronNow;

        healAmount.text = $"{curHP} → {target} (+{target - curHP})";
        healPrice.text = $"{price} 철괴";
        healPrice.color = price > iron ? Color.red : Color.white;

        healLabel.text = GetGuideText(price, iron);

        healButton.interactable = manager.IsHealableNow(target);
    }

    private string GetGuideText(int price, int iron)
    {
        switch (manager.ForgeStateNow)
        {
            case ForgeState.Upgrade:
                return upgradedText;
            case ForgeState.Expand:
                return expandedText;
        }

        if (curHP >= manager.MaxHPNow)
        {
            return fullHPText;
        }

        if (price > iron)
        {
            return string.Format(ironShortText, price - iron);
        }

        return defaultText;
    }
}
