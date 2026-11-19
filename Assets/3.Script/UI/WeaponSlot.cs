using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WeaponSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerClickHandler, IPointerDownHandler
{
    [Header("칸 내의 구성요소")]
    [SerializeField] private CanvasGroup self;
    [SerializeField] private Button itemButton;
    [SerializeField] private Text itemText;
    [SerializeField] private Image weaponImage;
    [SerializeField] private Image[] plate;
    [SerializeField] private Text unlockText;
    [SerializeField] private GameObject highlight;

    [Header("잠김 표시")]
    [SerializeField] private GameObject lockIcon;
    [SerializeField] private Text priceText;
    [SerializeField] private Color lockedColor = Color.black;
    [SerializeField] private Color unlockedColor = new Color(0f, 0f, 0f, 0.65f);
    [SerializeField] private Color priceColor = Color.white;
    [SerializeField] private Color warnColor = Color.red;

    [Header("충전 리벳 (왼쪽부터)")]
    [SerializeField] private Image[] rivets;
    [SerializeField] private Color rivetOff = new Color(0.35f, 0.35f, 0.37f, 1f);
    [SerializeField] private Color rivetOn = new Color(1f, 0.55f, 0.2f, 1f);
    [SerializeField] private Color rivetFull = new Color(1f, 0.92f, 0.6f, 1f);

    [Header("충전 종류 아이콘 (위력, 범위, 거리, 방어 순서)")]
    [SerializeField] private Image chargeIcon;
    [SerializeField] private Sprite[] chargeSprites;
    [SerializeField] private Color[] chargeColors;

    [Header("기타 설정값")]
    [SerializeField] private float draggingAlpha = 0.4f;

    public Weapon Weapon { get; private set; }

    private Image background;
    private DragView dragView;
    private int slotNum;
    private TurnManager manager;
    private SlotState state;
    private bool isSelectedOnPress;

    public void SetUpSlot(int num, TurnManager manager, DragView drag)
    {
        background = GetComponent<Image>();
        this.manager = manager;
        dragView = drag;
        slotNum = num;
    }

    public void UpdateSlot(SlotState state, Weapon weapon, int price)
    {
        this.state = state;
        this.Weapon = weapon;

        itemButton.interactable = state.Equals(SlotState.Unlocked) || state.Equals(SlotState.NextLock);

        if (state == SlotState.Unlocked || state == SlotState.Preview )
        {
            background.color = unlockedColor;
            lockIcon.SetActive(false);
            priceText.gameObject.SetActive(false);
            UpdateWeapon(weapon);
        }
        else
        {
            background.color = lockedColor;
            itemText.text = string.Empty;
            UpdateCharge(null);
            UpdateLock(state, price);
        }

        if (state == SlotState.Unlocked && weapon != null && weapon == manager.SelectedNow )
        {
            highlight.SetActive(true);
        }
        else
        {
            highlight.SetActive(false);
        }
    }

    private void UpdateLock(SlotState state, int price)
    {
        lockIcon.SetActive(true);

        bool isNext = state == SlotState.NextLock;
        priceText.gameObject.SetActive(isNext);
        if (isNext)
        {
            priceText.text = $"철괴 {price}개";
            priceText.color = manager.IronNow >= price ? priceColor : warnColor;
        }
    }

    private void UpdateWeapon(Weapon weapon)
    {
        if (weapon == null)
        {
            itemText.text = string.Empty;
            UpdateCharge(null);
            return;
        }

        itemText.text = $"{weapon.Data.WeaponName} Lv{manager.UpLvNow(weapon.Data)}";
        UpdateCharge(weapon);
    }

    private void UpdateCharge(Weapon weapon)
    {
        if (weapon == null)
        {
            foreach (Image i in plate)
            {
                i.gameObject.SetActive(false);
            }
            chargeIcon.enabled = false;
            return;
        }

        int cap = manager.CapNow(weapon.Data);
        int stack = weapon.CurStack;
        bool isFull = stack >= cap;

        if (cap > rivets.Length)
        {
            Debug.LogWarning($"충전 상한({cap})이 리벳 수({rivets.Length})보다 큽니다.");
        }

        for (int i = 0; i < rivets.Length; i++)
        {
            bool isUsed = i < cap;
            rivets[i].gameObject.SetActive(isUsed);
            if (!isUsed)
            {
                continue;
            }

            if (isFull)
            {
                rivets[i].color = rivetFull;
            }
            else if (i < stack)
            {
                rivets[i].color = rivetOn;
            }
            else
            {
                rivets[i].color = rivetOff;
            }
        }

        int type = (int)weapon.Data.ChargeType;
        if (type >= chargeSprites.Length || type >= chargeColors.Length)
        {
            Debug.LogWarning($"충전 종류 {weapon.Data.ChargeType}의 아이콘이나 색이 없습니다.");
            chargeIcon.enabled = false;
            return;
        }

        chargeIcon.enabled = true;
        chargeIcon.sprite = chargeSprites[type];
        foreach (Image i in plate)
        {
            i.color = chargeColors[type];
            i.gameObject.SetActive(true);
        }
    }

    public void OnButtonClicked()
    {
        switch (state)
        {
            case SlotState.Unlocked:
                manager.SelectWeapon(Weapon);
                break;
            case SlotState.NextLock:
                manager.TryExpand();
                break;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (Weapon == null || !manager.IsMeltAvailable)
        {
            eventData.pointerDrag = null;
            return;
        }

        if (manager.SelectedNow != null)
        {
            manager.CancelTargetSelect();
        }

        self.alpha = draggingAlpha;
        dragView.Show(Weapon.Data.Icon, itemText.text, eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        dragView.Move(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        dragView.Hide();
        self.alpha = 1f;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right)
        {
            return;
        }

        if (isSelectedOnPress || Weapon == null)
        {
            return;
        }

        manager.TryMelt(Weapon);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right)
        {
            return;
        }

        isSelectedOnPress = manager.SelectedNow != null || manager.IsCancelledThisFrame;
    }
}
