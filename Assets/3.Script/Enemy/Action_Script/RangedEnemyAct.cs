using UnityEngine;

[CreateAssetMenu(fileName = "EnemyAct", menuName = "Scriptable Objects/EnemyAct/Ranged")]
public class RangedEnemyAct: BaseEnemyAct 
{
    public override EnemyAction NextMove(Enemy[] cells, int pos)
    {
        Enemy self = cells[pos];

        if (self.IsInRange(pos))
        {
            return new EnemyAction
            {
                Cell = pos,
                E_Action = EnemyBehav.Attack,
                Dest = pos
            };
        }
        else if (pos != 0 && cells[pos - 1] == null)
        {
            return new EnemyAction
            {
                Cell = pos,
                E_Action = EnemyBehav.Move,
                Dest = pos - 1
            };
        }
        else
        {
            return new EnemyAction
            {
                Cell = pos,
                E_Action = EnemyBehav.Wait,
                Dest = pos
            };
        }
    }

}
