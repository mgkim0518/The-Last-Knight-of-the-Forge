using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum SlotState
{
    Unlocked,
    NextLock,
    Locked,
    Preview
}

public class WeaponSlotManager : MonoBehaviour
{
    [SerializeField] private TurnManager manager;
    [Tooltip("왼쪽 위부터 가로 순서대로 오른쪽 아래까지")] 
    [SerializeField] private WeaponSlot[] slots;
    [SerializeField] private WeaponSlot nextSlot;
    [SerializeField] private Text nextLabel;
    [SerializeField] private Color WarnBackColor;

    private bool isEmptySlot;
    private Image nextBackground;

    private void Awake()
    {
        if (slots.Length > 11)
        {
            Debug.LogWarning("인벤토리 칸이 너무 많이 할당되었습니다.");
        }

        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].SetUpSlot(i, manager);
        }

        nextSlot.SetUpSlot(-1, manager); //칸 번호 없음
        nextBackground = nextSlot.GetComponent<Image>();

        manager.OnStateChanged += ChangeSlots;
    }

    private void OnDestroy()
    {
        manager.OnStateChanged -= ChangeSlots;
    }

    private void ChangeSlots()
    {
        nextLabel.text = "다  음";
        isEmptySlot = false;
        Weapon[] inv = manager.InventoryNow;
        int maxInv = manager.MaxSlotsNow;
        int price = manager.SlotPriceNow;
        Weapon nextWeapon = manager.NextWeaponNow;

        for (int i = 0; i < slots.Length; i++)
        {
            if (i < inv.Length)
            {
                slots[i].UpdateSlot(SlotState.Unlocked, inv[i], 0);
                if (inv[i] == null)
                {
                    isEmptySlot = true;
                }
            }
            else if (i == inv.Length && i < maxInv)
            {
                slots[i].UpdateSlot(SlotState.NextLock, null, price);
            }
            else
            {
                slots[i].UpdateSlot(SlotState.Locked, null, 0);
            }
        }

        nextSlot.UpdateSlot(SlotState.Preview, nextWeapon, 0);

        if (!isEmptySlot)
        {
            WarnDiscard();
        }
    }

    public void WarnDiscard()
    {
        nextBackground.color = WarnBackColor;
        nextLabel.text = "쉬면 소멸";
    }
}
