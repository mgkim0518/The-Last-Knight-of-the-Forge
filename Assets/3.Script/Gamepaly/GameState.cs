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

    public GameState(int seed, EconomyData playData, SpawnData spawn)
    {
        Rng = new System.Random(seed);
        Player = new Player();
        PlayData = playData;
        Spawn = spawn;
        Inventory = new Weapon[playData.StartSlots];
        Turn = 1;
        NextWeapon = CreateRandomWeapon();
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
        bool added = AddWeapon(NextWeapon);
        NextWeapon = CreateRandomWeapon();
        return added;
    }

    public void NextTurn() => Turn++;
}
