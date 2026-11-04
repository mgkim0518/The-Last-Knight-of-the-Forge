using UnityEngine;

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