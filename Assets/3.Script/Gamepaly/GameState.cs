using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState 
{
    public readonly System.Random Rng;
    public EconomyData PlayData;
    public int Turn { get; private set; }
    public Weapon[] Inventory { get; private set; }
    public Field Field { get; } = new Field();
    public int SpawnIndex;

    public GameState(int seed, EconomyData playData)
    {
        Rng = new System.Random(seed);
        PlayData = playData;
        Inventory = new Weapon[playData.StartSlots];
        Turn = 1;
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

    public void NextTurn() => Turn++;
}
