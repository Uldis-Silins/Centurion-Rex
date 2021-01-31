using System.Collections.Generic;
using UnityEngine;
using System;

using Random = UnityEngine.Random;

public class AI_Strategy : MonoBehaviour
{
    public enum StrategyType { None, AttackAllIn, Attack, Roam }

    public Player_Controller playerController;
    public List<Building_Health> discoveredPlayerBuildings;

    public StrategyType[] strats;

    private List<Unit_Base> m_reserveUnits;
    private List<Unit_Base> m_activeUnits;

    public float switchPhaseTime = 120f;

    public StrategyType currentStrategy;

    private float m_switchPhaseTimer;
    private Action m_currentPhase;
    private int m_phaseCounter;

    private float m_tickTimer = 1.23f;

    private void Awake()
    {
        m_reserveUnits = new List<Unit_Base>();
        m_activeUnits = new List<Unit_Base>();
    }

    private void Start()
    {
        m_switchPhaseTimer = Time.time;
        m_phaseCounter = 0;
    }

    private void Update()
    {
        if(m_switchPhaseTimer <= 0)
        {
            m_switchPhaseTimer = switchPhaseTime;

            currentStrategy = strats[m_phaseCounter % strats.Length];
            m_currentPhase = GetCurrentPhase(currentStrategy);
            SetReserveForPhase((int)currentStrategy);
            m_phaseCounter++;

            Debug.Log("Switched phase to " + currentStrategy.ToString() + " t: " + Time.time);
        }

        if (m_tickTimer <= 0f)
        {
            SetReserveForPhase((int)currentStrategy);
            m_currentPhase();
            m_tickTimer = Random.Range(1f, 3f);
        }

        m_switchPhaseTimer -= Time.deltaTime;
        m_tickTimer -= Time.deltaTime;
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

    public void Roam()
    {
        for (int i = 0; i < m_activeUnits.Count; i++)
        {
            if(!m_activeUnits[i].seeker.IsMoving)
            {
                m_activeUnits[i].seeker.SetDestination(playerController.spawnedBuildings[Random.Range(0, playerController.spawnedBuildings.Count)].selecteble.transform.position + new Vector3(Random.insideUnitCircle.x * 25f, 0f, Random.insideUnitCircle.y * 25f));
            }
        }
    }

    public void Attack()
    {
        for (int i = 0; i < m_activeUnits.Count; i++)
        {
            if (!m_activeUnits[i].seeker.IsMoving)
            {
                m_activeUnits[i].seeker.SetDestination(discoveredPlayerBuildings[Random.Range(0, discoveredPlayerBuildings.Count)].transform.position + new Vector3(Random.insideUnitCircle.x * 25f, 0f, Random.insideUnitCircle.y * 25f));
            }
        }
    }

    public void AttackAllIn()
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

    private Action GetCurrentPhase(StrategyType type)
    {
        switch (type)
        {
            case StrategyType.None:
                break;
            case StrategyType.Roam:
                return Roam;
            case StrategyType.Attack:
                return Attack;
            case StrategyType.AttackAllIn:
                return AttackAllIn;
            default:
                break;
        }

        return () => { };
    }

    private void SetReserveForPhase(int defensiveness)
    {
        m_activeUnits.Clear();
        m_reserveUnits.Clear();

        for (int i = 0; i < playerController.OwnedUnits.Count; i++)
        {
            if(defensiveness <= 0 || i % defensiveness == 0)
            {
                m_activeUnits.Add(playerController.OwnedUnits[i]);
            }
            else
            {
                m_reserveUnits.Add(playerController.OwnedUnits[i]);
            }
        }
    }
}