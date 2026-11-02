using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    [SerializeField] private EconomyData economyData;
    [SerializeField] private RandomManager randomManager;
    [SerializeField] private WeaponData[] weaponPool;
    [SerializeField] private EnemyData normalEnemy;
    [SerializeField] private int prepTurns = 3;

    private GameState state;
    private readonly bool[] spawnPattern = { true, false, true, true, false, true, false, false };


    private void Start()
    {
        StartNewGame();
    }

    private void StartNewGame()
    {
        state = new GameState(randomManager.GetSeed(), economyData);
        state.AddWeapon(CreateRandomWeapon());
    }

    public void UseWeapon(Weapon weapon, int enemyCell = -1)
    {
        if (weapon == null)
        {
            return;
        }


        if (weapon.Data.Target == TargetType.Choose)
        {
            if (enemyCell.Equals(-1))
            {
                return;
            }
        }

        if (!state.RemoveWeapon(weapon))
        {
            return;
        }

        Debug.Log("사용한 무기: " + weapon.Data.WeaponName);

        GetTargets(weapon, enemyCell);

        EndTurn();
    }

    private void GetTargets(Weapon weapon, int enemyCell)
    {
        Debug.Log("슬라임이 선택되었습니다");
    }

    public void Rest()
    {
        Debug.Log("쉬기");
        EndTurn();
    }

    public void EndTurn()
    {
        Debug.Log("아이템 획득");
        state.AddWeapon(CreateRandomWeapon());
        Debug.Log("적 행동");
        int p_hit = state.Field.ActAll();
        Debug.Log($"받은 피해: {p_hit}");
        Debug.Log("다음 턴 준비");
        TrySpawnEnemy();
        state.NextTurn();
    }

    private Weapon CreateRandomWeapon()
    {
        var data = weaponPool[state.Rng.Next(weaponPool.Length)];
        return new Weapon(data);
    }

    private void TrySpawnEnemy()
    {
        if (state.Turn < prepTurns) return;
        if (!state.Field.IsEntranceFree) return;   // 입구 막힘 → 패턴 진행 안 하고 대기

        bool spawn = spawnPattern[state.SpawnIndex % spawnPattern.Length];
        state.SpawnIndex++;

        if (spawn)
            state.Field.TrySpawn(new Enemy(normalEnemy, normalEnemy.BaseHealth));
    }

    private void OnGUI()
    {
        if (state == null) return;

        GUILayout.BeginArea(new Rect(10, 40, 300, 400));
        GUILayout.Label($"턴: {state.Turn}");
        GUILayout.Label(FieldToString(state.Field));

        Weapon clicked = null;

        for (int i = 0; i < state.Inventory.Length; i++)
        {
            var w = state.Inventory[i];
            if (w == null)
            {
                GUILayout.Label($"[{i}] 빈칸");
                continue;
            }

            if (GUILayout.Button($"[{i}] {w.Data.WeaponName} (+{w.CurStack})"))
                clicked = w;
        }

        bool rest = GUILayout.Button("쉬기");
        GUILayout.EndArea();

        if (clicked != null) UseWeapon(clicked, 1);   // 석궁 테스트용으로 1번 칸 지정
        else if (rest) Rest();
    }

    private string FieldToString(Field field)
    {
        var sb = new System.Text.StringBuilder("[P]");
        for (int i = 0; i < Field.Size; i++)
        {
            var e = field.Get(i);
            sb.Append(e == null ? " _" : $" E{e.HP}");
        }
        return sb.ToString();
    }
}
