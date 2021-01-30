using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class Building_CaptureTrigger : MonoBehaviour
{
    public Building_Resource building;

    private int m_unitLayer;

    private void Awake()
    {
        m_unitLayer = LayerMask.NameToLayer("Unit");
        var col = GetComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = building.captureRadius;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("other layer: " + LayerMask.LayerToName(other.gameObject.layer));
        if(other.gameObject.layer == m_unitLayer)
        {
            var unitHealth = other.gameObject.GetComponent<Unit_Health>();

            if(unitHealth != null)
            {
                building.SetOwner(unitHealth.Faction);
            }
        }
    }
}