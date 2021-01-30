using System.Collections.Generic;
using UnityEngine;

public class Player_UnitMoveController : MonoBehaviour
{
    public UI_HudManager hudManager;
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
        if(Player_Controller.currentGameState != GameState.Playing)
        {
            return;
        }

        Ray camRay = m_mainCam.ScreenPointToRay(Input.mousePosition);

        // Force attack cursor on building hack
        RaycastHit hit;

        if (Physics.Raycast(camRay, out hit, 1000f, 1 << LayerMask.NameToLayer("Building")))
        {
            var health = hit.collider.gameObject.GetComponent<Building_Health>();

            if (health != null && health.Faction == FactionType.Enemy)
            {
                hudManager.ChangeCursor(UI_HudManager.CursorType.Attack);
            }

            return;
        }
        // ~hack

        float dist;

        if (m_groundPlane.Raycast(camRay, out dist))
        {
            List<GameObject> curSelectedUnits = new List<GameObject>(m_selectableManager.GetCurrentSelectedObjects());

            Vector3 hitPos = camRay.GetPoint(dist);
            hitPos.y = 0;

            List<KeyValuePair<GameObject, IDamageable>> hits = new List<KeyValuePair<GameObject, IDamageable>>(m_damageableManager.GetAtPosition(hitPos, 1f, FactionType.Enemy));

            if (hits.Count > 0)
            {
                if (curSelectedUnits.Count > 0)
                {
                    hudManager.ChangeCursor(UI_HudManager.CursorType.Attack);
                }

                if (Input.GetMouseButtonUp(1))
                {
                    for (int i = 0; i < curSelectedUnits.Count; i++)
                    {
                        curSelectedUnits[i].GetComponent<Unit_Base>().SetAttackState(hits[0].Value, hits[0].Key);
                    }
                }
            }
            else
            {
                hudManager.ChangeCursor(UI_HudManager.CursorType.Default);

                if (Input.GetMouseButtonUp(1))
                {
                    for (int i = 0; i < curSelectedUnits.Count; i++)
                    {
                        Unit_Base unit = curSelectedUnits[i].GetComponent<Unit_Base>();
                        unit.agent.enabled = true;
                        //unit.obstacle.enabled = false;
                        unit.SetAttackState(null, null);
                        unit.agent.SetDestination(hitPos);
                    }
                }
            }
        }
    }
}