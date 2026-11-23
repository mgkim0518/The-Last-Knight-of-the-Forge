using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 메인 강화 패널: 무기 칸 현황과 선택지만 보여 준다. 실제 강화는 UpPopup이 맡는다.
public class UpgradeSelector : MonoBehaviour
{
    [System.Serializable]
    private class UpgradeCell
    {
        public Button Button;
        public Text NameText;
        public Image Icon;
        public Image ChargeIcon;
        public GameObject OwnedBadge;
    }

    [SerializeField] private TurnManager manager;
    [SerializeField] private UpPopup popup;

    [Header("구성요소")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Text titleText;
    [SerializeField] private Text ironText;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button[] openButtons;
    [SerializeField] private UpgradeCell[] cells;

    [Header("충전 아이콘 (WeaponSlot과 같은 값)")]
    [SerializeField] private Sprite[] chargeSprites;
    [SerializeField] private Color[] chargeColors;

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
            int index = i;
            cells[i].Button.onClick.AddListener(() => SelectCell(index));
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
        foreach (UpgradeCell cell in cells)
        {
            cell.Button.onClick.RemoveAllListeners();
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

    // 팝업이 열려 있으면 팝업부터 닫는다.
    private void PressClose()
    {
        if (popup.IsOpen)
        {
            popup.Close();
            return;
        }

        ClosePanel();
    }

    // 열 때마다 한 번만 정렬한다: 인벤토리에 있는 무기(칸 순서) → 나머지(무기 풀 순서).
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
            UpgradeCell cell = cells[i];

            if (i >= order.Count)
            {
                cell.Button.gameObject.SetActive(false);
                continue;
            }

            WeaponData data = order[i];
            cell.Button.gameObject.SetActive(true);
            cell.NameText.text = $"{data.WeaponName} Lv{manager.UpLvNow(data)}";
            cell.Icon.sprite = data.Icon;
            cell.OwnedBadge.SetActive(owned.Contains(data));
            SetChargeIcon(cell.ChargeIcon, data.ChargeType);
            cell.Button.interactable = !isLocked;
        }
    }

    private void SelectCell(int index)
    {
        if (index >= order.Count || IsLocked())
        {
            return;
        }

        popup.Open(order[index]);
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

    private void SetChargeIcon(Image icon, ChargeType chargeType)
    {
        int type = (int)chargeType;
        if (type >= chargeSprites.Length || type >= chargeColors.Length)
        {
            icon.enabled = false;
            return;
        }

        icon.enabled = true;
        icon.sprite = chargeSprites[type];
        icon.color = chargeColors[type];
    }
}