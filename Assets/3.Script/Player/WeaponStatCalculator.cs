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
    public WeaponStatCalculator(BattleData tierStat) { this.tierStat = tierStat; }
    public int CalcDamage(Weapon weapon)
    {
        int dmg = weapon.Data.BasePower;

        if (weapon.Data.ChargeType.Equals(ChargeType.Power))
        {
            dmg += weapon.CurStack * tierStat.GetStackValue(weapon.Data);
        }

        return dmg;
    }
    public int CalcWEffect(Weapon weapon)
    {
        int coefficient = weapon.Data.BaseAmount;

        if (weapon.Data.ChargeType != ChargeType.Power)
        {
            coefficient += weapon.CurStack;
        }

        return coefficient;
    }

    public int CalcHitRange(Weapon weapon)
    {
        if(weapon.Data.Reloc == Relocation.Push)
        {
            return 1;   // 철퇴는 맨 앞 1명만 침
        }

        return CalcWEffect(weapon);
    }

    public int CalcChargeValue(Weapon weapon)
    {
        if (weapon.Data.ChargeType == ChargeType.Power)
        {
            return CalcDamage(weapon);
        }

        return CalcWEffect(weapon);
    }
}
