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
    private Action m_currentPhase = ()=> { };
    private int m_phaseCounter;

    private float m_tickTimer = 1.23f;

    private Queue<List<Unit_Base>> m_waves;

    private void Awake()
    {
        m_reserveUnits = new List<Unit_Base>();
        m_activeUnits = new List<Unit_Base>();
        m_waves = new Queue<List<Unit_Base>>();
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
            if (!m_activeUnits[i].seeker.IsMoving)
            {
                m_activeUnits[i].seeker.SetDestination(playerController.spawnedBuildings[Random.Range(0, playerController.spawnedBuildings.Count)].selecteble.transform.position + new Vector3(Random.insideUnitCircle.x * 25f, 0f, Random.insideUnitCircle.y * 25f));
            }
        }

        var reserveSoldiers = GetByType(m_reserveUnits, UnitData.UnitType.Soldier);

        if(reserveSoldiers.Count > 0)
        {
            var pos = GetFormationPositions(reserveSoldiers[0].transform.position, reserveSoldiers.Count);

            for (int i = 0; i < reserveSoldiers.Count; i++)
            {
                reserveSoldiers[i].seeker.SetDestination(pos[i]);
            }
        }

        var reserveRanged = GetByType(m_reserveUnits, UnitData.UnitType.Ranged);

        if (reserveRanged.Count > 0)
        {
            var pos = GetFormationPositions(reserveRanged[0].transform.position, reserveRanged.Count);

            for (int i = 0; i < reserveRanged.Count; i++)
            {
                reserveRanged[i].seeker.SetDestination(pos[i]);
            }
        }

        var reserveCavalry = GetByType(m_reserveUnits, UnitData.UnitType.Cavalry);

        if (reserveCavalry.Count > 0)
        {
            var pos = GetFormationPositions(reserveCavalry[0].transform.position, reserveCavalry.Count);

            for (int i = 0; i < reserveCavalry.Count; i++)
            {
                reserveCavalry[i].seeker.SetDestination(pos[i]);
            }
        }
    }

    public void Attack()
    {
        if (playerController.OwnedUnits.Count > 30)
        {
            if (m_waves.Count == 0)
            {
                Debug.Log("Setting up waves");
                m_waves.Enqueue(SetupWave(UnitData.UnitType.Soldier));
                m_waves.Enqueue(SetupWave(UnitData.UnitType.Ranged));
                m_waves.Enqueue(SetupWave(UnitData.UnitType.Cavalry));
            }
            else
            {
                bool canAttack = true;
                foreach (var unit in m_waves.Peek())
                {
                    if (unit == null || unit.seeker == null) continue;

                    if (unit.seeker.IsMoving)
                    {
                        canAttack = false;
                    }
                }

                if(canAttack)
                {
                    var wave = m_waves.Dequeue();

                    for (int i = 0; i < wave.Count; i++)
                    {
                        if (wave[i].seeker == null) continue;
                        wave[i].seeker.SetDestination(discoveredPlayerBuildings[0].transform.position);
                    }

                    wave.Clear();
                }
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

    private List<Unit_Base> SetupWave(UnitData.UnitType unitType)
    {
        List<Unit_Base> soldiers = GetByType(unitType);

        Vector3 pos;

        if (GetRallyPoint(out pos))
        {
            List<Vector3> positions = GetFormationPositions(pos, soldiers.Count);

            for (int i = 0; i < soldiers.Count; i++)
            {
                soldiers[i].seeker.SetDestination(positions[i]);
            }
        }

        return soldiers;
    }

    private bool GetRallyPoint(out Vector3 pos)
    {
        if(discoveredPlayerBuildings.Count > 0)
        {
            pos = (discoveredPlayerBuildings[0].transform.position - playerController.spawnedBuildings[0].selecteble.transform.position) / 2f;
            pos += new Vector3(Random.insideUnitCircle.x * 20f, 0f, Random.insideUnitCircle.y * 20f);

            return true;
        }

        pos = Vector3.zero;
        return false;
    }

    private List<Unit_Base> GetByType(UnitData.UnitType type)
    {
        List<Unit_Base> byType = new List<Unit_Base>();

        foreach (var unit in playerController.OwnedUnits)
        {
            if(unit.unitType == type)
            {
                byType.Add(unit);

                if (m_activeUnits.Contains(unit))
                {
                    m_activeUnits.Remove(unit);
                }
                else if (m_reserveUnits.Contains(unit))
                {
                    m_reserveUnits.Remove(unit);
                }
            }
        }

        return byType;
    }

    private List<Unit_Base> GetByType(List<Unit_Base> baseList, UnitData.UnitType type)
    {
        List<Unit_Base> byType = new List<Unit_Base>();

        foreach (var unit in baseList)
        {
            if (unit.unitType == type)
            {
                byType.Add(unit);
            }
        }

        return byType;
    }

    private List<Vector3> GetFormationPositions(Vector3 pos, int unitCount)
    {
        List<Vector3> positions = new List<Vector3>();
        int cols = (int)Mathf.Sqrt(unitCount);

        for (int y = 0; y <= cols; y++)
        {
            for (int x = 0; x <= unitCount / cols; x++)
            {
                positions.Add(pos + new Vector3(x, 0f, y));
            }
        }

        return positions;
    }
}