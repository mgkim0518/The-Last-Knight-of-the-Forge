using UnityEngine;

public enum UpgradeType { None = 0, Power = 1, Charge = 2 }
public enum ChargeType { Power = 0, Area = 1, Distance = 2, Defense = 3 }
public enum CapTier { Short = 0, Normal = 1, Long = 2 }      // 2 / 4 / 6
public enum ValueTier { Slow = 0, Normal = 1, Fast = 2 }       // +1 / +2 / +3
public enum TargetType { Front = 0, Back = 1, Choose = 2, FrontThree = 3, Self = 4 }

[CreateAssetMenu(fileName = "WeaponData", menuName = "Scriptable Objects/WeaponData")]
public class WeaponData : ScriptableObject
{
    [Header("기본")]
    public string WeaponName;
    public Sprite Icon;
    public TargetType Target;

    [Header("수치")]
    public int BasePower;
    [Tooltip("범위: 타격 칸 수 / 거리: 넉백·끌기 칸 수 / 방어: 막는 횟수")]
    public int BaseAmount = 1;

    [Header("충전")]
    public ChargeType ChargeType;
    public CapTier CapTier = CapTier.Normal;
    [Tooltip("위력 충전 무기만 사용")]
    public ValueTier ValueTier = ValueTier.Normal;

    [Header("강화")]
    public UpgradeType UpgradeType = UpgradeType.Power;

    [Header("등장")]
    public int SpawnWeight = 10;
    public int MaxSpawnWeight = 50;
}
