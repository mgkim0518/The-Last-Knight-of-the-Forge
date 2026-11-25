using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeCell : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Text nameText;
    [SerializeField] private Image icon;
    [SerializeField] private GameObject ownedBadge;

    public event Action<WeaponData> OnWeaponUp;

    private WeaponData selfData;

    private void Awake()
    {
        button.onClick.AddListener(WhenPressed);
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(WhenPressed);   
    }

    public void ShowCell(WeaponData data, int upLevel, bool isOwned, bool isLocked)
    {
        selfData = data;
        nameText.text = data.WeaponName + " Lv" + upLevel;
        icon.sprite = data.Icon;
        ownedBadge.SetActive(isOwned);
        button.interactable = !isLocked;
    }

    public void WhenPressed()
    {
        if (selfData== null)
        {
            return;
        }

        OnWeaponUp?.Invoke(selfData);
    }
}
