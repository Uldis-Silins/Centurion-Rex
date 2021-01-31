using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building_UnitSpawner : MonoBehaviour, ISelecteble
{
    public UnitData[] units;

    public Transform spawnPoint;
    public Transform moveTarget;

    [SerializeField] private Player_Controller playerController;

    public bool IsSelected { get; protected set; }

    public void OnSpawnUnitClick(int unitIndex)
    {
        var spawned = SpawnUnit(units[unitIndex].type);

        if (spawned != null)
        {
            playerController.AddToOwnedUnits(spawned);
            spawned.seeker.SetDestination(moveTarget.position);
        }
    }

    private Unit_Base SpawnUnit(UnitData.UnitType type)
    {
        for (int i = 0; i < units.Length; i++)
        {
            if(units[i].type == type)
            {
                if (playerController.currentResources >= units[i].price)
                {
                    var instance = Instantiate(units[i].unitPrefab, spawnPoint.position, Quaternion.identity) as GameObject;
                    instance.name = units[i].unitPrefab.name + playerController.CurrentPopulation;
                    playerController.AddResource(-units[i].price);
                    return instance.GetComponent<Unit_Base>();
                }
            }
        }

        return null;
    }

    public void Select()
    {
        IsSelected = true;
    }

    public void Deselect()
    {
        IsSelected = false;
    }
}
