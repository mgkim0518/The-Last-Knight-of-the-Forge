using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner
{
    public int blockedCounter { get; private set; } = 0;

    private readonly GameState state;
    private readonly System.Random rng;
    private readonly SpawnData spawn;
    private readonly BattleData foeData;
    private readonly EconomyData playData;
    private readonly HashSet<EnemyData> spawned = new HashSet<EnemyData>();

    private bool isShieldDeployed = false;
    private EnemyData chargerData;

    public EnemySpawner(GameState gamestate)
    {
        state = gamestate;
        spawn = gamestate.Spawn;
        foeData = gamestate.BattleData;
        playData = gamestate.PlayData;        
        rng = gamestate.Rng;

        foreach (EnemyData e in spawn.EnemyPool)
        {
            if (e.EnemyType == EnemyType.Charge)
            {
                chargerData = e;
            }

            if (e.SpawnTurn <= 0)
            {
                spawned.Add(e);
            }
        }

        if (chargerData == null)
        {
            Debug.LogWarning("기 모으는 적이 적 목록에 없습니다.");
        }
    }

    public Enemy CreateEnemy(int spawnTurn)
    {
        Enemy incoming = BlockBuster(spawnTurn);

        if (incoming != null)
        {
            return incoming;
        }

        EnemyData e_Data = CheckDefinite(spawnTurn);

        if (e_Data != null)
        {
            incoming = SpawnFromData(e_Data, spawnTurn);
            incoming.IsFirstAppearance = true;
            spawned.Add(e_Data);
            return incoming;
        }

        e_Data = RollforEnemy();

        if (e_Data != null)
        {
            incoming = SpawnFromData(e_Data, spawnTurn);
            return incoming;
        }

        return null;
    }

    private EnemyData CheckDefinite(int spawnTurn)
    { 
        foreach (EnemyData e in spawn.EnemyPool)
        {
            if (spawned.Contains(e))
            {
                continue;
            }

            if (e.SpawnTurn <= spawnTurn)
            {
                return e;
            }    
        }

        return null;
    }

    private EnemyData RollforEnemy()
    {
        if (rng.NextDouble() >= spawn.SpawnProb)
        {
            return null;
        }

        bool isChargerExisting = ChargeE_onField();

        int weightSum = 0;       

        foreach (EnemyData e in spawn.EnemyPool)
        {
            if (!spawned.Contains(e))
            {
                continue;
            }

            if (e.EnemyType.Equals(EnemyType.Charge) && isChargerExisting)
            {
                continue;
            }

            weightSum += e.SpawnWeight;
        }

        if (weightSum <= 0)
        {
            Debug.LogWarning("소환할 적이 없습니다.");
            return null;
        }

        int mobIndex = rng.Next(weightSum);
        int cumulative = 0;

        foreach (EnemyData e in spawn.EnemyPool)
        {
            if (!spawned.Contains(e))
            {
                continue;
            }

            if (e.EnemyType.Equals(EnemyType.Charge) && isChargerExisting)
            {
                continue;
            }

            cumulative += e.SpawnWeight;

            if (mobIndex < cumulative)
            {
                return e;
            }
        }

        return null;
    }

    private Enemy SpawnFromData(EnemyData data, int spawnTurn)
    {
        int startTurn = spawnTurn - state.PrepTurns - 1;
        float healthMul = Mathf.Max(1, foeData.EnemyHealthCurve.Evaluate(startTurn));
        float bountyMul = Mathf.Max(1, playData.EnemyBountyCurve.Evaluate(startTurn));

        Enemy e = new Enemy(data, Mathf.RoundToInt(data.BaseHealth*healthMul)
            , Mathf.RoundToInt(data.Reward * bountyMul));

        if (!isShieldDeployed && e.Data.IsShieldAllowed && spawn.FirstShieldTurn <= spawnTurn)
        {
            e.IsShielded = true;
            e.IsHavingFirstShield = true;
            isShieldDeployed = true;
        }
        else if (isShieldDeployed && e.Data.IsShieldAllowed)
        {
            if (rng.NextDouble() < spawn.ShieldProb)
            {
                e.IsShielded = true;
            }
        }

        return e;
    }

    public void CheckBlocked(List<EnemyAttack> attacks)
    {
        bool isRangedAttacked = false;

        if (ChargeE_onField())
        {
            blockedCounter = 0;
            return;
        }

        foreach (EnemyAttack a in attacks)
        {
            if (!a.Enemy.Data.EnemyType.Equals(EnemyType.Ranged))
            {
                blockedCounter = 0;
                return;
            }
            else
            {
                isRangedAttacked = true;
            }
        }

        if (isRangedAttacked)
        {
            blockedCounter++;
        }
        else
        {
            blockedCounter = 0;
        }

        Debug.Log($"막힘 카운터: {blockedCounter}");
    }

    public bool ChargeE_onField()
    {
        for (int i = 0; i < Field.Size; i++)
        {
            if (state.Field.Get(i) == null)
            {
                continue;
            }
            else if (state.Field.Get(i).Data.EnemyType.Equals(EnemyType.Charge))
            {
                return true;
            }
        }

        return false;
    }

    public Enemy BlockBuster(int spawnTurn)
    {
        if (chargerData == null)
        {
            Debug.LogWarning("기 모으는 적이 적 목록에 없습니다.");
            return null;
        }

        if (blockedCounter >= spawn.BlockBusterDeploy && !ChargeE_onField())
        {
            Enemy chargeE = SpawnFromData(chargerData, spawnTurn);

            if (!spawned.Contains(chargerData))
            {
                chargeE.IsFirstAppearance = true;
                spawned.Add(chargerData);
            }

            return chargeE;
        }

        return null;
    }
}
