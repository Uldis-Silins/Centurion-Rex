using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Unit Data", menuName = "Units/Data", order = 1)]
public class UnitData : ScriptableObject
{
    public enum FactionType { None, Roman, Samurai }
    public enum UnitType { None, Soldier, Ranged, Cavalry }

    public FactionType faction;
    public UnitType type;
    public Unit_Base unitPrefab;
    public Sprite uiSprite;

    public float price;
    public float buildTime;

    public UnitStatsData statsData;
}
