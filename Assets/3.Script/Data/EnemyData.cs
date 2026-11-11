using UnityEngine;

public enum EnemyType
{
    Normal = 0,
    Ranged = 1,
    Heal = 2
}

public enum EnemyBehav
{
    Move = 0,
    Attack = 1,
    Wait = 2, 
    Heal = 3,
    DecidedFleeing = 4,
    Flee = 5
}

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]


public class EnemyData : ScriptableObject
{
    [Header("기본")]
    public EnemyType EnemyType;
    public string Name;
    public Sprite Sprite;
    public int SpawnTurn;
    public int Reward;
    public int SpawnWeight = 10;
    public BaseEnemyAct Act;

    [Header("전투")]
    public int BaseHealth;
    public int Dmg;
    public int Range;
    public int MoveSpeed;
    public int FleeCounter = 0;

    [Header("특수")]
    [Range(0f, 1f)] public float HealPercent;
}
