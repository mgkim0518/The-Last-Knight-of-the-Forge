using UnityEngine;

public struct EnemyAction
{
    public int Cell;
    public EnemyBehav E_Action;
    public int Dest;
}

public struct EnemyAttack //적의 공격
{
    public int Cell;
    public int Damage;
}

public struct HitResult //실제 플레이어에게 적용 여부
{
    public int Cell;
    public int Damage;
    public bool Blocked;
}