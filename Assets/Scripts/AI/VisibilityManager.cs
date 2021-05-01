﻿using System.Collections.Generic;
using UnityEngine;

public class VisibilityManager : MonoBehaviour
{
    public Player_Controller playerController;
    public Player_Controller enemyController;

    public bool checkUnitVisibility;

    private List<FOVElement> m_fovElements;

    public HashSet<Unit_Base> VisibleUnits { get; private set; }
    public HashSet<GameObject> VisibleBuildings { get; private set; }

    private void Awake()
    {
        m_fovElements = new List<FOVElement>(GameObject.FindObjectsOfType<FOVElement>());
        VisibleBuildings = new HashSet<GameObject>();
        VisibleUnits = new HashSet<Unit_Base>();
    }

    private void Update()
    {
        if (enemyController == null || enemyController.OwnedUnits == null || !checkUnitVisibility) return;

        VisibleBuildings.Clear();
        VisibleUnits.Clear();

        for (int i = 0; i < enemyController.OwnedUnits.Count; i++)
        {
            enemyController.OwnedUnits[i].soldierRenderer.enabled = false;
        }

        for (int i = 0; i < playerController.OwnedUnits.Count; i++)
        {
            for (int j = 0; j < enemyController.OwnedUnits.Count; j++)
            {
                if (playerController.OwnedUnits[i].CanSeeUnit(enemyController.OwnedUnits[j]))
                {
                    enemyController.OwnedUnits[j].soldierRenderer.enabled = true;
                    VisibleUnits.Add(enemyController.OwnedUnits[j]);
                }
            }

            for (int j = 0; j < enemyController.ownedBuildings.Count; j++)
            {
                if(Vector2.Distance(playerController.OwnedUnits[i].transform.position, enemyController.ownedBuildings[j].selectable.transform.position) <= playerController.OwnedUnits[i].visionDistance)
                {
                    VisibleBuildings.Add(enemyController.ownedBuildings[j].selectable);
                }
            }
        }

        for (int i = 0; i < m_fovElements.Count; i++)
        {
            if (!m_fovElements[i].gameObject.activeInHierarchy) continue;

            for (int j = 0; j < enemyController.OwnedUnits.Count; j++)
            {
                if (m_fovElements[i].CanSeeUnit(enemyController.OwnedUnits[j]))
                {
                    enemyController.OwnedUnits[j].soldierRenderer.enabled = true;
                    VisibleUnits.Add(enemyController.OwnedUnits[j]);
                }
            }
        }
    }
}