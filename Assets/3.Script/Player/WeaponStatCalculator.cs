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

    public int CalcHitRange(Weapon weapon)
    {
        int hitRange = weapon.Data.BaseAmount;

        if (weapon.Data.Target.Equals(TargetType.WithinReach))
        {
            return hitRange + weapon.CurStack;
            //갈고리의 던지는 거리
        }

        if (weapon.Data.ChargeType.Equals(ChargeType.Area) 
            || weapon.Data.ChargeType.Equals(ChargeType.Defense))
        {
            hitRange += weapon.CurStack;
        }
        else if (weapon.Data.ChargeType.Equals(ChargeType.Distance))
        {
            hitRange = 1;
        }

        return hitRange;
    }

    public int CalcPush(Weapon weapon)
    {
        int pushRange = weapon.Data.BaseAmount + weapon.CurStack;

        return pushRange;
    }
}
