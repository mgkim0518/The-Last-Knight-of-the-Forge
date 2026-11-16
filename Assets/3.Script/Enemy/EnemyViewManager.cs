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

        for (int i = 0; i < Field.Size; i++)
        {
            Enemy e = manager.EnemyNow(i);
            if (e == null)
            {
                continue;
            }

            if (slots.TryGetSlotPos(i, out Vector3 pos))
            {
                ShowEnemy(e, pos, i);
            }
        }

        Enemy next = manager.NextEnemyNow;
        if (next != null)
        {
            ShowEnemy(next, slots.NextEnemyPos(), -1);
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

    private void ShowEnemy(Enemy e, Vector3 pos, int slotIndex)
    {
        if (!views.TryGetValue(e, out EnemyViewer viewer))
        {
            viewer = Instantiate(enemy_prefab, transform);
            viewer.SetupEnemy(e, manager);
            views[e] = viewer;
        }

        viewer.UpdateEnemy(pos, slotIndex);
        seen.Add(e);
    }
}
