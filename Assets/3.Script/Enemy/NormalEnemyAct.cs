using UnityEngine;

[CreateAssetMenu(fileName = "EnemyAct", menuName = "Scriptable Objects/EnemyAct/Normal")]
public class NormalEnemyAct: ScriptableObject
    
{
    public virtual EnemyAction NextMove(Enemy[] cells, int pos)
    {
        Enemy self = cells[pos];

        if (pos <= 0 && cells[pos - 1] != null)
        {
            if (self.IsInRange(pos))
            {
                return new EnemyAction
                {
                    Cell = pos,
                    E_Action = EnemyBehav.Attack,
                    Dest = pos
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
        else
        {
            if (!self.IsAttackedLastTurn && self.IsInRange(pos))
            {
                return new EnemyAction
                {
                    Cell = pos,
                    E_Action = EnemyBehav.Attack,
                    Dest = pos
                };
            }
            else
            {
                return new EnemyAction
                {
                    Cell = pos,
                    E_Action = EnemyBehav.Move,
                    Dest = pos -1
                };
            }
        }
        
        
    }

}
