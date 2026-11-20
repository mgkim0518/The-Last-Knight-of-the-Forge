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

    public event Action OnStateChanged;
    public event Action<Weapon> OnTargetSelect;
    public event Action OnChoiceFinished;
    public event Action<Weapon> OnMelting;
    public event Action<int> OnCurrencyChange;
    public event Action<IReadOnlyList<WeaponData>> OnWeightChoiceOpen;
    public event Action OnWeightChoiceClose;
    public event Action<int> OnGameOver;

    public bool IsInputBlocked => state != null && 
        (state.WeaponSelections.Count > 0 || isGameOver);
    public bool IsMeltAvailable => !IsInputBlocked && state.MeltLeft > 0;
    public bool IsCancelledThisFrame => lastCancelFrame == Time.frameCount;

    private GameState state;
    private int turn_survived = 0;
    private Weapon selected;
    private bool isGameOver = false;
    private int GUI_HealTo;
    private int GUI_LastHP;
    private int lastCancelFrame = -1;

    private void Start()
    {
        StartNewGame();
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
        OnStateChanged?.Invoke();
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

        if (weapon.Equals(selected))
        {
            CancelTargetSelect();
            return;
        }

        if (weapon.Data.Target == TargetType.Choose ||
        weapon.Data.Target == TargetType.WithinReach)
        {
            selected = weapon;
            OnTargetSelect?.Invoke(weapon);
            OnStateChanged?.Invoke();
        }
        else
        {
            UseWeapon(weapon);
        }
    }

    public string EffectText(Weapon weapon)
    {
        string effectLabel = string.Empty;

        switch (weapon.Data.ChargeType)
        {
            case ChargeType.Power:
            case ChargeType.Area:
                effectLabel = $"-{state.Calc.CalcDamage(weapon)}, {TargetText(weapon)}";
                break;
            case ChargeType.Distance:
                if (weapon.Data.Reloc.Equals(Relocation.Push))
                {
                    effectLabel = $"-{state.Calc.CalcDamage(weapon)}, " +
                        $"{state.Calc.CalcChargeValue(weapon)}칸 밀기";
                }
                else
                {
                    effectLabel = $"-{state.Calc.CalcDamage(weapon)}, " +
                        $"사거리 {state.Calc.CalcChargeValue(weapon)} 끌기";
                }
                break;
            case ChargeType.Defense:
                effectLabel = $"보호막 {state.Calc.CalcChargeValue(weapon)}회";
                break;
        }

        return effectLabel;
    }

    private string TargetText(Weapon weapon)
    {
        int hit = state.Calc.CalcHitRange(weapon);

        switch (weapon.Data.Target)
        {
            case TargetType.Front:
                return hit > 1 ? $"앞 {hit}칸" : "맨 앞";
            case TargetType.Back:
                return hit > 1 ? $"뒤 {hit}칸" : "맨 뒤";
            case TargetType.Choose:
                return "지정";
            default:
                return string.Empty;
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
        lastCancelFrame = Time.frameCount;
        OnStateChanged?.Invoke();
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
        state.PickUpShields();

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

    public int ExpectedDamage()
    {
        List<EnemyAttack> attacks = new List<EnemyAttack>();
        foreach (NextAction act in E_ActsNow)
        {
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

        return damage;
    }

    public void Rest()
    {
        if (IsInputBlocked)
        {
            return;
        }

        if (selected != null)
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

    public void Restart()
    {
        selected = null;
        isGameOver = false;
        StartNewGame();
    }

    #region TurnManager에서 다른 메소드 받는 영역

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
        OnStateChanged?.Invoke();
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
        bool isUpgraged = state.Forge.PaytoUpgrade(data, nextLevel);
        if (isUpgraged)
        {
            OnStateChanged?.Invoke();
        }
        return isUpgraged;
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

        bool isHealed = state.Forge.PayToHeal(healTarget);
        if (isHealed)
        {
            OnStateChanged?.Invoke();
        }
        return isHealed;
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

        bool isExpanded = state.Forge.PayToExpand();
        if (isExpanded)
        {
            OnStateChanged?.Invoke();
        }
        return isExpanded;
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
            OnStateChanged?.Invoke();
            return true;
        }

        return false;
    }

    public bool SkipWeightInc()
    {
        if (state.SkipWeightChoice())
        {
            OnWeightChoiceClose?.Invoke();
            OnStateChanged?.Invoke();
            return true;
        }

        return false;
    }

    #endregion

    #region UI용 Getter

    public int TurnNow => state.Turn; 
    public int IronNow => state.Iron; 
    public int HealthNow => state.Player.HP; 
    public int MaxHPNow => Player.MaxHP; 
    public int ShieldNow => state.Player.Shield;
    public Weapon[] InventoryNow => state.Inventory;
    public int MaxSlotsNow => state.PlayData.MaxSlots;
    public int SlotPriceNow => state.Forge.GetSlotPrice();
    public Weapon NextWeaponNow => state.NextWeapon;
    public Weapon SelectedNow => selected;
    public int CapNow(WeaponData data) => state.BattleData.GetCap(data.CapTier);
    public Enemy EnemyNow(int i) => state.Field.Get(i);
    public Enemy NextEnemyNow => state.NextEnemy;
    public List<NextAction> E_ActsNow => state.Field.PredictAct(state.Field.PlanAction());
    public int UpLvNow(WeaponData data) => state.Forge.GetUpgradeLevel(data);
    public int MeltLeftNow => state.MeltLeft;
    public int FurnaceCapNow => state.PlayData.FurnaceCap;
    public int MeltValNow(Weapon weapon) => state.PlayData.GetMeltValue(weapon);


    #endregion

    private void EndTurn()
    {
        if (!state.ReceiveNextWeapon())
        {
            
        }
        List<EnemyAttack> attacks = state.Field.ActAll(state.Field.PlanAction());
        state.Spawner.CheckBlocked(attacks);
        List<HitResult> validHits = state.Player.PredictHit(attacks);
        state.Player.TakeHit(validHits);
        if (state.Player.IsDead)
        {
            turn_survived = state.Turn;
            OnStateChanged?.Invoke();
            isGameOver = true;
            OnGameOver?.Invoke(turn_survived); 
            return;
        }
        state.NextTurn();
        state.TrySpawnEnemy();
        TryOpenChoice();
        OnStateChanged?.Invoke();
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

    private void OnGUI()
    {
        if (state == null) return;

        GUILayout.BeginArea(new Rect(10, 40, 500, 600));

        bool canAct = selected == null && !IsInputBlocked;


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

        GUI.enabled = canAct;
        WeaponData forUpgrade = null;

        for (int i = 0; i < state.Inventory.Length; i++)
        {
            var w = state.Inventory[i];
            if (w == null)
            {
                GUILayout.Label($"[{i}] 빈칸");
                continue;
            }

            GUILayout.BeginHorizontal();

            int nextLevel = state.Forge.GetUpgradeLevel(w.Data) + 1;
            GUI.enabled = canAct && state.Forge.CheckUpgradeable(w.Data, nextLevel);
            if (GUILayout.Button($" {w.Data.WeaponName} 강화 " +
                $"{state.Forge.GetUpgradePrice(w.Data, nextLevel)}",
                GUILayout.Width(100), GUILayout.Height(40)))
                forUpgrade = w.Data;

            GUI.enabled = canAct;

            GUILayout.EndHorizontal();
        }

        GUILayout.BeginHorizontal();

        GUILayout.EndHorizontal();

        GUILayout.Label($"용광로 {state.MeltLeft} / {state.PlayData.FurnaceCap}");

        GUI.enabled = selected == null;
        bool debug_skipturn = GUILayout.Button($"{Debug_SkipTurnTo}턴으로 이동");
        bool debug_addIron = GUILayout.Button($"철괴 {Debug_FreeIron}개");
        GUI.enabled = true;
        GUILayout.EndArea();
        int w_choice = ChoiceButton(out bool isSkipped);

        if (debug_skipturn)
        {
            state.Debug_SkipTurn(Debug_SkipTurnTo);
            OnStateChanged?.Invoke();
        }
        else if (forUpgrade != null) TryUpgrade(forUpgrade);
        else if (forHeal) TryHeal(GUI_HealTo);
        else if (debug_addIron)
        {
            state.AddIron(Debug_FreeIron);
            OnStateChanged?.Invoke();
        }
        else if (w_choice != -1) ChooseWeightInc(w_choice);
        else if (isSkipped) SkipWeightInc();
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
