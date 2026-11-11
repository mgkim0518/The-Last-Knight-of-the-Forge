using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner
{
    private readonly GameState state;
    private readonly System.Random rng;
    private readonly SpawnData spawn;
    private readonly BattleData foeData;
    private readonly EconomyData playData;
    private readonly HashSet<EnemyData> spawned = new HashSet<EnemyData>();

    private bool isShieldDeployed = false;

    public EnemySpawner(GameState gamestate)
    {
        state = gamestate;
        spawn = gamestate.Spawn;
        foeData = gamestate.BattleData;
        playData = gamestate.PlayData;        
        rng = gamestate.Rng;

        foreach (EnemyData e in spawn.EnemyPool)
        {
            if (e.SpawnTurn <= 0)
            {
                spawned.Add(e);
            }
        }
    }

    public Enemy CreateEnemy(int spawnTurn)
    {
        Enemy incoming = null;

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

        int weightSum = 0;

        foreach (EnemyData e in spawn.EnemyPool)
        {
            if (!spawned.Contains(e))
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

        if (!isShieldDeployed && spawn.FirstShieldTurn <= spawnTurn)
        {
            e.IsShielded = true;
            e.IsHavingFirstShield = true;
            isShieldDeployed = true;
        }
        else if (isShieldDeployed)
        {
            if (rng.NextDouble() < spawn.ShieldProb)
            {
                e.IsShielded = true;
            }
        }

        return e;
    }
}
