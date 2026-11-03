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
    public int DamageCalc(Weapon weapon)
    {
        int dmg = weapon.Data.BasePower;

        int tierVal = 0;

        switch (weapon.Data.ValueTier)
        {
            case ValueTier.Slow:
                tierVal = 1;
                break;
            case ValueTier.Normal:
                tierVal = 2;
                break;
            case ValueTier.Fast:
                tierVal = 3;
                break;
        }

        if (weapon.Data.ChargeType.Equals(ChargeType.Power))
        {
            dmg += weapon.CurStack * tierVal;
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

        if (weapon.Data.ChargeType.Equals(ChargeType.Area))
        {
            hitRange += weapon.CurStack;
        }
        else if (weapon.Data.ChargeType.Equals(ChargeType.Distance))
        {
            hitRange = 1;
        }

        return hitRange;
    }
}
