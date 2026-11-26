using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitHighlighter : MonoBehaviour
{
    [SerializeField] private TurnManager manager;
    [Header("필드 칸 표시 (앞 칸부터 7개)")]
    [SerializeField] private GameObject[] marks;

    private void Awake()
    {
        manager.OnTargetSelect += ShowFor;
        manager.OnChoiceFinished += ShowSelected;
        manager.OnStateChanged += ShowSelected;
        manager.OnHitPreview += Preview;
        manager.OnGameOver += ClearOnGameOver;
        Clear();
    }

    private void OnDestroy()
    {
        manager.OnTargetSelect -= ShowFor;
        manager.OnChoiceFinished -= ShowSelected;
        manager.OnStateChanged -= ShowSelected;
        manager.OnHitPreview -= Preview;
        manager.OnGameOver -= ClearOnGameOver;
    }

    private void Preview(Weapon weapon)
    {
        if (weapon == null)
        {
            ShowSelected();
            return;
        }

        if (manager.SelectedNow != null || manager.IsInputBlocked)
        {
            return;
        }

        ShowFor(weapon);
    }

    private void ShowSelected() => ShowFor(manager.SelectedNow);

    private void ShowFor(Weapon weapon)
    {
        Clear();
        if (weapon == null)
        {
            return;
        }

        foreach (int cell in manager.GetHitCells(weapon))
        {
            if (cell >= 0 && cell < marks.Length)
            {
                marks[cell].SetActive(true);
            }
        }
    }

    private void Clear()
    {
        foreach (GameObject mark in marks)
        {
            mark.SetActive(false);
        }
    }

    private void ClearOnGameOver(int turn, int kills) => Clear();
}
