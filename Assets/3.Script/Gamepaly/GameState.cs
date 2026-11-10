using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState 
{
    public readonly System.Random Rng;
    public EconomyData PlayData { get; private set; }
    public BattleData BattleData { get; private set; }
    public SpawnData Spawn;
    public int Turn { get; private set; }

    public Player Player;
    public Weapon[] Inventory { get; private set; }
    public Weapon NextWeapon { get; private set; }
    public WeaponStatCalculator Calc { get; private set; }
    public int SpawnIndex;
    public int MeltLeft;
    public int Iron { get; private set; }
    public Forge Forge { get; private set; }

    public Field Field { get; } = new Field();
    public Enemy NextEnemy { get; private set; }
    public int PrepTurns;

    public event Action<int> OnIronChanged;


    private readonly bool[] spawnPattern = { true, false, true, true, false, true, false, false };

    public GameState(int seed, EconomyData playData, SpawnData spawn, 
        int prepTurns, BattleData battleData)
    {
        Rng = new System.Random(seed);
        Player = new Player();
        PlayData = playData;
        BattleData = battleData;
        Turn = 1;
        Spawn = spawn;
        
        Inventory = new Weapon[playData.StartSlots];
        NextWeapon = CreateRandomWeapon();
        MeltLeft = playData.FurnaceCap;
        Iron = playData.StartIron;
        Forge = new Forge(this);
        Calc = new WeaponStatCalculator(battleData, playData, Forge);
        
        PrepTurns = prepTurns;
        NextEnemy = CreateNextEnemy();
    }

    public bool RemoveWeapon(Weapon weapon)
    {
        for (int i = 0; i < Inventory.Length; i++)
        {
            if (Inventory[i] == weapon)
            {
                Inventory[i] = null;
                return true;
            }
        }
        return false;
    }

    public bool AddWeapon(Weapon weapon)
    {
        for (int i = 0; i < Inventory.Length; i++)
        {
            if (Inventory[i] == null)
            {
                Inventory[i] = weapon;
                return true;
            }
        }
        return false;
    }

    public Weapon CreateRandomWeapon()
    {
        WeaponData data = Spawn.WeaponPool[Rng.Next(Spawn.WeaponPool.Length)];
        return new Weapon(data);
    }

    public bool ReceiveNextWeapon()
    {
        bool added = true;

        if (NextWeapon != null) 
        {
            added = AddWeapon(NextWeapon);
        }
        NextWeapon = CreateRandomWeapon();
        return added;
    }

    public void NextTurn()
    {
        Turn++;
        MeltLeft = PlayData.FurnaceCap;
        Forge.ResetForgeState();
    }

    public bool Melt(Weapon weapon)
    {
        if (MeltLeft <= 0)
        {
            return false;
        }

        if (weapon == null)
        {
            return false;
        }

        if (weapon == NextWeapon)
        {
            NextWeapon = null;
        }
        else
        {
            if (!RemoveWeapon(weapon))
            {
                return false;
            }
        }

        MeltLeft--;
        AddIron(PlayData.GetMeltValue(weapon));
        return true;
    }

    public void AddIron(int addVal)
    {
        if (addVal <= 0)
        {
            return;
        }

        Iron += addVal;
        OnIronChanged?.Invoke(Iron);
    }

    public bool UseIron(int useVal)
    {
        if (Iron < useVal)
        {
            return false;
        }

        Iron -= useVal;
        OnIronChanged?.Invoke(Iron);
        return true;
    }

    public Enemy CreateNextEnemy()
    {
        if (Turn < PrepTurns - 1) return null;

        bool spawn = spawnPattern[SpawnIndex % spawnPattern.Length];
        SpawnIndex++;

        if (spawn)
        {
           return new Enemy(Spawn.EnemyPool[0], Spawn.EnemyPool[0].BaseHealth);
        }
        else
        {
            return null;
        }
    }

    public void TrySpawnEnemy()
    { 
        if (!Field.IsEntranceFree) return;

        if (NextEnemy != null)
        {
            Field.TrySpawn(NextEnemy);
        }

        NextEnemy = CreateNextEnemy();
    }

    public int ClearDeadEnemies()
    {
        int bounty = 0;
        List<Enemy> casualty = Field.CheckDead();

        foreach (Enemy e in casualty)
        {
            bounty += e.Data.Reward;
        }

        AddIron(bounty);
        return bounty;
    }

    public bool AddSlot()
    {
        Weapon[] slots = Inventory;

        if (Inventory.Length >= PlayData.MaxSlots)
        {
            return false;
        }

        Array.Resize(ref slots, slots.Length + 1);

        Inventory = slots;

        return true;
    }

    public void Debug_SkipTurn(int dest)
    {
        if (Turn >= dest)
        {
            return;
        }

        Turn = dest;
    }
}
