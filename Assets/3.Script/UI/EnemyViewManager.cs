using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyViewManager : MonoBehaviour
{
    [SerializeField] private TurnManager manager;
    [SerializeField] private FieldSlots slots;
    [SerializeField] private EnemyViewer enemy_prefab;

    private Dictionary<Enemy, EnemyViewer> views = new Dictionary<Enemy, EnemyViewer>();
    private HashSet<Enemy> seen = new HashSet<Enemy>();
    private readonly List<Enemy> toRemove = new List<Enemy>();
    private readonly NextAction?[] actsByCell = new NextAction?[Field.Size];

    private void Awake()
    {
        manager.OnStateChanged += WriteEnemies;
    }

    private void OnDestroy()
    {
        manager.OnStateChanged -= WriteEnemies;
    }

    public void WriteEnemies()
    {
        seen.Clear();
        SortActsByCell(manager.E_ActsNow);

        for (int i = 0; i < Field.Size; i++)
        {
            Enemy e = manager.EnemyNow(i);
            if (e == null)
            {
                continue;
            }

            if (slots.TryGetSlotPos(i, out Vector3 pos))
            {
                ShowEnemy(e, pos, i, actsByCell[i]);
            }
        }

        Enemy next = manager.NextEnemyNow;
        if (next != null)
        {
            ShowEnemy(next, slots.NextEnemyPos(), -1, null);
        }

        toRemove.Clear();
        foreach (Enemy e in views.Keys)
        {
            if (!seen.Contains(e))
            {
                toRemove.Add(e);
            }
        }

        foreach (Enemy e in toRemove)
        {
            Destroy(views[e].gameObject);
            views.Remove(e);
        }

    }

    private void SortActsByCell(List<NextAction> acts)
    {
        for (int i = 0; i < actsByCell.Length; i++)
        {
            actsByCell[i] = null;
        }

        foreach (NextAction act in acts)
        {
            if (act.Pos < 0 || act.Pos >= actsByCell.Length)
            {
                Debug.LogWarning($"칸을 벗어난 행동 예측입니다: {act.Pos}");
                continue;
            }

            actsByCell[act.Pos] = act;
        }
    }

    private void ShowEnemy(Enemy e, Vector3 pos, int slotIndex, NextAction? action)
    {
        if (!views.TryGetValue(e, out EnemyViewer viewer))
        {
            viewer = Instantiate(enemy_prefab, transform);
            viewer.SetupEnemy(e, manager);
            views[e] = viewer;
        }

        viewer.UpdateEnemy(pos, slotIndex);
        viewer.UpdateAction(action);
        seen.Add(e);
    }
}