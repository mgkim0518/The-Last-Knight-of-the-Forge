using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SlotState
{
    Unlocked,
    NextLock,
    Locked
}

public class WeaponSlotManager : MonoBehaviour
{
    [SerializeField] private TurnManager manager;
    [Tooltip("왼쪽 위부터 가로 순서대로 오른쪽 아래까지")] 
    [SerializeField] private WeaponSlot[] slots;

    private void Awake()
    {
        if (slots.Length > 12)
        {
            Debug.LogWarning("인벤토리 칸이 너무 많이 할당되었습니다.");
        }

        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].SetUpSlot(i, manager);
        }
        manager.OnStateChanged += ChangeSlots;
    }

    private void OnDestroy()
    {
        manager.OnStateChanged -= ChangeSlots;
    }

    private void ChangeSlots()
    {
        Weapon[] inv = manager.InventoryNow;
        int maxInv = manager.MaxSlotsNow;
        int price = manager.SlotPriceNow;

        for (int i = 0; i < slots.Length; i++)
        {
            if (i < inv.Length)
            {
                slots[i].UpdateSlot(SlotState.Unlocked, inv[i], 0);
            }
            else if (i == inv.Length && i < manager.MaxSlotsNow)
            {
                slots[i].UpdateSlot(SlotState.NextLock, null, price);
            }
            else
            {
                slots[i].UpdateSlot(SlotState.Locked, null, 0);
            }
        }
    }
}
