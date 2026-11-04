using UnityEngine;
using System.Collections.Generic;

public class Enemy
{
    public EnemyData Data;
    public int HP;
    public bool IsShielded;
    public bool IsAttackedLastTurn;
    public Enemy(EnemyData data, int hp)
    {
        Data = data;
        HP = hp;
    }

    public void Damaged(int damage)
    {
        if (IsShielded)
        {
            IsShielded = false;
            return;
        }

        HP -= damage;
    }
    public bool IsDead => HP <= 0;

    public bool IsInRange(int index) => index < Data.Range;
    //index 0 = 기획서 1번 칸
}

public class Field
{
    public const int Size = 7;
    readonly Enemy[] cells = new Enemy[Size];

    public Enemy Get(int i) => cells[i];
    public bool IsEmpty(int i) => cells[i] == null;
    public bool IsEntranceFree => IsEmpty(Size - 1);

    public bool TrySpawn(Enemy e)
    {
        if (!IsEntranceFree)
        {
            return false;
        }
        cells[Size - 1] = e;
        return true;
    }

    public List<EnemyAttack> ActAll()   
    {
        List<EnemyAttack> attacks = new List<EnemyAttack>();
        for (int i = 0; i < Size; i++)
        {
            Enemy e = cells[i];
            if (e == null) continue;

            bool canMove = i > 0 && cells[i - 1] == null;
            bool inRange = e.IsInRange(i);

            if (inRange && !(e.IsAttackedLastTurn && canMove))
            {
                attacks.Add(new EnemyAttack { Cell = i, Damage = e.Data.Dmg});
                e.IsAttackedLastTurn = true;
            }
            else if (canMove)
            {
                cells[i - 1] = e;
                cells[i] = null;
                e.IsAttackedLastTurn = false;
            }
        }
        return attacks;
    }

    public void CheckDead()
    {
        for (int i = 0; i < cells.Length; i++)
        {
            Enemy e = cells[i];
            if (e != null && e.IsDead)
            {
                Debug.Log($"{i}번 칸 적 처치");
                cells[i] = null;
            }
        }
    }
}
