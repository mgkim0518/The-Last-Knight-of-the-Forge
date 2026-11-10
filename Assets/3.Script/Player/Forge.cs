using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ForgeState
{
    None,
    Upgrade,
    Heal,
    Expand
}

public class Forge
{
    public ForgeState CurState { get; private set; } = ForgeState.None;
    public Dictionary<WeaponData, int> UpLevel { get; private set; }

    private EconomyData economy;
    private GameState state;

    public Forge(GameState gamestate)
    {
        state = gamestate;
        economy = gamestate.PlayData;
        UpLevel = new Dictionary<WeaponData, int>();
    }

    public bool CheckForgeState(ForgeState state_f)
    {
        if (CurState != ForgeState.None && CurState != state_f)
        {
            return false;
        }
        return true;
    }

    private void SetForgeState(ForgeState state_f)
    {
        CurState = state_f;
    }

    public void ResetForgeState()
    {
        CurState = ForgeState.None;
    }

    #region 강화

    public int GetUpgradeLevel(WeaponData data)
    {
        int upLevel = 0;

        if (UpLevel.ContainsKey(data))
        {
            upLevel = UpLevel[data];
        }

        return upLevel;
    }

    public int GetUpgradePrice(WeaponData data, int upTarget)
    {
        int upLevel = GetUpgradeLevel(data);

        if (upLevel >= upTarget)
        {
            return 0;
        }

        return economy.CalcUpgradePrice(upLevel, upTarget - upLevel);
    }

    public bool CheckUpgradeable(WeaponData data, int upTarget)
    {

        if (!CheckForgeState(ForgeState.Upgrade))
        {
            return false;
        }

        if (data.UpgradeType.Equals(UpgradeType.None))
        {
            return false;
        }

        int upIronVal = GetUpgradePrice(data, upTarget);
        
        if (upIronVal <= 0)
        {
            return false;
        }

        if (state.Iron < upIronVal)
        {
            return false;
        }

        return true;
    }

    public bool Upgrade(WeaponData data, int upTarget)
    {
        if (!CheckUpgradeable(data, upTarget))
        {
            return false;
        }

        if (!state.UseIron(GetUpgradePrice(data, upTarget)))
        {
            return false;
        }

        UpLevel[data] = upTarget;

        SetForgeState(ForgeState.Upgrade);

        return true;
    }

    #endregion

    #region 회복

    public int GetHealPrice(int healTarget)
    {
        return economy.CalcHealPrice(healTarget - state.Player.HP, state.Turn);
    }

    public bool CheckHealable(int healTarget)
    {
        if (!CheckForgeState(ForgeState.Heal))
        {
            return false;
        }

        if (healTarget > Player.MaxHP)
        {
            return false;
        }

        if (healTarget <= state.Player.HP)
        {
            return false;
        }

        if (GetHealPrice(healTarget) > state.Iron)
        {
            return false;
        }

        return true;
    }

    public bool PayToHeal(int healTarget)
    {
        if (!CheckHealable(healTarget))
        {
            return false;
        }

        if (!state.UseIron(GetHealPrice(healTarget)))
        {
            return false;
        }

        state.Player.Heal(healTarget - state.Player.HP);

        SetForgeState(ForgeState.Heal);

        return true;
    }

    #endregion
}
