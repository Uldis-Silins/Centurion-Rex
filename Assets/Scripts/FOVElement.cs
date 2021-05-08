using System.Collections;
using UnityEngine;

public class FOVElement : MonoBehaviour
{
    public float visionDistance;

    private void Start()
    {
        transform.localScale = new Vector3(visionDistance * 2, visionDistance * 2, 1f);
    }

    public bool CanSeeUnit(Unit_Base unit)
    {
        return Vector2.SqrMagnitude(transform.position - unit.transform.position) < visionDistance * visionDistance;
    }
}