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

                List<GameObject> curSelectedUnits = new List<GameObject>(m_selectableManager.GetCurrentSelectedObjects());

                for (int i = 0; i < curSelectedUnits.Count; i++)
                {
                    curSelectedUnits[i].GetComponent<Unit_Base>().SetAttackTarget(health, health.gameObject);
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
                    List<Vector3> targetPositions = GetPositionListCircle(hitPos, new float[] { 1f, 2f, 3f, 4f, 5f }, new int[] { 5, 10, 20, 40, 60 });

                    for (int i = 0; i < curSelectedUnits.Count; i++)
                    {
                        curSelectedUnits[i].GetComponent<Unit_Base>().SetAttackTarget(hits[0].Value, hits[0].Key);
                        curSelectedUnits[i].GetComponent<Unit_Base>().SetMoveTarget(targetPositions[i % targetPositions.Count]);
                        curSelectedUnits[i].GetComponent<Unit_Base>().SetState(Unit_Base.UnitStateType.Move);
                    }
                }
            }
            else
            {
                hudManager.ChangeCursor(UI_HudManager.CursorType.Default);

                if (Input.GetMouseButtonUp(1))
                {
                    List<Vector3> targetPositions = GetPositionListCircle(hitPos, new float[] { 1f, 2f, 3f, 4f, 5f }, new int[] { 5, 10, 20, 40, 60 });

                    for (int i = 0; i < curSelectedUnits.Count; i++)
                    {
                        Unit_Base unit = curSelectedUnits[i].GetComponent<Unit_Base>();
                        unit.SetAttackTarget(null, null);

                        unit.SetMoveTarget(targetPositions[i % targetPositions.Count]);
                        unit.SetState(Unit_Base.UnitStateType.Move);
                    }
                }
            }
        }
    }

    private List<Vector3> GetPositionListCircle(Vector3 startPos, float[] dist, int[] posCount)
    {
        List<Vector3> positions = new List<Vector3>();
        positions.Add(startPos);

        for (int i = 0; i < dist.Length; i++)
        {
            positions.AddRange(GetPositionListCircle(startPos, dist[i], posCount[i]));
        }

        return positions;
    }

    private List<Vector3> GetPositionListCircle(Vector3 startPos, float dist, int posCount)
    {
        List<Vector3> positions = new List<Vector3>();

        for (int i = 0; i < posCount; i++)
        {
            float angle = i * (360f / posCount);
            Vector3 dir = Quaternion.Euler(0f, angle, 0f) * Vector3.right;
            positions.Add(startPos + dir * dist);
        }

        return positions;
    }
}