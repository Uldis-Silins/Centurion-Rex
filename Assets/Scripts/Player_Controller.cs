using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Player_Controller : MonoBehaviour
{
    public bool ownedByPlayer;
    [SerializeField] private SelectableManager selectableManager;
    [SerializeField] private DamageableManager damageableManager;

    private List<Unit_Base> m_ownedUnits;

    public int CurrentPopulation { get { return m_ownedUnits.Count; } }

    private void Awake()
    {
        m_ownedUnits = new List<Unit_Base>();

        if(ownedByPlayer)
        {
            NavMesh.avoidancePredictionTime = 0.5f;
        }
    }

    public void AddToOwnedUnits(Unit_Base unit)
    {
        m_ownedUnits.Add(unit);

        var health = unit.GetComponent<Unit_Health>();

        unit.agent.avoidancePriority = m_ownedUnits.Count * 5;

        if (health != null)
        {
            health.onKilled += HandleUnitKilled;
            damageableManager.RegisterDamageable(health, unit.gameObject);
        }

        if(ownedByPlayer && selectableManager != null)
        {
            selectableManager.RegisterSelectable(unit.GetComponent<ISelecteble>(), unit.gameObject);
        }
    }

    private void HandleUnitKilled(IDamageable damageable)
    {
        GameObject killedObj = damageableManager.GetObject(damageable);

        for (int i = m_ownedUnits.Count - 1; i >= 0; i--)
        {
            if(m_ownedUnits[i].gameObject == killedObj)
            {
                damageableManager.UnregisterDamageable(damageable);
                m_ownedUnits.RemoveAt(i);
                break;
            }
        }

        damageable.onKilled -= HandleUnitKilled;
        Destroy(killedObj);
    }
}

public enum FactionType { None, Player, Enemy }