using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeightUpPanel : MonoBehaviour
{
    [SerializeField] TurnManager manager;

    [Header("구성요소 (위에서 아래로)")]
    [SerializeField] GameObject panel;
    [SerializeField] Button[] selections;
    [SerializeField] Image[] weaponIcons;
    [SerializeField] Text[] weaponTexts;
    [SerializeField] Button skipButton;
    [SerializeField] Text skipText;

    private void Awake()
    {
        manager.OnWeightChoiceOpen += ShowButtons;
        manager.OnWeightChoiceClose += ClosePanel;

        for (int i = 0; i < selections.Length; i++)
        {
            int index = i;
            selections[i].onClick.AddListener(() => manager.ChooseWeightInc(index));
        }
        skipButton.onClick.AddListener(Skip);

        panel.SetActive(false);
    }

    private void OnDestroy()
    {
        manager.OnWeightChoiceOpen -= ShowButtons;
        manager.OnWeightChoiceClose -= ClosePanel;

        foreach (Button button in selections)
        {
            button.onClick.RemoveAllListeners();
        }
        skipButton.onClick.RemoveListener(Skip);
    }

    private void ShowButtons(IReadOnlyList<WeaponData> choices)
    {
        skipText.text = $"건너뛰기 (+{manager.SkipRewardNow} 철괴)";

        for (int i = 0; i < selections.Length; i++)
        {
            if (i < choices.Count)
            {
                int weight = manager.ChoiceWeightNow(i);
                int nextWeight = manager.ChoiceNextWeightNow(i);

                weaponIcons[i].sprite = choices[i].Icon;
                weaponTexts[i].text = $"{choices[i].WeaponName} (가중치 {weight} → {nextWeight})";
                selections[i].gameObject.SetActive(true);
            }
            else
            {
                selections[i].gameObject.SetActive(false);
            }
        }

        panel.SetActive(true);
    }

    private void Skip()
    {
        manager.SkipWeightInc();
    }

    private void ClosePanel()
    {
        panel.SetActive(false);
    }
}