using System.Collections.Generic;
using UnityEngine;

public class VisibilityManager : MonoBehaviour
{
    public Player_Controller playerController;
    public Player_Controller enemyController;

    public bool checkUnitVisibility;

    private List<FOVElement> m_fovElements;

    public HashSet<Unit_Base> VisibleUnits { get; private set; }
    public HashSet<Player_Controller.Building> VisibleBuildings { get; private set; }

    private void Awake()
    {
        m_fovElements = new List<FOVElement>(GameObject.FindObjectsOfType<FOVElement>());
        VisibleBuildings = new HashSet<Player_Controller.Building>();
        VisibleUnits = new HashSet<Unit_Base>();
    }

    private void Start()
    {
        for (int i = 0; i < enemyController.ownedBuildings.Count; i++)
        {
            if (enemyController.ownedBuildings[i].gameObject != null)
            {
                (enemyController.ownedBuildings[i].selectable as Building_Base).spriteRenderer.enabled = false;
            }
        }
    }

    private void Update()
    {
        if (Player_Controller.currentGameState != GameState.Playing) return;
        if (enemyController == null || enemyController.OwnedUnits == null || !checkUnitVisibility) return;

        VisibleBuildings.Clear();
        VisibleUnits.Clear();

        for (int i = 0; i < enemyController.OwnedUnits.Count; i++)
        {
            enemyController.OwnedUnits[i].soldierRenderer.enabled = false;
        }

        for (int i = 0; i < playerController.OwnedUnits.Count; i++)
        {
            Unit_Base unit = playerController.OwnedUnits[i];
            List<GridHashList2D.Node> enemiesInRange = enemyController.UnitPositions.Find(unit.transform.position, Vector2.one * unit.stats.visionDistance);

            for (int j = 0; j < enemiesInRange.Count; j++)
            {
                Unit_Base enemy = enemyController.UnitsByPosition[enemiesInRange[j]];

                if (unit.CanSeeUnit(enemy))
                {
                    enemy.soldierRenderer.enabled = true;
                    VisibleUnits.Add(enemyController.UnitsByPosition[enemiesInRange[j]]);
                }
            }
            //for (int j = 0; j < enemyController.OwnedUnits.Count; j++)
            //{
            //    if (playerController.OwnedUnits[i].CanSeeUnit(enemyController.OwnedUnits[j]))
            //    {
            //        enemyController.OwnedUnits[j].soldierRenderer.enabled = true;
            //        VisibleUnits.Add(enemyController.OwnedUnits[j]);
            //    }
            //}

            for (int j = 0; j < enemyController.ownedBuildings.Count; j++)
            {
                if(Vector2.Distance(playerController.OwnedUnits[i].transform.position, enemyController.ownedBuildings[j].gameObject.transform.position) <= playerController.OwnedUnits[i].stats.visionDistance)
                {
                    (enemyController.ownedBuildings[j].selectable as Building_Base).spriteRenderer.enabled = true;
                    VisibleBuildings.Add(enemyController.ownedBuildings[j]);
                }
            }
        }

        m_fovElements.Clear();

        for (int i = 0; i < playerController.ownedBuildings.Count; i++)
        {
            m_fovElements.Add(playerController.ownedBuildings[i].gameObject.GetComponentInChildren<FOVElement>());
        }

        for (int i = 0; i < m_fovElements.Count; i++)
        {
            if (!m_fovElements[i].gameObject.activeSelf) continue;

            List<GridHashList2D.Node> enemiesInRange = enemyController.UnitPositions.Find(m_fovElements[i].transform.position, Vector2.one * m_fovElements[i].visionDistance);

            for (int j = 0; j < enemiesInRange.Count; j++)
            {
                Unit_Base enemy = enemyController.UnitsByPosition[enemiesInRange[j]];

                if (m_fovElements[i].CanSeeUnit(enemy))
                {
                    enemy.soldierRenderer.enabled = true;
                    VisibleUnits.Add(enemy);
                }
            }

            //for (int j = 0; j < enemyController.OwnedUnits.Count; j++)
            //{
            //    if (m_fovElements[i].CanSeeUnit(enemyController.OwnedUnits[j]))
            //    {
            //        enemyController.OwnedUnits[j].soldierRenderer.enabled = true;
            //        VisibleUnits.Add(enemyController.OwnedUnits[j]);
            //    }
            //}
        }
    }
}