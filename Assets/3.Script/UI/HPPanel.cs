using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPPanel : MonoBehaviour
{
    [SerializeField] private TurnManager manager;
    [SerializeField] private GameObject healPanel;
    [SerializeField] private Text healLabel;
    [SerializeField] private Scrollbar healBar;
    [SerializeField] private Text healAmount;
    [SerializeField] private Text healPrice;
    [SerializeField] private Button healButton;
    [SerializeField] private Button closeButton;

}
