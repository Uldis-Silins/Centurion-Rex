using System.Collections;
using UnityEngine;

public class FOVElement : MonoBehaviour
{
    public float visionDistance;

    public bool CanSeeUnit(Unit_Base unit)
    {
        return Vector3.Distance(transform.position, unit.transform.position) <= visionDistance;
    }
}