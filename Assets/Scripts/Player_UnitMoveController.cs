using System.Collections.Generic;
using UnityEngine;

public class Player_UnitMoveController : MonoBehaviour
{
    public UI_HudManager hudManager;
    [SerializeField] private SelectableManager m_selectableManager;
    [SerializeField] private DamageableManager m_damageableManager;
    [SerializeField] private Renderer m_mapRenderer;
    [SerializeField] private LayerMask attackableLayers;
    [SerializeField] private Canvas m_minimapCanvas;

    private Camera m_mainCam;
    private Plane m_groundPlane;

    private void Awake()
    {
        m_mainCam = Camera.main;
        m_groundPlane = new Plane(-Vector3.forward, m_mapRenderer.transform.position);
    }

    private void OnDrawGizmos()
    {
        Color prevColor = Gizmos.color;
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(m_mapRenderer.transform.position, m_mapRenderer.transform.position + (Vector3.right + Vector3.up) * 100.0f);
        Gizmos.DrawLine(m_mapRenderer.transform.position, m_mapRenderer.transform.position - Vector3.forward * 200.0f);
        Gizmos.color = prevColor;
    }

    private void Update()
    {
        if(Player_Controller.currentGameState != GameState.Playing)
        {
            return;
        }

        // Force attack cursor on building hack
        RaycastHit2D hit = Physics2D.Raycast(m_mainCam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, Mathf.Infinity, 1 << LayerMask.NameToLayer("Building"));

        if (hit.collider != null)
        {
            var health = hit.collider.gameObject.GetComponent<Building_Health>();

            if (health != null && health.Faction == FactionType.Enemy)
            {
                hudManager.ChangeCursor(UI_HudManager.CursorType.Attack);

                List<GameObject> curSelectedUnits = new List<GameObject>(m_selectableManager.GetCurrentSelectedObjects());

                for (int i = 0; i < curSelectedUnits.Count; i++)
                {
                    curSelectedUnits[i].GetComponent<Unit_Base>().SetAttackTarget(health);
                }
            }
            else
            {
                Building_Resource resourceBuilding = hit.collider.gameObject.GetComponent<Building_Resource>();

                if (resourceBuilding != null && resourceBuilding.ownerFaction != FactionType.Player)
                {
                    hudManager.ChangeCursor(UI_HudManager.CursorType.Capture);

                    List<GameObject> curSelectedUnits = new List<GameObject>(m_selectableManager.GetCurrentSelectedObjects());

                    for (int i = 0; i < curSelectedUnits.Count; i++)
                    {
                        curSelectedUnits[i].GetComponent<Unit_Base>().SetMoveTarget(resourceBuilding.transform.position);
                        curSelectedUnits[i].GetComponent<Unit_Base>().SetState(Unit_Base.UnitStateType.Move);
                    }
                }
            }

            return;
        }
        // ~hack

        Ray camRay = m_mainCam.ScreenPointToRay(Input.mousePosition);
        float dist;

        if (m_groundPlane.Raycast(camRay, out dist))
        {
            List<ISelecteble> curSelectedUnits = new List<ISelecteble>(m_selectableManager.GetCurrentSelected());

            for (int i = curSelectedUnits.Count - 1; i >= 0; i--)
            {
                Unit_Base unit;
                if((unit = curSelectedUnits[i] as Unit_Base) == null || unit.health.Faction != FactionType.Player)
                {
                    curSelectedUnits.Remove(curSelectedUnits[i]);
                }
            }

            const float radiusPerUnit = 0.25f;
            float checkEnemiesRadius = curSelectedUnits.Count * radiusPerUnit;

            Vector3 hitPos = camRay.GetPoint(dist);
            hitPos.z = 0;

            List<IDamageable> hits = new List<IDamageable>(m_damageableManager.GetAtPosition(hitPos, checkEnemiesRadius, FactionType.Enemy));

            if (hits.Count > 0)
            {
                if (curSelectedUnits.Count > 0)
                {
                    hudManager.ChangeCursor(UI_HudManager.CursorType.Attack);
                }

                if (Input.GetMouseButtonUp(1) && !UI_Helpers.IsPointerOverCanvasElement(m_minimapCanvas))
                {
                    for (int i = 0; i < curSelectedUnits.Count; i++)
                    {
                        Vector2 enemyPos = hits[i % hits.Count].DamageableGameObject.transform.position;
                        List<Vector2> formationPositions = Formations.GetPositionListCircle(enemyPos, new float[] { 1f, 2f, 3f, 4f, 5f }, new int[] { 5, 10, 20, 40, 60 });
                        Unit_Base unit = curSelectedUnits[i] as Unit_Base;
                        unit.SetAttackTarget(hits[0]);
                        unit.SetMoveTarget(Formations.GetAdjustedPosition(formationPositions[i], unit, unit.circleCollider.radius * 1.5f));
                        unit.SetState(Unit_Base.UnitStateType.Move);
                    }
                }
            }
            else
            {
                hudManager.ChangeCursor(UI_HudManager.CursorType.Default);

                if (Input.GetMouseButtonUp(1) && !UI_Helpers.IsPointerOverCanvasElement(m_minimapCanvas))
                {
                    List<Vector2> formationPositions = Formations.GetPositionListCircle(hitPos, new float[] { 1f, 2f, 3f, 4f, 5f }, new int[] { 5, 10, 20, 40, 60 });

                    for (int i = 0; i < curSelectedUnits.Count; i++)
                    {
                        Unit_Base unit = curSelectedUnits[i] as Unit_Base;
                        unit.SetAttackTarget(null);

                        
                        unit.SetMoveTarget(Formations.GetAdjustedPosition(formationPositions[i], unit, unit.circleCollider.radius * 1.5f));
                        unit.SetState(Unit_Base.UnitStateType.Move);
                    }
                }
            }
        }
    }
}