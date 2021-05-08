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
    public bool sortResourceBuildingsByDistance;

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

    private void Awake()
    {
        m_idleUnits = new List<Unit_Base>();
        m_unitsByCommand = new Dictionary<int, HashSet<Unit_Base>>();
        m_ownedResourceBuildings = new List<Building_Resource>();
    }

    private void Start()
    {
        List<Building_Resource> resBuildings = new List<Building_Resource>(resourceBuildings);

        if (sortResourceBuildingsByDistance)
        {
            resBuildings.Sort(new ResourceBuildingDistanceComparer(ownerController.ownedBuildings[0].gameObject.transform.position));
        }

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

    private void OnDrawGizmos()
    {
        if (m_unitsByCommand != null)
        {
            foreach (var item in m_unitsByCommand)
            {
                foreach (var unit in m_unitsByCommand[item.Key])
                {
#if UNITY_EDITOR
                    UnityEditor.Handles.Label(transform.position + Vector3.up * 2.5f, ((UnitCommandType)item.Key).ToString());
#endif
                }
            }
        }
    }

    private void Update()
    {
        if (Player_Controller.currentGameState != GameState.Playing) return;

        if(m_reassignTimer <= 0f && m_idleUnits.Count > 0)
        {
            m_reassignTimer = reassignTickRate;

            UpdateResourceBuildings();

            if (m_baseInDanger)
            {
                AssignIdleUnits(UnitCommandType.MoveHome, 1f);
            }
            else if (m_idleUnits.Count >= enemyController.OwnedUnits.Count * 2 && (HasHalfOfResourceBuildings || Random.value < m_earlyHarrasmentChance))
            {
                AssignIdleUnits(UnitCommandType.MoveToEnemyBase, 0.5f);
            }
            else if (ownerController.currentResources < 150 && m_resourceBuildingQueue.Count > 0 && m_ownedResourceBuildings.Count < 3 && m_curResourceBuilding == null)
            {
                m_curResourceBuilding = m_resourceBuildingQueue.Dequeue();
                AssignIdleUnits(UnitCommandType.GetResourceBuilding, 1f);
            }
            else if(m_idleUnits.Count >= 10 && m_resourceBuildingQueue.Count > 0 && (m_curResourceBuilding == null || m_curResourceBuilding.ownerFaction != FactionType.Enemy))
            {
                m_curResourceBuilding = m_resourceBuildingQueue.Dequeue();
                AssignIdleUnits(UnitCommandType.GetResourceBuilding, Mathf.Lerp(0.8f, 0.2f, (float)m_ownedResourceBuildings.Count / resourceBuildings.Count));
            }

            // Find player units near AI base and set base attacked flag if enemy unit count is larger than base defense unit count
            var positions = enemyController.UnitPositions.Find(ownerController.ownedBuildings[0].gameObject.transform.position, Vector2.one * 30.0f);
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
                        if (unit.HasMoveTarget || unit.HasAttackTarget) break;

                        if (!unit.HasAttackTarget)
                        {
                            unit.SetMoveTarget((Vector2)enemyController.ownedBuildings[0].gameObject.transform.position + Random.insideUnitCircle * unit.visionDistance);
                            unit.SetState(Unit_Base.UnitStateType.Move);
                        }
                        break;
                    case UnitCommandType.GetResourceBuilding:
                        if (unit.HasAttackTarget)
                        {
                            removedUnits.Push(unit);
                        }

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
                        if (Vector2.Distance(unit.transform.position, ownerController.ownedBuildings[0].gameObject.transform.position) > 30f)
                        {
                            if (!unit.HasMoveTarget)
                            {
                                unit.SetMoveTarget((Vector2)ownerController.ownedBuildings[0].gameObject.transform.position + Random.insideUnitCircle * 30.0f);
                                unit.SetState(Unit_Base.UnitStateType.Move);
                            }
                        }
                        else
                        {
                            if(!unit.HasAttackTarget)
                            {
                                List<GridHashList2D.Node> closeEnemies = enemyController.UnitPositions.Find(unit.transform.position, Vector2.one * 30f);

                                if (closeEnemies.Count > 0)
                                {
                                    Unit_Base selectedEnemy = GetClosest(unit, closeEnemies);
                                    unit.SetAttackTarget(selectedEnemy.health);
                                    unit.SetState(Unit_Base.UnitStateType.Attack);
                                }
                            }
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

    private void HandleOwnedUnitSpawned(Unit_Base unit)
    {
        Debug.Assert(unit.gameObject != null, "Passed null unit");
        m_idleUnits.Add(unit);
    }

    private void HandleOwnedUnitKilled(Unit_Base unit)
    {
        Debug.Assert(unit.gameObject != null, "Passed null unit");

        //if (m_idleUnits.Contains(unit))
        //{
        //    m_idleUnits.Remove(unit);
        //}

        for (int i = m_idleUnits.Count - 1; i >= 0; i--)
        {
            if (m_idleUnits[i] == unit || m_idleUnits[i] == null)
            {
                m_idleUnits.RemoveAt(i);
            }
        }

        foreach (var command in m_unitsByCommand)
        {
            if(command.Value.Contains(unit))
            {
                command.Value.Remove(unit);
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
                if (resourceBuildings[i].ownerFaction != FactionType.Enemy && !m_resourceBuildingQueue.Contains(resourceBuildings[i]))
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

        private Unit_Base GetClosest(Unit_Base unit, List<GridHashList2D.Node> enemies)
        {
            if (enemies.Count == 0) return null;

            Unit_Base closest = enemyController.UnitsByPosition[enemies[0]];
            if (enemies.Count == 1) return closest;

            Vector2 pos = unit.transform.position;
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