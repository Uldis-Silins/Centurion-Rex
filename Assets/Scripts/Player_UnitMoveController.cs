using System.Collections.Generic;
using UnityEngine;

public class Player_UnitMoveController : MonoBehaviour
{
    [SerializeField] private SelectableManager m_selectableManager;
    [SerializeField] private DamageableManager m_damageableManager;
    [SerializeField] private Renderer m_mapRenderer;
    [SerializeField] private LayerMask attackableLayers;

    private Camera m_mainCam;
    private Plane m_groundPlane;

    private void Awake()
    {
        m_mainCam = Camera.main;
        m_groundPlane = new Plane(m_mapRenderer.transform.up, m_mapRenderer.transform.position);
    }

    private void Update()
    {
        if(Input.GetMouseButtonUp(1))
        {
            //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            //RaycastHit hit;

            //Debug.DrawRay(ray.origin, ray.direction * 30f, Color.red, 10f);

            //if (Physics.Raycast(ray, out hit, 1000f, attackableLayers))
            //{
            //    var hitDamageable = m_damageableManager.GetDamageable(hit.collider.gameObject);

            //    if (hitDamageable.Faction == FactionType.Enemy)
            //    {
            //        List<GameObject> curSelectedUnits = new List<GameObject>(m_selectableManager.GetCurrentSelectedObjects());

            //        for (int i = 0; i < curSelectedUnits.Count; i++)
            //        {
            //            m_selectableManager.GetObjectAt(i).GetComponent<Unit_Base>().SetAttackState(hitDamageable, hit.collider.gameObject);
            //        }
            //    }
            //}
            //else

            {
                float dist;

                Ray camRay = m_mainCam.ScreenPointToRay(Input.mousePosition);

                if (m_groundPlane.Raycast(camRay, out dist))
                {
                    Vector3 hitPos = camRay.GetPoint(dist);
                    hitPos.y = 0;

                    List<KeyValuePair<GameObject, IDamageable> > hits = new List<KeyValuePair<GameObject, IDamageable>>(m_damageableManager.GetAtPosition(hitPos, 5f));

                    if (hits.Count > 0)
                    {
                            List<GameObject> curSelectedUnits = new List<GameObject>(m_selectableManager.GetCurrentSelectedObjects());

                            for (int i = 0; i < curSelectedUnits.Count; i++)
                            {
                                m_selectableManager.GetObjectAt(i).GetComponent<Unit_Base>().SetAttackState(hits[0].Value, hits[0].Key);
                            }
                    }
                    else
                    {
                        List<GameObject> curSelectedUnits = new List<GameObject>(m_selectableManager.GetCurrentSelectedObjects());

                        for (int i = 0; i < curSelectedUnits.Count; i++)
                        {
                            Unit_Base unit = curSelectedUnits[i].GetComponent<Unit_Base>();
                            unit.agent.enabled = true;
                            unit.obstacle.enabled = false;
                            unit.SetAttackState(null, null);
                            unit.agent.SetDestination(hitPos);
                        }
                    }
                }
            }
        }
    }
}