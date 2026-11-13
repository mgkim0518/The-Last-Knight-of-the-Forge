using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "SpawnData", menuName = "Scriptable Objects/SpawnData")]
public class SpawnData : ScriptableObject
{
    [Header("무기")]
    public WeaponData[] WeaponPool;
    public int UpWeightTurn = 10;
    public int ChoiceCount = 3;
    public int WeightIncrease = 1;
    public int MaxSpawnWeight = 20;

    [Header("적")]
    public EnemyData[] EnemyPool;
    public float SpawnProb = 0.8f;
    public float ShieldProb = 0.3f;
    public int FirstShieldTurn = 50;
    public int BlockBusterDeploy = 5;
}
