using System.Collections;
using UnityEngine;

public class VisibilityManager : MonoBehaviour
{
    public Player_Controller playerController;
    public Player_Controller enemyController;

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
    }
}