using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FurnaceUI : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TurnManager manager;
    [Header("용광로 투입구 (왼쪽부터 오른쪽으로)")]
    [SerializeField] private Image[] doors;
    [SerializeField] private IronGainView upIronNotice;
    [SerializeField] private Color openColor;
    [SerializeField] private Color closedColor;

    private void Awake()
    {
        manager.OnStateChanged += FurnaceDoor;
        upIronNotice.enabled = false;
    }

    private void OnDestroy()
    {
        manager.OnStateChanged -= FurnaceDoor;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag.TryGetComponent<WeaponSlot>(out WeaponSlot slot))
        {
            manager.TryMelt(slot.Weapon);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null &&
            eventData.pointerDrag.TryGetComponent<WeaponSlot>(out WeaponSlot slot))
        {
            if (slot.Weapon != null)
            {
                upIronNotice.ShowPreview(manager.MeltValNow(slot.Weapon));
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        upIronNotice.HidePreview();
    }

    private void FurnaceDoor()
    {
        int cap = manager.FurnaceCapNow;
        int left = manager.MeltLeftNow;

        if (cap > doors.Length)
        {
            Debug.LogWarning($"용광로 횟수({cap})가 문 수({doors.Length})보다 많습니다.");
        }

        for (int i = 0; i < doors.Length; i++)
        {
            bool isUsed = i < cap;
            doors[i].gameObject.SetActive(isUsed);
            if (!isUsed)
            {
                continue;
            }

            doors[i].color = i >= cap - left ? openColor : closedColor;
        }
    }
}
