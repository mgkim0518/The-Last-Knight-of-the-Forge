using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon
{
    public WeaponData Data;
    public int CurStack;

    public Weapon(WeaponData data) { Data = data; }
}

public class WeaponStatCalculator
{
    private readonly BattleData tierStat;
    private readonly EconomyData playData;
    private readonly Forge forge;
    public WeaponStatCalculator(BattleData tierStat, EconomyData playData, Forge forge) 
    { 
        this.tierStat = tierStat;
        this.playData = playData;
        this.forge = forge;

    }
    public int CalcDamage(Weapon weapon)
    {
        int dmg = weapon.Data.BasePower;

        int upLevel = forge.GetUpgradeLevel(weapon.Data);

        int stackVal = tierStat.GetStackValue(weapon.Data);
        if (weapon.Data.UpgradeType.Equals(UpgradeType.Charge))
        {
            stackVal += upLevel * playData.ChargeValuePerUpgrade;
        }

        if (weapon.Data.UpgradeType.Equals(UpgradeType.Power))
        {
            dmg += upLevel * playData.PowerPerUpgrade;   
        }

        if (weapon.Data.ChargeType.Equals(ChargeType.Power))
        {
            dmg += weapon.CurStack * stackVal;
        }

        return dmg;
    }
    public int CalcWEffect(Weapon weapon)
    {
        int coefficient = weapon.Data.BaseAmount;

        if (!weapon.Data.ChargeType.Equals(ChargeType.Power))
        {
            coefficient += weapon.CurStack;
        }

        return coefficient;
    }

    public int CalcHitRange(Weapon weapon)
    {
        if(weapon.Data.Reloc.Equals(Relocation.Push))
        {
            return 1;   // 철퇴는 맨 앞 1명만 침
        }

        return CalcWEffect(weapon);
    }

    public int CalcChargeValue(Weapon weapon)
    {
        if (weapon.Data.ChargeType.Equals(ChargeType.Power))
        {
            return CalcDamage(weapon);
        }

        return CalcWEffect(weapon);
    }
}
