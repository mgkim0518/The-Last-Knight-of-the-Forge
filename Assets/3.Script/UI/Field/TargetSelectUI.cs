using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TargetSelectUI : MonoBehaviour
{
    [SerializeField] private TurnManager manager;
    [Tooltip("\"대상을 고르세요\" 문구와 취소 버튼이 들어 있는 패널")]
    [SerializeField] private GameObject hintPanel;

    private void Awake()
    {
        manager.OnTargetSelect += ShowHint;
        manager.OnChoiceFinished += HideHint;
        manager.OnGameOver += HideOnGameOver;
        manager.OnGameReset += HideHint;

        hintPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        manager.OnTargetSelect -= ShowHint;
        manager.OnChoiceFinished -= HideHint;
        manager.OnGameOver -= HideOnGameOver;
        manager.OnGameReset -= HideHint;
    }

    private void Update()
    {
        if (!hintPanel.activeSelf)
        {
            return;
        }

        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            manager.CancelTargetSelect();
        }
    }

    private void ShowHint(Weapon weapon)
    {
        hintPanel.SetActive(true);
    }

    private void HideHint()
    {
        hintPanel.SetActive(false);
    }

    private void HideOnGameOver(int turn, int kills)
    {
        HideHint();
    }

    public void CancelButton()
    {
        manager.CancelTargetSelect();
    }
}