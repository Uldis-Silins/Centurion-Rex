using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Player_Controller : MonoBehaviour
{
    public bool ownedByPlayer;
    public FactionType enemyFaction;

    [SerializeField] private SelectableManager selectableManager;
    [SerializeField] private DamageableManager damageableManager;

    private List<Unit_Base> m_ownedUnits;

    private Stack<IDamageable> m_waitingForKill;

    public List<Unit_Base> OwnedUnits { get { return m_ownedUnits; } }

    public int CurrentPopulation { get { return m_ownedUnits.Count; } }

    private void Awake()
    {
        m_ownedUnits = new List<Unit_Base>();

        if(ownedByPlayer)
        {
            NavMesh.avoidancePredictionTime = 0.5f;
        }

        m_waitingForKill = new Stack<IDamageable>();
    }

    private void Update()
    {
        while (m_waitingForKill.Count > 0)
        {
            IDamageable kill = m_waitingForKill.Pop();
            damageableManager.UnregisterDamageable(kill);
            GameObject killedObj = damageableManager.GetObject(kill);
            kill.onKilled -= HandleUnitKilled;
            Destroy(killedObj);
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
                if(selectableManager != null)
                {
                    selectableManager.UnregisterSelectable(killedObj);
                }

                m_ownedUnits.RemoveAt(i);
                break;
            }
        }
    }
}

public enum FactionType { None, Player, Enemy }