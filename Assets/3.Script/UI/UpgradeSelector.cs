using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeSelector : MonoBehaviour
{
    [SerializeField] private TurnManager manager;
    [SerializeField] private UpPopup popup;

    [Header("구성요소")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Text titleText;
    [SerializeField] private Text ironText;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button[] openButtons;
    [SerializeField] private UpgradeCell[] cells;

    [Header("안내 문구")]
    [SerializeField] private string defaultText = "무기 강화";
    [SerializeField] private string healedText = "이번 턴엔 회복만 할 수 있어요";
    [SerializeField] private string expandedText = "이번 턴엔 이미 슬롯을 늘렸어요";

    private readonly List<WeaponData> order = new List<WeaponData>();
    private readonly HashSet<WeaponData> owned = new HashSet<WeaponData>();

    public bool IsOpen => panel.activeSelf;

    private void Awake()
    {
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i].OnWeaponUp += SelectWeapon;
        }
        closeButton.onClick.AddListener(PressClose);
        foreach (Button button in openButtons)
        {
            button.onClick.AddListener(OpenPanel);
        }

        manager.OnStateChanged += RefreshCells;
        manager.OnCurrencyChange += ShowIron;

        panel.SetActive(false);
    }

    private void OnDestroy()
    {
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i].OnWeaponUp -= SelectWeapon;
        }
        closeButton.onClick.RemoveListener(PressClose);
        foreach (Button button in openButtons)
        {
            button.onClick.RemoveListener(OpenPanel);
        }

        manager.OnStateChanged -= RefreshCells;
        manager.OnCurrencyChange -= ShowIron;
    }

    public void OpenPanel()
    {
        if (!manager.IsForgeOpenable)
        {
            return;
        }

        BuildOrder();
        panel.SetActive(true);
        manager.OpenForge(ForgeState.Upgrade);

        ShowIron(manager.IronNow);
        RefreshCells();
    }

    public void ClosePanel()
    {
        popup.Close();
        panel.SetActive(false);
        manager.CloseForge();
    }

    private void PressClose()
    {
        if (popup.IsOpen)
        {
            popup.Close();
            return;
        }

        ClosePanel();
    }

    private void BuildOrder()
    {
        order.Clear();
        owned.Clear();

        foreach (Weapon weapon in manager.InventoryNow)
        {
            if (weapon == null || !IsUpgradable(weapon.Data))
            {
                continue;
            }

            if (owned.Add(weapon.Data))
            {
                order.Add(weapon.Data);
            }
        }

        foreach (WeaponData data in manager.WeaponPoolNow)
        {
            if (!IsUpgradable(data) || owned.Contains(data))
            {
                continue;
            }

            order.Add(data);
        }

        if (order.Count > cells.Length)
        {
            Debug.LogWarning($"강화 칸이 부족합니다. 무기 {order.Count}종, 칸 {cells.Length}개");
        }
    }

    private bool IsUpgradable(WeaponData data) => data.UpgradeType != UpgradeType.None;

    // 구매 뒤에는 순서는 그대로 두고 내용만 갱신한다.
    private void RefreshCells()
    {
        if (!IsOpen)
        {
            return;
        }

        bool isLocked = IsLocked();
        titleText.text = GetGuideText();

        for (int i = 0; i < cells.Length; i++)
        {
            if (i >= order.Count)
            {
                cells[i].gameObject.SetActive(false);
                continue;
            }

            WeaponData data = order[i];
            cells[i].ShowCell(data, manager.UpLvNow(data), owned.Contains(data), isLocked,
                manager.SpawnRateNow(data));
            cells[i].gameObject.SetActive(true);
        }
    }

    private void SelectWeapon(WeaponData data)
    {
        if (IsLocked())
        {
            return;
        }

        popup.Open(data);
    }

    private bool IsLocked()
    {
        return manager.ForgeStateNow == ForgeState.Heal
            || manager.ForgeStateNow == ForgeState.Expand;
    }

    private string GetGuideText()
    {
        switch (manager.ForgeStateNow)
        {
            case ForgeState.Heal:
                return healedText;
            case ForgeState.Expand:
                return expandedText;
            default:
                return defaultText;
        }
    }

    private void ShowIron(int iron)
    {
        ironText.text = $"철괴 {iron}";
    }
}