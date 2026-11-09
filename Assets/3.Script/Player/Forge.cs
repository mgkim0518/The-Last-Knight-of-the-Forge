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

        return economy.GetUpgradePrice(upLevel, upTarget - upLevel);
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
}
