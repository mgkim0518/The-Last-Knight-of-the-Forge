using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    [SerializeField] private EconomyData economyData;
    [SerializeField] private RandomManager randomManager;
    [SerializeField] private WeaponData[] weaponPool;

    private GameState state;


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
        Debug.Log("다음 턴 준비");
        state.NextTurn();
    }

    private Weapon CreateRandomWeapon()
    {
        var data = weaponPool[state.Rng.Next(weaponPool.Length)];
        return new Weapon(data);
    }

    private void OnGUI()
    {
        if (state == null) return;

        GUILayout.BeginArea(new Rect(10, 40, 300, 400));
        GUILayout.Label($"턴: {state.Turn}");

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
}
