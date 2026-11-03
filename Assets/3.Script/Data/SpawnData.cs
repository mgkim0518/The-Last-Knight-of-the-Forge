using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "SpawnData", menuName = "Scriptable Objects/SpawnData")]
public class SpawnData : ScriptableObject
{
    [Header("무기")]
    public WeaponData[] WeaponPool;
    public int UpWeightTurn = 10;
    public int WeightIncrease = 1;
    public int MaxSpawnWeight = 20;

    //[Header("적")]
}
