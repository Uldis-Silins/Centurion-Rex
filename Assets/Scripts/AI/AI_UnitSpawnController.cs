using System.Collections.Generic;
using UnityEngine;

public class AI_UnitSpawnController : MonoBehaviour
{
    public Player_Controller playerController;
    public List<Building_UnitSpawner> unitSpawners;
    public DamageableManager damageableManager;

    public float spawnDelay = 5f;
    public int maxPopulation = 30;

    private float m_spawnTimer;

    private void Update()
    {
        if (playerController.CurrentPopulation < maxPopulation)
        {
            if (m_spawnTimer < 0f)
            {
                m_spawnTimer = spawnDelay;

                var spawner = unitSpawners[Random.Range(0, unitSpawners.Count)];

                if (spawner != null)
                {
                    spawner.OnSpawnUnitClick(Random.Range(0, spawner.units.Length));
                }
            }

            m_spawnTimer -= Time.deltaTime;
        }
    }
}