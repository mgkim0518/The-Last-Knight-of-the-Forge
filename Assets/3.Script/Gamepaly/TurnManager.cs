using System;
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
    [SerializeField] private int Debug_SkipTurnTo = 15;

    public event Action<Weapon> OnTargetSelect;
    public event Action OnChoiceFinished;

    private GameState state;
    private WeaponStatCalculator calc;
    private readonly bool[] spawnPattern = { true, false, true, true, false, true, false, false };
    private int turn_survived = 0;
    private Weapon selected;

    private void Start()
    {
        calc = new WeaponStatCalculator(battleData);
        StartNewGame();
    }

    private void StartNewGame()
    {
        state = new GameState(randomManager.GetSeed(), economyData, spawnData);
        state.AddWeapon(state.CreateRandomWeapon());
        lastHitTexts.Clear(); //GUI용 임시 코드
    }

    public void SelectWeapon(Weapon weapon)
    {
        if (weapon == null)
        {
            return;
        }

        if (weapon.Data.Target == TargetType.Choose ||
        weapon.Data.Target == TargetType.WithinReach)
        {
            selected = weapon;
            OnTargetSelect?.Invoke(weapon);
        }
        else
        {
            UseWeapon(weapon);
        }
    }

    public void SelectTarget(int enemyCell)
    {
        if (selected == null)
        {
            return;
        }

        if (UseWeapon(selected, enemyCell))
        {
            CancelTargetSelect();
        }
    }

    public void CancelTargetSelect()
    {
        selected = null;
        OnChoiceFinished?.Invoke();
    }

    public bool UseWeapon(Weapon weapon, int enemyCell = -1)
    {
        if (weapon == null)
        {
            Debug.LogWarning("빈 무기가 들어왔습니다");
            return false;
        }

        if (weapon.Data.Target == TargetType.Choose ||
        weapon.Data.Target == TargetType.WithinReach)
        {
            if (enemyCell.Equals(-1))
            {
                return false;
            }
        }

        List<int> targets = GetTargets(state.Field, weapon, enemyCell);
        if (targets.Count.Equals(0) && !weapon.Data.Target.Equals(TargetType.Self))
        {
            return false;
        }

        if (!state.RemoveWeapon(weapon))
        {
            return false;
        }

        if (weapon.Data.ChargeType.Equals(ChargeType.Defense))
        {
            state.Player.SetShield(calc.CalcHitRange(weapon));
        }

        int dmg = calc.CalcDamage(weapon);
        foreach (int cell in targets)
        {
            Enemy e = state.Field.Get(cell);
            if (e != null) e.Damaged(dmg);
        }

        if (weapon.Data.Reloc.Equals(Relocation.Push))
        {
            int pushRange = calc.CalcWEffect(weapon);
            state.Field.Push(targets[0], pushRange, battleData.CollisionDamagePercent);
        }
        else if (weapon.Data.Reloc.Equals(Relocation.Pull))
        {
            state.Field.Pull(enemyCell);
        }

        state.Field.CheckDead();

        Debug.Log("사용한 무기: " + weapon.Data.WeaponName);

        EndTurn();
        return true;
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
                if (enemyCell >= 0 && enemyCell < Field.Size
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
        List<HitResult> validHits = state.Player.PredictHit(state.Field.ActAll());
        state.Player.TakeHit(validHits);
        lastHitTexts = ShowHits(validHits); //GUI에 넘기기
        if (state.Player.IsDead)
        {
            turn_survived = state.Turn;
            StartNewGame();
            return;
        }
        Debug.Log("다음 턴 준비");
        TrySpawnEnemy();
        state.NextTurn();
    }

    private void TrySpawnEnemy() //몬스터 스폰용 임시 코드
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

    private List<string> lastHitTexts = new List<string>();

    private void OnGUI()
    {
        if (state == null) return;

        GUILayout.BeginArea(new Rect(10, 40, 300, 600));
        GUILayout.Label($"지난 판 {(turn_survived == 0 ? "기록 없음" : turn_survived + "턴")}");
        GUILayout.Label($"턴: {state.Turn}");

        int clickedCell = -1;
        bool isCancelled = false;

        if (selected != null)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("[P]");
            for (int i = 0; i < Field.Size; i++)
            {
                var e = state.Field.Get(i);
                GUI.enabled = GetTargets(state.Field, selected, i).Count > 0;
                if (GUILayout.Button(e == null ? "_" : $"E{e.HP}"))
                    clickedCell = i;
            }
            GUILayout.EndHorizontal();

            GUI.enabled = true;
            isCancelled = GUILayout.Button("취소");
        }
        else
        {
            GUILayout.Label(FieldToString(state.Field));
        }

        GUILayout.Label($"체력: {state.Player.HP}/{Player.MaxHP}  보호막: {state.Player.Shield}");
        GUILayout.Label("지난 적 행동");
        foreach (string t in lastHitTexts)
            GUILayout.Label("  " + t);

        GUI.enabled = selected == null;
        Weapon clicked = null;

        for (int i = 0; i < state.Inventory.Length; i++)
        {
            var w = state.Inventory[i];
            if (w == null)
            {
                GUILayout.Label($"[{i}] 빈칸");
                continue;
            }

            if (GUILayout.Button($"[{i}] {w.Data.WeaponName} (+{w.CurStack}) {ShowEffect(w)}" +
                $"\n{ChargeGauge(w)}"))
                clicked = w;
        }

        string next = state.NextWeapon != null ? state.NextWeapon.Data.WeaponName : "없음";
        GUILayout.Label($"다음: {next}");

        bool rest = GUILayout.Button("쉬기");
        bool debug_skipturn = GUILayout.Button($"{Debug_SkipTurnTo}턴으로 이동");
        GUI.enabled = true;
        GUILayout.EndArea();

        if (clickedCell != -1) SelectTarget(clickedCell);
        else if (isCancelled) CancelTargetSelect();
        else if (clicked != null)
        {
            SelectWeapon(clicked);
        }
        else if (rest) Rest();
        else if (debug_skipturn) state.Debug_SkipTurn(Debug_SkipTurnTo);
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

    private List<string> ShowHits(List<HitResult> validHits)
    {
        List<string> DmgResult = new List<string>();

        foreach (HitResult r in validHits)
        {
            if (r.Blocked)
            {
                DmgResult.Add($"{r.Cell + 1}번 적 공격 막힘");
            }
            else
            {
                DmgResult.Add($"{r.Cell + 1}번 적이 입힌 데미지: {r.Damage}");
            }
        }

        if (DmgResult.Count.Equals(0))
        {
            DmgResult.Add("공격하는 적 없음");
            return DmgResult;
        }

        return DmgResult;
    }

    private string ShowEffect(Weapon weapon)
    {
        string effectLabel = string.Empty;

        switch (weapon.Data.ChargeType)
        {
            case ChargeType.Power:
                effectLabel = $"위력 {calc.CalcChargeValue(weapon)}";
                break;
            case ChargeType.Area:
                effectLabel = $"위력 {calc.CalcDamage(weapon)}, {calc.CalcChargeValue(weapon)}칸";
                break;
            case ChargeType.Distance:
                if (weapon.Data.Reloc.Equals(Relocation.Push))
                {
                    effectLabel = $"위력 {calc.CalcDamage(weapon)}, {calc.CalcChargeValue(weapon)}칸 밀치기";
                }
                else
                {
                    effectLabel = $"위력 {calc.CalcDamage(weapon)}, {calc.CalcChargeValue(weapon)}칸 내 대상 끌어오기";
                }
                break;
            case ChargeType.Defense:
                effectLabel = $"보호막 {calc.CalcChargeValue(weapon)}회";
                break;
        }

        return effectLabel;
    }

    private string ChargeGauge(Weapon weapon)
    {
        int cap = battleData.GetCap(weapon.Data.CapTier);
        var sb = new System.Text.StringBuilder();

        for (int i = 0; i < cap; i++)
        {
            sb.Append(i < weapon.CurStack ? "■" : "□");
        }

        return sb.ToString();
    }
}
