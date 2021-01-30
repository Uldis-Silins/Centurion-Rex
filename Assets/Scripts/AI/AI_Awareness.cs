using System.Collections.Generic;
using UnityEngine;

public class AI_Awareness : MonoBehaviour
{
    public Player_Controller playerController;
    public DamageableManager damageableManager;
    public SelectableManager selectableManager;

    public float minAttackDistance = 3f, maxAttackDistance = 6f;

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
                if (unit.HasAttackTarget) continue;
                if (playerController.enemyFaction == FactionType.Enemy && selectableManager.GetSelectable(unit.gameObject).IsSelected) continue;

                if(unit != null)
                {
                    var closestEnemies = new List<KeyValuePair<GameObject, IDamageable> >(damageableManager.GetAtPosition(unit.transform.position, Random.Range(minAttackDistance, maxAttackDistance), playerController.enemyFaction));

                    if(closestEnemies.Count > 0)
                    {
                        unit.SetAttackState(closestEnemies[0].Value, closestEnemies[0].Key);
                    }
                }
            }
        }

        m_timer -= Time.deltaTime;
    }
}