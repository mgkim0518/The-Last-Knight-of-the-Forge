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
    public int MeltLeft;
    public int Iron { get; private set; }
    public Forge Forge { get; private set; }

    public Field Field { get; } = new Field();
    public EnemySpawner Spawner { get; private set; }
    public Enemy NextEnemy { get; private set; }
    public int PrepTurns;

    public event Action<int> OnIronChanged;

    private Dictionary<WeaponData, int> weaponGenWeights = new Dictionary<WeaponData, int>();

    public GameState(int seed, EconomyData playData, SpawnData spawn, 
        int prepTurns, BattleData battleData)
    {
        Rng = new System.Random(seed);
        Player = new Player();
        PlayData = playData;
        BattleData = battleData;
        Turn = 1;
        Spawn = spawn;
        
        foreach (WeaponData w in Spawn.WeaponPool)
        {
            weaponGenWeights[w] = w.SpawnWeight;
        }
        Inventory = new Weapon[playData.StartSlots];
        NextWeapon = CreateRandomWeapon();
        MeltLeft = playData.FurnaceCap;
        Iron = playData.StartIron;
        Forge = new Forge(this);
        Calc = new WeaponStatCalculator(battleData, playData, Forge);
        
        PrepTurns = prepTurns;
        Spawner = new EnemySpawner(this);
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
        int weightSum = 0;
        WeaponData data = null;

        foreach (WeaponData w in Spawn.WeaponPool)
        {
            weightSum += weaponGenWeights[w];
        }

        if (weightSum <= 0)
        {
            Debug.LogWarning("만들 무기가 없습니다.");
            return null;
        }

        int weaponIndex = Rng.Next(weightSum);

        int cumulative = 0;

        foreach (WeaponData w in Spawn.WeaponPool)
        {
            cumulative += weaponGenWeights[w];
            if (weaponIndex < cumulative)
            {
                data = w;
                break;
            }
        }

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
        if (Turn + 1 <= PrepTurns) return null;

        Enemy spawned = Spawner.CreateEnemy(Turn+1);

        return spawned;
    }

    public void TrySpawnEnemy()
    {
        if (!Field.IsEntranceFree)
        {
            if (NextEnemy != null && NextEnemy.Data.EnemyType.Equals(EnemyType.Charge))
            {
                NextEnemy = Field.BreakThrough(NextEnemy);
            }
            else
            {
                Enemy chargeE = Spawner.BlockBuster(Turn + 1);

                if (chargeE != null)
                {
                    NextEnemy = chargeE;
                }
            }
            
            return;
        }

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
            bounty += e.Bounty;
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
