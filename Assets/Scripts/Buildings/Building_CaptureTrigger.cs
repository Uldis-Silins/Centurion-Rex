using System.Collections;
using UnityEngine;

public class Building_CaptureTrigger : MonoBehaviour
{
    public Building_Resource building;
    public AudioSource audioSource;

    private int m_unitLayer;

    private void Awake()
    {
        m_unitLayer = LayerMask.GetMask("Unit"/*, "Selectable"*/);
    }

    private void Update()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, building.captureRadius, m_unitLayer);

        int playerUnitsInRange = 0;
        int enemyUnitsInRange = 0;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D other = hits[i];
            //Debug.Log("Building capture trigger enter other layer: " + LayerMask.LayerToName(other.gameObject.layer));

            var unitHealth = other.gameObject.GetComponent<Unit_Health>();

            if (unitHealth != null)
            {
                if(unitHealth.Faction == OwnerType.Player)
                {
                    playerUnitsInRange++;
                }
                else if(unitHealth.Faction == OwnerType.Enemy)
                {
                    enemyUnitsInRange++;
                }
            }
        }

        if (playerUnitsInRange > 0 && playerUnitsInRange > enemyUnitsInRange && building.ownerFaction != OwnerType.Player)
        {
            building.SetOwner(OwnerType.Player);
            audioSource.Play();
        }
        else if(enemyUnitsInRange > 0 && enemyUnitsInRange > playerUnitsInRange && building.ownerFaction != OwnerType.Enemy)
        {
            building.SetOwner(OwnerType.Enemy);
        }
    }
}