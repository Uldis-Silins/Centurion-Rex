using System.Collections.Generic;
using UnityEngine;
using System;

using Random = UnityEngine.Random;

public class AI_Strategy : MonoBehaviour
{
    public Player_Controller ownerController;
    public Player_Controller enemyController;

    private Dictionary<int, HashSet<Unit_Base>> m_unitsByCommand;
    private HashSet<Unit_Base> m_idleUnits;

    public float tickRate = 0.3f;

    private float m_tickTimer;

    private void Awake()
    {
        m_idleUnits = new HashSet<Unit_Base>();
        m_unitsByCommand = new Dictionary<int, HashSet<Unit_Base>>();
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
        if(m_tickTimer <= 0f)
        {
            m_tickTimer = tickRate;

            if(m_idleUnits.Count >= 10)
            {
                AssignIdleUnits(1);
            }
        }

        m_tickTimer -= Time.deltaTime;
    }

    private void AssignIdleUnits(int commandIndex)
    {
        if(!m_unitsByCommand.ContainsKey(commandIndex))
        {
            m_unitsByCommand.Add(commandIndex, new HashSet<Unit_Base>());
        }

        m_unitsByCommand[commandIndex].UnionWith(m_idleUnits);
        m_idleUnits.Clear();

        foreach (var unit in m_unitsByCommand[commandIndex])
        {
            unit.SetMoveTarget((Vector2)enemyController.ownedBuildings[0].gameObject.transform.position + Random.insideUnitCircle * 10.0f);
            unit.SetState(Unit_Base.UnitStateType.Move);
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
}