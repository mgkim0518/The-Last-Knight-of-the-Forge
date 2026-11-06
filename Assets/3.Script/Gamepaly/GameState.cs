using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState 
{
    public readonly System.Random Rng;
    public EconomyData PlayData;
    public SpawnData Spawn;
    public int Turn { get; private set; }
    public Weapon[] Inventory { get; private set; }
    public Field Field { get; } = new Field();
    public int SpawnIndex;
    public Weapon NextWeapon { get; private set; }
    public Player Player;
    public int MeltLeft;
    public Enemy NextEnemy { get; private set; }
    public int PrepTurns;

    private readonly bool[] spawnPattern = { true, false, true, true, false, true, false, false };

    public GameState(int seed, EconomyData playData, SpawnData spawn, int prepTurns)
    {
        Rng = new System.Random(seed);
        Player = new Player();
        PlayData = playData;
        Spawn = spawn;
        Inventory = new Weapon[playData.StartSlots];
        Turn = 1;
        NextWeapon = CreateRandomWeapon();
        MeltLeft = playData.FurnaceCap;
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

    public void Debug_SkipTurn(int dest)
    {
        if (Turn >= dest)
        {
            return;
        }

        Turn = dest;
    }
}
