using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponSlot : MonoBehaviour
{
    [SerializeField] private Button itemButton;
    [SerializeField] private Text itemText;
    //[SerializeField] private SpriteRenderer weaponRenderer;

    private int slotNum;
    private TurnManager manager;
    private Weapon weapon;

    public void SetUpWeapon(int num, TurnManager manager, Weapon weapon)
    {
        this.manager = manager;
        slotNum = num;
        this.weapon = weapon;
    }

    public void UpdateWeapon(bool isUnlocked)
    {
        itemButton.interactable = isUnlocked;

        if (weapon == null)
        {
            itemText.text = string.Empty;
            //weaponRenderer.enabled = false;
        }
        else
        {
            itemText.text = $"{weapon.Data.WeaponName} (+{manager.UpLvNow(weapon.Data)})";
            //weaponRenderer.enabled = true;
        }
    }

    public void OnButtonClicked()
    {
        manager.SelectWeapon(weapon);
    }
}
