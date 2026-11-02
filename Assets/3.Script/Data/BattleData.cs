using UnityEngine;

[CreateAssetMenu(fileName = "BattleData", menuName = "Scriptable Objects/BattleData")]
public class BattleData : ScriptableObject
{
    [Header("충전 등급")]
    public int CapShort = 2;
    public int CapNormal = 4;
    public int CapLong = 6;

    public int ValueSlow = 1;
    public int ValueNormal = 2;
    public int ValueFast = 3;

    [Header("충돌")]
    [Range(0f, 1f)] public float CollisionDamagePercent = 0.1f;

    [Header("적")]
    [Tooltip("x = 턴, y = 체력 배율")]
    public AnimationCurve EnemyHealthCurve = AnimationCurve.Linear(0, 1, 100, 10);

    public int GetCap(CapTier tier) => tier switch
    {
        CapTier.Short => CapShort,
        CapTier.Long => CapLong,
        _ => CapNormal,
    };

    public int GetStackValue(WeaponData w) => w.ChargeType switch
    {
        ChargeType.Power => w.ValueTier switch
        {
            ValueTier.Slow => ValueSlow,
            ValueTier.Fast => ValueFast,
            _ => ValueNormal,
        },
        _ => 1, // 범위·거리·방어는 스택당 1칸(1회) 고정
    };
}