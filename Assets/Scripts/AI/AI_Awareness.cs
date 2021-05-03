using System.Collections.Generic;
using UnityEngine;

public class AI_Awareness : MonoBehaviour
{
    public Player_Controller ownerController;
    public Player_Controller enemyController;

    public bool canCancelMove;

    //private Dictionary<Unit_Base, List<Unit_Base>> m_unitsAssigned;  // key: enemy; value: assigned owned units

    private void Update()
    {
        if (Player_Controller.currentGameState != GameState.Playing) return;

        for (int i = 0; i < ownerController.OwnedUnits.Count; i++)
        {
            Unit_Base unit = ownerController.OwnedUnits[i];

            if (!(unit as ISelecteble).IsSelected && (canCancelMove || !unit.HasMoveTarget) && !unit.HasAttackTarget)
            {
                for (int j = 0; j < enemyController.ownedBuildings.Count; j++)
                {
                    if (Vector2.Distance(enemyController.ownedBuildings[j].gameObject.transform.position, unit.transform.position) <= unit.visionDistance)
                    {
                        Building_Base building = enemyController.ownedBuildings[j].selectable as Building_Base;

                        if (building.health != null)
                        {
                            unit.SetAttackTarget(building.health);
                            unit.SetState(Unit_Base.UnitStateType.Attack);
                            continue;
                        }
                    }
                }

                List<GridHashList2D.Node> closeEnemies = enemyController.UnitPositions.Find(unit.transform.position, Vector2.one * unit.visionDistance);

                if (closeEnemies.Count > 0)
                {
                    Unit_Base selectedEnemy = GetClosest(closeEnemies);

                    //if ((unit.unitType == UnitData.UnitType.Soldier && selectedEnemy.unitType == UnitData.UnitType.Ranged && Vector2.Distance(selectedEnemy.transform.position, unit.transform.position) > unit.visionDistance / 2f) ||
                    //    (unit.unitType == UnitData.UnitType.Ranged && selectedEnemy.unitType == UnitData.UnitType.Soldier && Vector2.Distance(selectedEnemy.transform.position, unit.transform.position) < unit.visionDistance / 2f))
                    //{
                    //    unit.SetMoveTarget(unit.transform.position + (selectedEnemy.transform.position - unit.transform.position).normalized * unit.visionDistance);
                    //    unit.SetState(Unit_Base.UnitStateType.Move);
                    //}
                    //else
                    {
                        unit.SetAttackTarget(selectedEnemy.health);
                        unit.SetState(Unit_Base.UnitStateType.Attack);
                    }
                }
            }
        }
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