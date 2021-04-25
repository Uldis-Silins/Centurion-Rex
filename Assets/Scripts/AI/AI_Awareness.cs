using System.Collections.Generic;
using UnityEngine;

public class AI_Awareness : MonoBehaviour
{
    public Player_Controller playerController;
    public DamageableManager damageableManager;
    public SelectableManager selectableManager;

    public float minAgroDistance = 3f, maxAgroDistance = 6f;

    public float tickRate = 1.5f;
    public float tickDelay = 0.5f;

    private float m_timer;

    private void Start()
    {
        m_timer = tickRate + tickDelay;
    }

    private void Update()
    {
        if(m_timer <= 0f)
        {
            m_timer = tickRate;

            var units = playerController.OwnedUnits;

            foreach (var unit in units)
            {
                bool isSelectedByPlayer = false;

                if (playerController.enemyFaction == FactionType.Enemy && selectableManager.GetSelectable(unit.gameObject).IsSelected)
                {
                    isSelectedByPlayer = true;   
                }

                if(unit != null)
                {
                    var closestEnemies = new List<KeyValuePair<GameObject, IDamageable> >(damageableManager.GetAtPosition(unit.transform.position, unit.visionDistance, playerController.enemyFaction));

                    if(closestEnemies.Count > 0)
                    {
                        if (isSelectedByPlayer && !unit.HasMoveTarget)
                        {
                            Vector3 closestDist = closestEnemies[0].Key.transform.position;
                            closestDist.y = unit.transform.position.y;

                            if (Vector3.Distance(closestDist, unit.transform.position) <= unit.AttackDistance)
                            {
                                unit.SetAttackTarget(closestEnemies[0].Value, closestEnemies[0].Key);
                            }
                        }
                        else
                        {
                            unit.SetAttackTarget(closestEnemies[0].Value, closestEnemies[0].Key);
                        }
                    }
                }

                GameObject attacker = unit.health.Attacker;

                if(attacker != null && !unit.HasAttackTarget)
                {
                    IDamageable damageable = attacker.GetComponent<IDamageable>();

                    if(damageable != null)
                    {
                        unit.SetAttackTarget(damageable, attacker);
                    }
                }
            }
        }

        m_timer -= Time.deltaTime;
    }
}