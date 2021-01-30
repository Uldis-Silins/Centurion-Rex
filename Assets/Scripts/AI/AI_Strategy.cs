using System.Collections.Generic;
using UnityEngine;

public class AI_Strategy : MonoBehaviour
{
    public enum StrategyType { None, AttackAllIn }

    public Player_Controller playerController;
    public List<Building_Health> discoveredPlayerBuildings;

    public float attackWithAllUnitsDelay = 30f;

    public StrategyType currentStrategy;

    private float m_lastAttackWithAllUnitsTime;

    private void Start()
    {
        m_lastAttackWithAllUnitsTime = Time.time;
    }

    private void Update()
    {
        if(currentStrategy != StrategyType.AttackAllIn && Time.time - m_lastAttackWithAllUnitsTime > attackWithAllUnitsDelay)
        {
            currentStrategy = StrategyType.AttackAllIn;

            Building_Health playerBase = TryGetPlayerBaseBuilding();

            if (playerBase != null)
            {
                for (int i = 0; i < playerController.OwnedUnits.Count; i++)
                {
                    playerController.OwnedUnits[i].SetAttackState(playerBase, playerBase.gameObject);
                }
            }
        }
    }

    private Building_Health TryGetPlayerBaseBuilding()
    {
        for (int i = 0; i < discoveredPlayerBuildings.Count; i++)
        {
            if(discoveredPlayerBuildings[i].Faction == FactionType.Player && discoveredPlayerBuildings[i].buildingType == BuildingType.Base)
            {
                return discoveredPlayerBuildings[i];
            }
        }

        return null;
    }
}