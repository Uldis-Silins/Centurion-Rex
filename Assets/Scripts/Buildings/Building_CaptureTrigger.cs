using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class Building_CaptureTrigger : MonoBehaviour
{
    public Building_Resource building;
    public AudioSource audioSource;

    private int m_unitLayer;

    private void Awake()
    {
        m_unitLayer = LayerMask.NameToLayer("Unit");
        var col = GetComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = building.captureRadius;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Building capture trigger enter other layer: " + LayerMask.LayerToName(other.gameObject.layer));
        if(other.gameObject.layer == m_unitLayer)
        {
            var unitHealth = other.gameObject.GetComponent<Unit_Health>();

            if(unitHealth != null)
            {
                building.SetOwner(unitHealth.Faction);

                if(unitHealth.Faction == FactionType.Player)
                {
                    audioSource.Play();
                }
            }
        }
    }
}