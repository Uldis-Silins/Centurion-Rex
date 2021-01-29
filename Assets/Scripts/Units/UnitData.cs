using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Unit Data", menuName = "Units/Data", order = 1)]
public class UnitData : ScriptableObject
{
    public enum UnitType { None, Soldier, Ranged, Cavalry }

    //TODO: Faction?
    public UnitType type;
    public GameObject unitPrefab;
}
