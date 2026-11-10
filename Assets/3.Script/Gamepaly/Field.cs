using UnityEngine;
using System;
using System.Collections.Generic;

public class Enemy
{
    public EnemyData Data;
    public readonly int MaxHP;
    public int HP;
    public bool IsShielded;
    public bool IsAttackedLastTurn;
    public bool IsFirstAppearance = false;
    public Enemy(EnemyData data, int hp)
    {
        Data = data;
        MaxHP = hp;
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

    public int CollisionDamage(float colDmg)
    {
        int col = Mathf.Max(1, (int)(MaxHP * colDmg));

        return col;
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

    public List<EnemyAction> PlanAction()
    {
        Enemy[] currentField = new Enemy[Size];
        List<EnemyAction> nextMove = new List<EnemyAction>();

        Array.Copy(cells, currentField, Size);

        for (int i = 0; i < Size; i++)
        {
            Enemy e = currentField[i];
            if (e == null) continue;

            EnemyAction act = e.Data.Act.NextMove(currentField, i);
            nextMove.Add(act);
            if (act.E_Action.Equals(EnemyBehav.Move))
            {
                currentField[act.Dest] = currentField[act.Cell];
                currentField[act.Cell] = null;
            }
        }
        return nextMove;
    }

    public List<EnemyAttack> ActAll(List<EnemyAction> planned)   
    {
        List<EnemyAttack> attacks = new List<EnemyAttack>();

        foreach (EnemyAction act in planned)
        {
            Enemy e = cells[act.Cell];

            if (act.E_Action.Equals(EnemyBehav.Attack))
            {
                attacks.Add(new EnemyAttack { Cell = act.Cell, Damage = e.Data.Dmg });
                e.IsAttackedLastTurn = true;
            }
            else if (act.E_Action.Equals(EnemyBehav.Move))
            {
                cells[act.Dest] = e;
                cells[act.Cell] = null;
                e.IsAttackedLastTurn = false;
            }
        }

        return attacks;
    }

    public List<Enemy> CheckDead()
    {
        List<Enemy> deadList = new List<Enemy>();

        for (int i = 0; i < cells.Length; i++)
        {
            Enemy e = cells[i];
            if (e != null && e.IsDead)
            {
                Debug.Log($"{i}번 칸 적 처치");
                deadList.Add(e);
                cells[i] = null;
            }
        }

        return deadList;
    }

    public void Push(int target, int pushRange, float colDmg)
    {
        if (cells[target] == null)
        {
            return;
        }

        for (int i = 0; i < pushRange; i++)
        {
            if (i + target + 1 >= Size)
            {
                break;
            }

            if (cells[i+target+1] == null)
            {
                cells[i + target + 1] = cells[i + target];
                cells[i + target] = null;
            }
            else
            {
                cells[i+target].Damaged(cells[i + target].CollisionDamage(colDmg));
                cells[i+target+1].Damaged(cells[i + target+1].CollisionDamage(colDmg));
            }
        }
    }
    public void Pull(int target)
    {
        Enemy e_pulled = cells[target];

        if (e_pulled == null)
        {
            return;
        }

        for (int i = target-1; i >= 0; i--)
        {
            cells[i + 1] = cells[i];
        }

        cells[0] = e_pulled;
    }
}
