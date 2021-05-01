using System.Collections.Generic;
using UnityEngine;

public class AI_Awareness : MonoBehaviour
{
    public Player_Controller ownerController;
    public Player_Controller enemyController;

    public float tickRate = 0.3f;

    private Dictionary<Unit_Base, HashSet<Unit_Base>> m_unitsAssigned;  // key: enemy; value: assigned owned units

    private float m_timer;

    private void Update()
    {
        if(m_timer <= 0f)
        {
            for (int i = 0; i < ownerController.OwnedUnits.Count; i++)
            {
                Unit_Base unit = ownerController.OwnedUnits[i];

                if (!unit.HasMoveTarget && !unit.HasAttackTarget)
                {
                    List<GridHashList2D.Node> closeEnemies = enemyController.UnitPositions.Find(unit.transform.position, Vector2.one * unit.AttackDistance);

                    if (closeEnemies.Count > 0)
                    {
                        Unit_Base selectedEnemy = GetClosest(closeEnemies);
                        unit.SetAttackTarget(selectedEnemy.health);
                        unit.SetState(Unit_Base.UnitStateType.Attack);
                    }
                }
            }

            m_timer = tickRate;
        }

        m_timer -= Time.deltaTime;
    }

    private Unit_Base GetClosest(List<GridHashList2D.Node> enemies)
    {
        if (enemies.Count == 0) return null;

        Unit_Base closest = enemyController.UnitsByPosition[enemies[0]];
        if (enemies.Count == 1) return closest;

        Vector2 pos = transform.position;
        float closestDist = (enemies[0].position - pos).sqrMagnitude;

        for (int i = 0; i < enemies.Count; i++)
        {
            float dist = (enemies[i].position - pos).sqrMagnitude;

            if (dist < closestDist)
            {
                closest = enemyController.UnitsByPosition[enemies[i]];
                closestDist = dist;

            }
        }

        return closest;
    }
}