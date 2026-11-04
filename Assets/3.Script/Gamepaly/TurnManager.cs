using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    [SerializeField] private EconomyData economyData;
    [SerializeField] private BattleData battleData;
    [SerializeField] private SpawnData spawnData;
    [SerializeField] private RandomManager randomManager;
    [SerializeField] private EnemyData normalEnemy;
    [SerializeField] private int prepTurns = 3;

    private GameState state;
    private WeaponStatCalculator calc;
    private readonly bool[] spawnPattern = { true, false, true, true, false, true, false, false };

    private void Start()
    {
        calc = new WeaponStatCalculator(battleData);
        StartNewGame();
    }

    private void StartNewGame()
    {
        state = new GameState(randomManager.GetSeed(), economyData, spawnData);
        state.AddWeapon(state.CreateRandomWeapon());
    }

    public void UseWeapon(Weapon weapon, int enemyCell = -1)
    {
        if (weapon == null)
        {
            return;
        }

        if (weapon.Data.Target == TargetType.Choose ||
        weapon.Data.Target == TargetType.WithinReach)
        {
            if (enemyCell.Equals(-1))
            {
                return;
            }
        }

        List<int> targets = GetTargets(state.Field, weapon, enemyCell);
        if (targets.Count.Equals(0) && !weapon.Data.Target.Equals(TargetType.Self))
        {
            return;
        }

        if (!state.RemoveWeapon(weapon))
        {
            return;
        }

        int dmg = calc.DamageCalc(weapon);
        foreach (int cell in targets)
        {
            Enemy e = state.Field.Get(cell);
            if (e != null) e.Damaged(dmg);
        }
        state.Field.CheckDead();

        Debug.Log("사용한 무기: " + weapon.Data.WeaponName);

        EndTurn();
    }

    private List<int> GetTargets(Field field, Weapon weapon, int enemyCell)
    {
        int hitRange = calc.CalcHitRange(weapon);

        List<int> w_target = new List<int>();

        switch (weapon.Data.Target)
        {
            case TargetType.Front:
                {
                    int start = -1;
                    for (int i = 0; i < Field.Size; i++)
                    {
                        if (field.Get(i) != null)
                        {
                            start = i; break;
                        }
                    }

                    if (start == -1)
                    {
                        break;
                    }

                    for (int i = start; i < Field.Size && i < start + hitRange; i++)
                    {
                        if (field.Get(i) != null)
                        {
                            w_target.Add(i);
                        }
                    }
                    break;
                }
            case TargetType.Back:
                {
                    int start = -1;
                    for (int i = Field.Size - 1; i >= 0; i--)
                    {
                        if (field.Get(i) != null)
                        {
                            start = i;
                            break;
                        }
                    }

                    if (start == -1)
                    {
                        break;
                    }

                    for (int i = start; i >= 0 && i > start - hitRange; i--)
                    {
                        if (field.Get(i) != null)
                        {
                            w_target.Add(i);
                        }
                    }
                    break;
                }

            case TargetType.Choose:
                if (enemyCell >= 0 && enemyCell < hitRange && enemyCell < Field.Size
        && field.Get(enemyCell) != null)
                {
                    w_target.Add(enemyCell);
                }
                break;
            case TargetType.Self:
                break;
            case TargetType.WithinReach:
                if (enemyCell >= 0 && enemyCell < hitRange && enemyCell < Field.Size
        && field.Get(enemyCell) != null)
                {
                    w_target.Add(enemyCell);
                }
                break;
        }

        return w_target;

    }

    public void Rest()
    {
        foreach (Weapon w in state.Inventory)
        {
            if (w == null) continue;

            if (w.CurStack < battleData.GetCap(w.Data.CapTier))
            {
                w.CurStack++;
            }
        }
        EndTurn();
    }

    private void EndTurn()
    {
        if (!state.ReceiveNextWeapon())
        {
            Debug.Log("슬롯이 가득 차 무기 소멸");
        }
        Debug.Log("적 행동");
        int p_hit = state.Field.ActAll();
        Debug.Log($"받은 피해: {p_hit}");
        Debug.Log("다음 턴 준비");
        TrySpawnEnemy();
        state.NextTurn();
    }

    private void TrySpawnEnemy()
    {
        if (state.Turn < prepTurns) return;
        if (!state.Field.IsEntranceFree) return;   // 입구 막힘 → 패턴 진행 안 하고 대기

        bool spawn = spawnPattern[state.SpawnIndex % spawnPattern.Length];
        state.SpawnIndex++;

        if (spawn)
        {
            state.Field.TrySpawn(new Enemy(normalEnemy, normalEnemy.BaseHealth));
        }
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

            if (GUILayout.Button($"[{i}] {w.Data.WeaponName} (+{w.CurStack}) 위력 {calc.DamageCalc(w)}"))
                clicked = w;
        }

        string next = state.NextWeapon != null ? state.NextWeapon.Data.WeaponName : "없음";
        GUILayout.Label($"다음: {next}");

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
