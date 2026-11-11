using UnityEngine;

[CreateAssetMenu(fileName = "EnemyAct", menuName = "Scriptable Objects/EnemyAct/Healer")]
public class HealerEnemyAct: BaseEnemyAct 
{
    public override EnemyAction NextMove(Enemy[] cells, int pos)
    {
        Enemy self = cells[pos];

        if (pos.Equals(0))
        {
            return new EnemyAction
            {
                Cell = pos,
                E_Action = EnemyBehav.Wait,
                Dest = pos
            };
        }
        else
        {
            Enemy front = cells[pos - 1];

            if (front == null)
            {
                return new EnemyAction
                {
                    Cell = pos,
                    E_Action = EnemyBehav.Move,
                    Dest = pos - 1
                };
            }
            else if (front.HP >= front.MaxHP)
            {
                return new EnemyAction
                {
                    Cell = pos,
                    E_Action = EnemyBehav.Wait,
                    Dest = pos
                };
            }
            else
            {
                return new EnemyAction
                {
                    Cell = pos,
                    E_Action = EnemyBehav.Heal,
                    Dest = pos - 1
                };
            }
        }
        
    }

}
