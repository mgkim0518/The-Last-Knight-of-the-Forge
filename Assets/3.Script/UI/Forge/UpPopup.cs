using UnityEngine;
using UnityEngine.UI;

// 강화 팝업: 목표 Lv를 정하고 결정하면 실제로 강화한다.
public class UpPopup : MonoBehaviour
{
    [SerializeField] private TurnManager manager;

    [Header("구성요소")]
    [SerializeField] private GameObject popup;
    [SerializeField] private Image weaponIcon;
    [SerializeField] private Text nameText;
    [SerializeField] private Text levelOriginText;
    [SerializeField] private Text levelAfterText;
    [SerializeField] private Text valueText;
    [SerializeField] private Text ironText;
    [SerializeField] private Button minusButton;
    [SerializeField] private Button plusButton;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    [Header("색")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warnColor = Color.red;

    private WeaponData target;
    private int curLevel;
    private int targetLevel;

    public bool IsOpen => popup.activeSelf;

    private void Awake()
    {
        minusButton.onClick.AddListener(LowerLevel);
        plusButton.onClick.AddListener(RaiseLevel);
        confirmButton.onClick.AddListener(Confirm);
        cancelButton.onClick.AddListener(Close);

        popup.SetActive(false);
    }

    private void OnDestroy()
    {
        minusButton.onClick.RemoveListener(LowerLevel);
        plusButton.onClick.RemoveListener(RaiseLevel);
        confirmButton.onClick.RemoveListener(Confirm);
        cancelButton.onClick.RemoveListener(Close);
    }

    public void Open(WeaponData data)
    {
        target = data;
        curLevel = manager.UpLvNow(data);
        targetLevel = curLevel + 1;

        popup.SetActive(true);
        Refresh();
    }

    public void Close()
    {
        popup.SetActive(false);
        target = null;
    }

    private void LowerLevel()
    {
        targetLevel = Mathf.Max(curLevel + 1, targetLevel - 1);
        Refresh();
    }

    private void RaiseLevel()
    {
        targetLevel++;
        Refresh();
    }

    private void Confirm()
    {
        if (target == null)
        {
            return;
        }

        if (manager.TryUpgrade(target, targetLevel))
        {
            Close();
        }
    }

    private void Refresh()
    {
        int iron = manager.IronNow;
        int left = iron - manager.UpPriceNow(target, targetLevel);

        weaponIcon.sprite = target.Icon;
        nameText.text = target.WeaponName;
        levelOriginText.text = $"Lv {curLevel} →";
        levelAfterText.text = targetLevel.ToString();
        valueText.text = GetValueText();
        ironText.text = $"철괴 {iron} → {left}";
        ironText.color = left < 0 ? warnColor : normalColor;

        minusButton.interactable = targetLevel > curLevel + 1;
        confirmButton.interactable = manager.IsUpgradeableNow(target, targetLevel);
    }

    private string GetValueText()
    {
        int before = manager.UpValueAt(target, curLevel);
        int after = manager.UpValueAt(target, targetLevel);

        if (target.UpgradeType == UpgradeType.Charge)
        {
            return $"충전당 +{before} → +{after}";
        }

        return $"위력 {before} → {after}";
    }
}
