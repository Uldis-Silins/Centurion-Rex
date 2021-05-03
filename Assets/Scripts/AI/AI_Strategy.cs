using System.Collections.Generic;
using UnityEngine;
using System;

using Random = UnityEngine.Random;

public class AI_Strategy : MonoBehaviour
{
    public enum UnitCommandType { None, MoveToEnemyBase, GetResourceBuilding, MoveHome }

    public Player_Controller ownerController;
    public Player_Controller enemyController;

    public List<Building_Resource> resourceBuildings;

    private Dictionary<int, HashSet<Unit_Base>> m_unitsByCommand;
    private List<Unit_Base> m_idleUnits;

    public float reassignTickRate = 0.3f;

    private float m_reassignTimer;
    private int m_currentCommandUpdate;
    private int m_totalCommands = 4;

    private Queue<Building_Resource> m_resourceBuildingQueue;
    private List<Building_Resource> m_ownedResourceBuildings;
    private Building_Resource m_curResourceBuilding;

    private bool m_baseInDanger;
    private float m_earlyHarrasmentChance = 0.1f;

    private bool HasHalfOfResourceBuildings { get { return m_ownedResourceBuildings.Count > resourceBuildings.Count / 2; } }
    private bool HasHarrasmentAttackCommenced { get { return m_unitsByCommand.ContainsKey((int)UnitCommandType.MoveToEnemyBase) && m_unitsByCommand[(int)UnitCommandType.MoveToEnemyBase].Count > 0; } }

    private void Awake()
    {
        m_idleUnits = new List<Unit_Base>();
        m_unitsByCommand = new Dictionary<int, HashSet<Unit_Base>>();
        m_ownedResourceBuildings = new List<Building_Resource>();
    }

    private void Start()
    {
        List<Building_Resource> resBuildings = new List<Building_Resource>(resourceBuildings);
        resBuildings.Sort(new ResourceBuildingDistanceComparer(ownerController.ownedBuildings[0].gameObject.transform.position));
        m_resourceBuildingQueue = new Queue<Building_Resource>(resBuildings);
    }

    private void OnEnable()
    {
        ownerController.onOwnedUnitAdded += HandleOwnedUnitSpawned;
        ownerController.onOwnedUnitRemoved += HandleOwnedUnitKilled;
    }

    private void OnDisable()
    {
        ownerController.onOwnedUnitAdded -= HandleOwnedUnitSpawned;
        ownerController.onOwnedUnitRemoved -= HandleOwnedUnitKilled;
    }

    private void Update()
    {
        if (Player_Controller.currentGameState != GameState.Playing) return;

        if(m_reassignTimer <= 0f)
        {
            m_reassignTimer = reassignTickRate;

            UpdateResourceBuildings();

            if(m_idleUnits.Count >= 10 && (HasHalfOfResourceBuildings || Random.value < m_earlyHarrasmentChance) && !HasHarrasmentAttackCommenced)
            {
                AssignIdleUnits(UnitCommandType.MoveToEnemyBase, 0.5f);
            }
            else if(ownerController.currentResources < 150 && m_resourceBuildingQueue.Count > 0 && m_curResourceBuilding == null)
            {
                m_curResourceBuilding = m_resourceBuildingQueue.Dequeue();
                AssignIdleUnits(UnitCommandType.GetResourceBuilding, 1f);
            }
            else if(m_idleUnits.Count >= 10 && m_resourceBuildingQueue.Count > 0 && m_curResourceBuilding == null)
            {
                m_curResourceBuilding = m_resourceBuildingQueue.Dequeue();
                AssignIdleUnits(UnitCommandType.GetResourceBuilding, Mathf.Lerp(0.8f, 0.1f, (float)m_ownedResourceBuildings.Count / resourceBuildings.Count));
            }
            else if(m_baseInDanger)
            {
                AssignIdleUnits(UnitCommandType.MoveHome, 1f);
            }

            // Find player units near AI base and set base attacked flag if enemy unit count is larger than base defense unit count
            var positions = enemyController.UnitPositions.Find(ownerController.ownedBuildings[0].gameObject.transform.position, Vector2.one * 50.0f);
            m_baseInDanger = positions.Count > 0;

            if(m_baseInDanger && m_idleUnits.Count > 0)
            {
                int posIndex = 0;

                foreach (var unit in m_idleUnits)
                {
                    if(!unit.HasAttackTarget)
                    {
                        unit.SetAttackTarget(enemyController.UnitsByPosition[positions[posIndex % positions.Count]].health);
                        unit.SetState(Unit_Base.UnitStateType.Attack);
                        posIndex++;
                    }
                }
            }
        }

        m_reassignTimer -= Time.deltaTime;

        if (m_totalCommands > 0)
        {
            int curCommand = m_currentCommandUpdate % m_totalCommands;

            if (m_unitsByCommand.ContainsKey(curCommand))
            {
                ExecuteCommands(curCommand);
            }

            m_currentCommandUpdate++;
        }
    }

    private void AssignIdleUnits(UnitCommandType commandType, float percent)
    {
        int commandIndex = (int)commandType;

        if(!m_unitsByCommand.ContainsKey(commandIndex))
        {
            m_unitsByCommand.Add(commandIndex, new HashSet<Unit_Base>());
        }

        int count = Mathf.FloorToInt(m_idleUnits.Count * percent);

        while (count > 0)
        {
            m_unitsByCommand[commandIndex].Add(m_idleUnits[count - 1]);
            m_idleUnits.RemoveAt(count - 1);
            count--;
        }
    }

    private void ExecuteCommands(int curCommand)
    {
        Stack<Unit_Base> removedUnits = new Stack<Unit_Base>();

        if (m_baseInDanger && m_unitsByCommand[curCommand].Count > 0 && (UnitCommandType)curCommand != UnitCommandType.MoveHome)
        {
            foreach (var unit in m_unitsByCommand[curCommand])
            {
                removedUnits.Push(unit);
            }
        }
        else
        {
            foreach (var unit in m_unitsByCommand[curCommand])
            {
                switch ((UnitCommandType)curCommand)
                {
                    case UnitCommandType.None:
                        break;
                    case UnitCommandType.MoveToEnemyBase:
                        if (unit.HasMoveTarget) break;

                        if (!unit.HasAttackTarget)
                        {
                            unit.SetMoveTarget((Vector2)enemyController.ownedBuildings[0].gameObject.transform.position + Random.insideUnitCircle * unit.visionDistance);
                            unit.SetState(Unit_Base.UnitStateType.Move);
                        }
                        break;
                    case UnitCommandType.GetResourceBuilding:
                        if (unit.HasMoveTarget) break;

                        if(m_curResourceBuilding == null)
                        {
                            removedUnits.Push(unit);
                        }
                        else
                        {
                            unit.SetMoveTarget((Vector2)m_curResourceBuilding.transform.position + Random.insideUnitCircle * m_curResourceBuilding.captureRadius);
                            unit.SetState(Unit_Base.UnitStateType.Move);
                        }
                        break;
                    case UnitCommandType.MoveHome:
                        if (!unit.HasMoveTarget)
                        {
                            unit.SetMoveTarget(ownerController.ownedBuildings[0].gameObject.transform.position * Random.insideUnitCircle * 20.0f);
                            unit.SetState(Unit_Base.UnitStateType.Move);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        while (removedUnits.Count > 0)
        {
            Unit_Base unit = removedUnits.Pop();
            m_unitsByCommand[curCommand].Remove(unit);
            m_idleUnits.Add(unit);
        }
    }

    private void HandleOwnedUnitSpawned(int ownedUnitIndex)
    {
        m_idleUnits.Add(ownerController.OwnedUnits[ownedUnitIndex]);
    }

    private void HandleOwnedUnitKilled(int ownedUnitIndex)
    {
        Unit_Base removedUnit = ownerController.OwnedUnits[ownedUnitIndex];

        if (m_idleUnits.Contains(removedUnit))
        {
            m_idleUnits.Remove(removedUnit);
        }

        foreach (var command in m_unitsByCommand)
        {
            if(command.Value.Contains(removedUnit))
            {
                command.Value.Remove(removedUnit);
            }
        }
    }

    private void UpdateResourceBuildings()
    {
        for (int i = 0; i < resourceBuildings.Count; i++)
        {
            if (!m_ownedResourceBuildings.Contains(resourceBuildings[i]))
            {
                if (resourceBuildings[i].ownerFaction == FactionType.Enemy)
                {
                    m_ownedResourceBuildings.Add(resourceBuildings[i]);
                }
            }
            else
            {
                if (resourceBuildings[i].ownerFaction != FactionType.Enemy)
                {
                    m_resourceBuildingQueue.Enqueue(resourceBuildings[i]);
                }
            }
        }

        if(m_ownedResourceBuildings.Contains(m_curResourceBuilding))
        {
            m_curResourceBuilding = null;
        }
    }

    private class ResourceBuildingDistanceComparer : IComparer<Building_Resource>
    {
        private Vector3 m_pos;

        public ResourceBuildingDistanceComparer(Vector3 pos)
        {
            m_pos = pos;
        }

        public int Compare(Building_Resource x, Building_Resource y)
        {
            return Vector2.SqrMagnitude(x.transform.position - m_pos).CompareTo(Vector2.SqrMagnitude(y.transform.position - m_pos));
        }
    }
}