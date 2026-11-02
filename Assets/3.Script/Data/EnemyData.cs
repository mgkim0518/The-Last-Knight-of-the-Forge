using UnityEngine;

public enum EnemyType
{
    Normal = 0,
    Ranged = 1,
    Heal = 2,
    Shield = 3
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

    [Header("전투")]
    public int BaseHealth;
    public int Dmg;
    public int Range;
    public int StopCell;
    public int MoveSpeed;

    [Header("특수")]
    [Range(0f, 1f)] public float HealPercent;
    public bool HasShield = false;
}
