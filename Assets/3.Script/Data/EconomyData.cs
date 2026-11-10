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
    public int FurnaceCap = 2;
    public int SmeltValue_Short = 2;              
    public int SmeltValue_Normal = 4;              
    public int SmeltValue_Long = 5;              
    public int WeightSkipReward = 3;
    public int StartIron = 10;
    public int GetMeltValue(Weapon weapon)
    {
        switch (weapon.Data.CapTier)
        {
            case CapTier.Short:
                return SmeltValue_Short;
            case CapTier.Normal:
                return SmeltValue_Normal;
            case CapTier.Long:
                return SmeltValue_Long;
            default:
                Debug.LogWarning("무기에 잘못된 값이 들어갔습니다.");
                return 0;
        }
    }

    public int CalcUpgradePrice(int level)
        => UpgradeBasePrice + UpgradePriceStep * level;

    public int CalcUpgradePrice(int fromLevel, int count)
    {
        int sum = 0;
        for (int i = 0; i < count; i++) sum += CalcUpgradePrice(fromLevel + i);
        return sum;
    }
    public int CalcHealPrice(int percent, int turn)
        => Mathf.CeilToInt(HealUnitPrice * percent * HealTurnMultiplier.Evaluate(turn));

    public int CalcSlotPrice(int expandCount)
        => Mathf.RoundToInt(SlotBasePrice * Mathf.Pow(SlotGrowth, expandCount));


}