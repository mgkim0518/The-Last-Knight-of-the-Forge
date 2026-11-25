using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ForgeClick : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private UpgradeSelector selector;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        selector.OpenPanel();
    }
}
