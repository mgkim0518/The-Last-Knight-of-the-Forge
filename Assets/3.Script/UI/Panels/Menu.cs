using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    [SerializeField] private TurnManager manager;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject confirmPanel;

    [SerializeField] private Button openMenuButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button toTitleButton;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button declineButton;

    private void Awake()
    {
        openMenuButton.onClick.AddListener(OpenMenu);
        continueButton.onClick.AddListener(CloseMenu);
        resetButton.onClick.AddListener(OpenConfirmMenu);
        declineButton.onClick.AddListener(CloseConfirmMenu);
        toTitleButton.onClick.AddListener(ToTitle);
        confirmButton.onClick.AddListener(Restart);
    }

    private void OnDestroy()
    {
        openMenuButton.onClick.RemoveListener(OpenMenu);
        continueButton.onClick.RemoveListener(CloseMenu);
        resetButton.onClick.RemoveListener(OpenConfirmMenu);
        declineButton.onClick.RemoveListener(CloseConfirmMenu);
        toTitleButton.onClick.RemoveListener(ToTitle);
        confirmButton.onClick.RemoveListener(Restart);
    }

    private void OpenMenu()
    {
        menuPanel.SetActive(true);
    }

    private void CloseMenu()
    {
        if (confirmPanel.activeSelf)
        {
            CloseConfirmMenu();
        }

        menuPanel.SetActive(false);
    }

    private void OpenConfirmMenu()
    {
        confirmPanel.SetActive(true);
    }

    private void CloseConfirmMenu()
    {
        confirmPanel.SetActive(false);
    }

    private void ToTitle()
    {
        CloseMenu();
        manager.GotoTitle();        
    }

    private void Restart()
    {
        CloseMenu();
        manager.InitGame();
    }
}
