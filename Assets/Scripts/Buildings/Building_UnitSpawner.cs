using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building_UnitSpawner : MonoBehaviour
{
    public UnitData[] units;

    public Transform moveTarget;

    [SerializeField] private Player_Controller playerController;

    private void Start()
    {
        SpawnUnit();
    }

    public void SpawnUnit()
    {
        var spawned = SpawnUnit(UnitData.UnitType.Soldier);
        playerController.AddToOwnedUnits(spawned);
        
        spawned.agent.SetDestination(moveTarget.position);
    }

    private Unit_Base SpawnUnit(UnitData.UnitType type)
    {
        for (int i = 0; i < units.Length; i++)
        {
            if(units[i].type == type)
            {
                var instance = Instantiate(units[i].unitPrefab, transform.position, transform.rotation) as GameObject;
                return instance.GetComponent<Unit_Base>();
            }
        }

        return null;
    }
}
