using System;
using System.Collections.Generic;

public class Player
{
    public const int MaxHP = 100;
    public int HP { get; private set; } = MaxHP;
    public int Shield { get; private set; }
    public bool IsDead => HP <= 0;

    public event Action OnChanged;

    public List<HitResult> PredictHit(List<EnemyAttack> attacks)
    {
        List<HitResult> realHits = new List<HitResult>();
        int blockCount = 0;

        foreach (EnemyAttack a in attacks)
        {
            bool blocked = false;
            if (Shield > blockCount)
            {
                blocked = true;
                blockCount++;
            }

            realHits.Add(new HitResult
            {
                Cell = a.Cell,
                Damage = a.Damage,
                Blocked = blocked
            }
            );
        }

        return realHits;
    }
    
    public void TakeHit(List<HitResult> hitResults)
    {
        foreach (HitResult r in hitResults)
        {
            if (r.Blocked)
            {
                Shield--;
            }
            else
            {
                HP -= r.Damage;
            }
        }

        OnChanged?.Invoke();
    }

    public void SetShield(int count) { Shield = count; OnChanged?.Invoke(); }
    public void Heal(int amount) { HP = System.Math.Min(MaxHP, HP + amount); OnChanged?.Invoke(); }
}
