using UnityEngine;

[CreateAssetMenu(fileName = "EnemyAct", menuName = "Scriptable Objects/EnemyAct/Charge")]
public class ChargeEnemyAct: BaseEnemyAct   
{
    public override EnemyAction NextMove(Enemy[] cells, int pos)
    {
        Enemy self = cells[pos];

        if (pos != 0)
        {
            if (self.IsInRange(pos))
            {
                if (self.IsLoaded)
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
                        E_Action = EnemyBehav.Charging,
                        Dest = pos
                    };
                }
            }
            else
            {
                return new EnemyAction
                {
                    Cell = pos,
                    E_Action = EnemyBehav.Move,
                    Dest = pos - 1
                };
            }
        }
        else
        {
            if (self.IsLoaded)
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
                    E_Action = EnemyBehav.Charging,
                    Dest = pos
                };
            }
        }      
    }
}
