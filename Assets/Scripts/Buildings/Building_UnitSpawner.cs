using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building_UnitSpawner : MonoBehaviour
{
    public UnitData[] units;

    [SerializeField] private SelectableManager selectableManager;

    private List<Unit_Base> m_spawnedUnits;

    private void Awake()
    {
        m_spawnedUnits = new List<Unit_Base>();
    }

    private void Start()
    {
        SpawnUnit();
    }

    public void SpawnUnit()
    {
        var spawned = SpawnUnit(UnitData.UnitType.Soldier);
        m_spawnedUnits.Add(spawned);
        selectableManager.RegisterSelectable(spawned.GetComponent<ISelecteble>(), spawned.gameObject);
        spawned.agent.SetDestination(transform.position + new Vector3(Random.insideUnitCircle.x * 5f, 0f, Random.insideUnitCircle.y * 5f));
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
