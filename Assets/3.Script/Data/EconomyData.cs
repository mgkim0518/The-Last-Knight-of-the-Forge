using UnityEngine;

[CreateAssetMenu(fileName = "EconomyData", menuName = "Scriptable Objects/EconomyData")]
public class EconomyData : ScriptableObject
{
    [Header("무기 강화 (무기 종류별 선형 증가)")]
    public int UpgradeBasePrice = 10;
    public int UpgradePriceStep = 5;        
    public int PowerPerUpgrade = 1;         
    public int ChargeValuePerUpgrade = 1;   

    [Header("슬롯 확장 (지수 증가)")]
    public int StartSlots = 4;
    public int MaxSlots = 8;
    public int SlotBasePrice = 20;
    public float SlotGrowth = 1.5f;

    [Header("회복 (단가 × 회복% × 턴 배율)")]
    public float HealUnitPrice = 0.5f;
    [Tooltip("x = 턴, y = 배율")]
    public AnimationCurve HealTurnMultiplier = AnimationCurve.Linear(0, 1, 100, 5);

    [Header("철괴 수입")]
    public int SmeltValue = 2;              
    public int WeightSkipReward = 3;        

    public int GetUpgradePrice(int level)
        => UpgradeBasePrice + UpgradePriceStep * level;

    public int GetUpgradePrice(int fromLevel, int count)
    {
        int sum = 0;
        for (int i = 0; i < count; i++) sum += GetUpgradePrice(fromLevel + i);
        return sum;
    }

    public int GetSlotPrice(int expandCount)
        => Mathf.RoundToInt(SlotBasePrice * Mathf.Pow(SlotGrowth, expandCount));

    public int GetHealPrice(int percent, int turn)
        => Mathf.CeilToInt(HealUnitPrice * percent * HealTurnMultiplier.Evaluate(turn));
}