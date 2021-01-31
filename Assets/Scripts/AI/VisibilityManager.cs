using System.Collections.Generic;
using UnityEngine;

public class VisibilityManager : MonoBehaviour
{
    public Player_Controller playerController;
    public Player_Controller enemyController;

    private List<FOVElement> m_fovElements;

    private void Awake()
    {
        m_fovElements = new List<FOVElement>(GameObject.FindObjectsOfType<FOVElement>());
    }

    private void Update()
    {
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
                }
            }
        }
    }
}