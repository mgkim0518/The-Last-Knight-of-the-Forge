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
    [SerializeField] private int prepTurns = 3;
    [SerializeField] private int Debug_SkipTurnTo = 15;
    [SerializeField] private int Debug_FreeIron = 100;

    public event Action<Weapon> OnTargetSelect;
    public event Action OnChoiceFinished;
    public event Action<Weapon> OnMelting;
    public event Action<int> OnCurrencyChange;
    public event Action<IReadOnlyList<WeaponData>> OnWeightChoiceOpen;
    public event Action OnWeightChoiceClose;

    public bool IsInputBlocked => state != null && state.WeaponSelections.Count > 0;

    private GameState state;
    private int turn_survived = 0;
    private Weapon selected;
    private Weapon hovered;   // 마우스를 올린 무기 (OnGUI 임시)
    private int GUI_Iron;
    private int GUI_HealTo;
    private int GUI_LastHP;

    private void Start()
    {
        OnCurrencyChange += iron => GUI_Iron = iron;
        StartNewGame();
    }

    private void OnDestroy()
    {
        state.OnIronChanged -= ChangeIronValUI;
    }

    private void StartNewGame()
    {
        if (state != null)
        {
            state.OnIronChanged -= ChangeIronValUI;
        }
        state = new GameState(randomManager.GetSeed(), economyData, spawnData,
            prepTurns, battleData);
        state.AddWeapon(state.CreateRandomWeapon());
        state.OnIronChanged += ChangeIronValUI;
        OnCurrencyChange?.Invoke(state.Iron);
        lastHitTexts.Clear(); //GUI용 임시 코드
    }

    public void SelectWeapon(Weapon weapon)
    {
        if (IsInputBlocked)
        {
            return;
        }

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
        if (IsInputBlocked)
        {
            return false ;
        }

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
            state.Player.SetShield(state.Calc.CalcHitRange(weapon));
        }

        int dmg = state.Calc.CalcDamage(weapon);
        foreach (int cell in targets)
        {
            Enemy e = state.Field.Get(cell);
            if (e != null) e.Damaged(dmg);
        }

        if (weapon.Data.Reloc.Equals(Relocation.Push))
        {
            int pushRange = state.Calc.CalcWEffect(weapon);
            state.Field.Push(targets[0], pushRange, state.BattleData.CollisionDamagePercent);
        }
        else if (weapon.Data.Reloc.Equals(Relocation.Pull))
        {
            state.Field.Pull(enemyCell);
        }

        state.ClearDeadEnemies();

        Debug.Log("사용한 무기: " + weapon.Data.WeaponName);

        EndTurn();
        return true;
    }

    private List<int> GetTargets(Field field, Weapon weapon, int enemyCell)
    {
        int hitRange = state.Calc.CalcHitRange(weapon);

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
        if (IsInputBlocked)
        {
            return;
        }

        foreach (Weapon w in state.Inventory)
        {
            if (w == null) continue;

            if (w.CurStack < state.BattleData.GetCap(w.Data.CapTier))
            {
                w.CurStack++;
            }
        }
        EndTurn();
    }

    public bool TryMelt(Weapon weapon)
    {
        if (IsInputBlocked)
        {
            return false;
        }

        if (selected != null)
        {
            return false;
        }

        if (!state.Melt(weapon))
        {
            return false;
        }

        OnMelting?.Invoke(weapon);
        Debug.Log(weapon.Data.WeaponName + "을 녹였습니다.");
        return true;
    }

    public bool TryUpgrade(WeaponData data)
    {
        if (IsInputBlocked)
        {
            return false;
        }

        if (selected != null)
        {
            return false;
        }

        int nextLevel = state.Forge.GetUpgradeLevel(data) + 1;
        return state.Forge.PaytoUpgrade(data, nextLevel);
    }

    public bool TryHeal(int healTarget)
    {
        if (IsInputBlocked)
        {
            return false;
        }

        if (selected != null)
        {
            return false;
        }

        return state.Forge.PayToHeal(healTarget);
    }

    public bool TryExpand()
    {
        if (IsInputBlocked)
        {
            return false;
        }

        if (selected != null)
        {
            return false;
        }

        return state.Forge.PayToExpand();
    }

    private bool TryOpenChoice()
    {
        if (spawnData.UpWeightTurn <= 0)
        {
            return false;
        }

        if (state.Turn % spawnData.UpWeightTurn != 0)
        {
            return false;
        }

        state.CreateWeaponChoice();

        if (state.WeaponSelections.Count.Equals(0))
        {
            return false;
        }

        OnWeightChoiceOpen?.Invoke(state.WeaponSelections);
        return true;
    }

    public bool ChooseWeightInc(int index)
    {
        if (state.ChooseWeightUp(index))
        {
            OnWeightChoiceClose?.Invoke();
            return true;
        }

        return false;
    }

    public bool SkipWeightInc()
    {
        if (state.SkipWeightChoice())
        {
            OnWeightChoiceClose?.Invoke();
            return true;
        }

        return false;
    }

    private void EndTurn()
    {
        if (!state.ReceiveNextWeapon())
        {
            Debug.Log("슬롯이 가득 차 무기 소멸");
        }
        Debug.Log("적 행동");
        List<EnemyAttack> attacks = state.Field.ActAll(state.Field.PlanAction());
        state.Spawner.CheckBlocked(attacks);
        List<HitResult> validHits = state.Player.PredictHit(attacks);
        state.Player.TakeHit(validHits);
        lastHitTexts = ShowHits(validHits); //GUI에 넘기기
        if (state.Player.IsDead)
        {
            turn_survived = state.Turn;
            StartNewGame();
            return;
        }
        Debug.Log("다음 턴 준비");
        state.NextTurn();
        state.TrySpawnEnemy();
        TryOpenChoice();
    }

    private void ChangeIronValUI(int iron)
    {
        OnCurrencyChange?.Invoke(iron);
    }

    public List<int> GetHitCells(Weapon weapon)
    {
        List<int> atkRange = new List<int>();

        if (weapon == null)
        {
            return atkRange;
        }

        if (weapon.Data.Target.Equals(TargetType.Choose)
            || weapon.Data.Target.Equals(TargetType.WithinReach))
        {
            for (int i = 0; i < Field.Size; i++)
            {
                if (GetTargets(state.Field, weapon, i).Count > 0)
                {
                    atkRange.Add(i);
                }

            }
        }
        else
        {
            atkRange = GetTargets(state.Field, weapon, -1);
        }

        return atkRange;
    }

    private List<string> lastHitTexts = new List<string>();

    private void OnGUI()
    {
        if (state == null) return;
        List<NextAction> nextMove_E = state.Field.PredictAct(state.Field.PlanAction());

        GUILayout.BeginArea(new Rect(10, 40, 500, 600));
        GUILayout.Label($"지난 판 {(turn_survived == 0 ? "기록 없음" : turn_survived + "턴")}");

        GUILayout.BeginHorizontal();
        GUILayout.Label($"턴: {state.Turn}");
        GUILayout.Label($"철괴: {GUI_Iron}");
        GUILayout.EndHorizontal();

        int clickedCell = -1;
        bool isCancelled = false;
        string nextEnemy = "없음";
        bool canAct = selected == null && !IsInputBlocked;

        if (state.NextEnemy != null)
        {
            nextEnemy = EnemyToString(state.NextEnemy);

            if (state.NextEnemy.IsFirstAppearance)
            {
                nextEnemy += " (첫 등장!)";
            }

            if (state.NextEnemy.IsHavingFirstShield)
            {
                nextEnemy += " (방패 등장!)";
            }
        }

        if (selected != null)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("[P]");
            for (int i = 0; i < Field.Size; i++)
            {
                var e = state.Field.Get(i);
                GUI.enabled = GetTargets(state.Field, selected, i).Count > 0;
                if (GUILayout.Button(e == null ? "_" : $"{EnemyToString(e)}"))
                    clickedCell = i;
            }
            GUI.enabled = true;

            GUILayout.Label($" | 등장: {nextEnemy}");

            GUILayout.EndHorizontal();

            isCancelled = GUILayout.Button("취소");
        }
        else
        {
            GUILayout.Label(FieldToString(state.Field, GetHitCells(hovered), nextMove_E) + 
                $" | 등장: {nextEnemy}");
        }

        List<EnemyAttack> attacks = new List<EnemyAttack>();
        foreach (NextAction act in nextMove_E)
        {
            Enemy e = state.Field.Get(act.Pos);

            if (act.Behav.Equals(EnemyBehav.Attack))
            {
                attacks.Add(new EnemyAttack { Cell = act.Pos, Damage = act.ChangeVal });
            }
        }

        List<HitResult> validHits = state.Player.PredictHit(attacks);

        int damage = 0;
        foreach (HitResult r in validHits)
        {
            if (!r.Blocked)
            {
                damage += r.Damage;
            }
        }

        string expected = damage > 0 ? $" (예상 -{damage})" : "";

        GUILayout.Label($"체력: {state.Player.HP}/{Player.MaxHP}{expected} " +
            $" 보호막: {state.Player.Shield}");

        GUILayout.BeginHorizontal();

        bool forHeal = false;
        Color prevColor = GUI.contentColor;

        if (state.Player.HP != GUI_LastHP)
        {
            GUI_HealTo = state.Player.HP;
            GUI_LastHP = state.Player.HP;
        }

        GUI.enabled = canAct;
        GUI_HealTo = Mathf.RoundToInt(GUILayout.HorizontalSlider
            (GUI_HealTo, 0, Player.MaxHP, GUILayout.Width(200)));
        GUI_HealTo = Mathf.Clamp(GUI_HealTo, state.Player.HP, Player.MaxHP);

        int healPrice = state.Forge.GetHealPrice(GUI_HealTo);

        if (healPrice > state.Iron)
        {
            GUI.contentColor = Color.red;
        }
        GUILayout.Label($"{state.Player.HP} → {GUI_HealTo} (+{GUI_HealTo - state.Player.HP})" +
            $" · {healPrice} 철괴");
        GUI.contentColor = prevColor;

        GUI.enabled = canAct && state.Forge.CheckHealable(GUI_HealTo);

        if (GUILayout.Button("회복", GUILayout.Width(80)))
        {
            forHeal = true;
        }

        GUI.enabled = canAct;

        GUILayout.EndHorizontal();

        GUILayout.Label("지난 적 행동");
        foreach (string t in lastHitTexts)
            GUILayout.Label("  " + t);

        GUI.enabled = canAct;
        Weapon clicked = null;
        Weapon forMelt = null;
        WeaponData forUpgrade = null;
        bool forExpand = false;

        bool isRepaint = Event.current.type == EventType.Repaint;
        if (isRepaint) hovered = null;

        for (int i = 0; i < state.Inventory.Length; i++)
        {
            var w = state.Inventory[i];
            if (w == null)
            {
                GUILayout.Label($"[{i}] 빈칸");
                continue;
            }

            GUILayout.BeginHorizontal();

            if (GUILayout.Button($"[{i}] {w.Data.WeaponName} Lv {state.Forge.GetUpgradeLevel(w.Data)}" +
                $" (+{w.CurStack}) {ShowEffect(w)}" +
                $"\n{ChargeGauge(w)}"))
                clicked = w;

            if (isRepaint && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                hovered = w;

            GUI.enabled = canAct && state.MeltLeft > 0;
            if (GUILayout.Button($"녹이기 +{economyData.GetMeltValue(w)}",
                GUILayout.Width(80), GUILayout.Height(40)))
                forMelt = w;

            int nextLevel = state.Forge.GetUpgradeLevel(w.Data) + 1;
            GUI.enabled = canAct && state.Forge.CheckUpgradeable(w.Data, nextLevel);
            if (GUILayout.Button($"강화 {state.Forge.GetUpgradePrice(w.Data, nextLevel)}",
                GUILayout.Width(80), GUILayout.Height(40)))
                forUpgrade = w.Data;

            GUI.enabled = canAct;

            GUILayout.EndHorizontal();
        }

        GUI.enabled = canAct && state.Forge.CheckExpandable();
        if (GUILayout.Button($"슬롯 확장 {state.Forge.GetSlotPrice()}"))
        {
            forExpand = true;
        }

        GUILayout.BeginHorizontal();

        string nextWeapon = state.NextWeapon != null ? state.NextWeapon.Data.WeaponName : "없음";
        GUILayout.Label($"다음: {nextWeapon}");

        GUI.enabled = canAct && state.MeltLeft > 0 && state.NextWeapon != null;
        string meltVal = state.NextWeapon != null ? $" +{economyData.GetMeltValue(state.NextWeapon)}"
            : "";
        if (GUILayout.Button($"녹이기{meltVal}", GUILayout.Width(80)))
            forMelt = state.NextWeapon;
        GUI.enabled = canAct;

        GUILayout.EndHorizontal();

        GUILayout.Label($"용광로 {state.MeltLeft} / {state.PlayData.FurnaceCap}");

        bool rest = GUILayout.Button("쉬기");

        GUI.enabled = selected == null;
        bool debug_skipturn = GUILayout.Button($"{Debug_SkipTurnTo}턴으로 이동");
        bool debug_addIron = GUILayout.Button($"철괴 {Debug_FreeIron}개");
        GUI.enabled = true;
        GUILayout.EndArea();
        int w_choice = ChoiceButton(out bool isSkipped);

        if (clickedCell != -1) SelectTarget(clickedCell);
        else if (isCancelled) CancelTargetSelect();
        else if (clicked != null)
        {
            SelectWeapon(clicked);
        }
        else if (rest) Rest();
        else if (debug_skipturn) state.Debug_SkipTurn(Debug_SkipTurnTo);
        else if (forMelt != null) TryMelt(forMelt);
        else if (forUpgrade != null) TryUpgrade(forUpgrade);
        else if (forHeal) TryHeal(GUI_HealTo);
        else if (forExpand) TryExpand();
        else if (debug_addIron) state.AddIron(Debug_FreeIron);
        else if (w_choice != -1) ChooseWeightInc(w_choice);
        else if (isSkipped) SkipWeightInc();
    }

    private string FieldToString(Field field, List<int> highlight, List<NextAction> next)
    {
        string[] marks = new string[Field.Size];
        for (int i = 0; i < Field.Size; i++)
        {
            marks[i] = string.Empty;
        }

        foreach (NextAction act in next)
        {
            switch (act.Behav)
            {
                case EnemyBehav.Attack:
                    marks[act.Pos] = $"!{act.ChangeVal}";
                    break;
                case EnemyBehav.Move:
                    marks[act.Pos] = "←";
                    break;
                case EnemyBehav.Heal:
                    marks[act.Pos] = $"←+{act.ChangeVal}";
                    break;
                case EnemyBehav.Wait:
                    marks[act.Pos] = "…";
                    break;
                case EnemyBehav.DecidedFleeing:
                    marks[act.Pos] = $"→{act.ChangeVal}";
                    break;
                case EnemyBehav.Flee:
                    marks[act.Pos] = "→!";
                    break;
                case EnemyBehav.Charging:
                    marks[act.Pos] = $"(!{act.ChangeVal})*";
                    break;
            }
        }

        var sb = new System.Text.StringBuilder("[P]");
        for (int i = 0; i < Field.Size; i++)
        {
            var e = field.Get(i);
            string cell = e == null ? "_" : $"{EnemyToString(e)}{marks[i]}";
            sb.Append(highlight.Contains(i) ? $" [{cell}]" : $" {cell}");
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
                effectLabel = $"위력 {state.Calc.CalcChargeValue(weapon)}";
                break;
            case ChargeType.Area:
                effectLabel = $"위력 {state.Calc.CalcDamage(weapon)}, " +
                    $"{state.Calc.CalcChargeValue(weapon)}칸";
                break;
            case ChargeType.Distance:
                if (weapon.Data.Reloc.Equals(Relocation.Push))
                {
                    effectLabel = $"위력 {state.Calc.CalcDamage(weapon)}, " +
                        $"{state.Calc.CalcChargeValue(weapon)}칸 밀기";
                }
                else
                {
                    effectLabel = $"위력 {state.Calc.CalcDamage(weapon)}, " +
                        $"{state.Calc.CalcChargeValue(weapon)}칸 내 대상 끌기";
                }
                break;
            case ChargeType.Defense:
                effectLabel = $"보호막 {state.Calc.CalcChargeValue(weapon)}회";
                break;
        }

        return effectLabel;
    }

    private string ChargeGauge(Weapon weapon)
    {
        int cap = state.BattleData.GetCap(weapon.Data.CapTier);
        var sb = new System.Text.StringBuilder();

        for (int i = 0; i < cap; i++)
        {
            sb.Append(i < weapon.CurStack ? "■" : "□");
        }

        return sb.ToString();
    }

    private string EnemyToString(Enemy enemy)
    {
        char type = 'e';
        switch (enemy.Data.EnemyType)
        {
            case EnemyType.Normal:
                type = 'N';
                break;
            case EnemyType.Ranged:
                type = 'R';
                break;
            case EnemyType.Heal:
                type = 'H';
                break;
            case EnemyType.Charge:
                type = 'C';
                break;
        }

        string shield = enemy.IsShielded ? "◆" : string.Empty;

        string charge = enemy.IsLoaded ? "*" : string.Empty;

        return $"{type}{enemy.HP}{shield}{charge}";
    }

    private int ChoiceButton(out bool isSkipped)
    {
        isSkipped = false;

        if (!IsInputBlocked)
        {
            return -1;
        }

        int clickedIndex = -1;
        float width = 300f;
        float height = 200f;
        Rect area = new Rect((Screen.width - width) / 2f, (Screen.height - height) / 2f, width, height);

        GUILayout.BeginArea(area, GUI.skin.box);
        GUILayout.Label("등장 확률을 올릴 무기를 고르세요");

        IReadOnlyList<WeaponData> choices = state.WeaponSelections;
        List<int> weights = state.GetChoiceWeights();

        for (int i = 0; i < choices.Count; i++)
        {
            int nextWeight = Mathf.Min(weights[i] + spawnData.WeightIncrease, spawnData.MaxSpawnWeight);

            if (GUILayout.Button($"{choices[i].WeaponName} (가중치 {weights[i]} → {nextWeight})"))
            {
                clickedIndex = i;
            }
        }

        if (GUILayout.Button($"건너뛰기 (철괴 +{economyData.WeightSkipReward})"))
        {
            isSkipped = true;
        }

        GUILayout.EndArea();
        return clickedIndex;

    }
}
