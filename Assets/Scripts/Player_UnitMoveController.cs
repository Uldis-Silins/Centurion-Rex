using System.Collections.Generic;
using UnityEngine;

public class Player_UnitMoveController : MonoBehaviour
{
    [SerializeField] private SelectableManager m_selectableManager;
    [SerializeField] private Renderer m_mapRenderer;

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
            float dist;

            Ray camRay = m_mainCam.ScreenPointToRay(Input.mousePosition);

            if(m_groundPlane.Raycast(camRay, out dist))
            {
                Vector3 hitPos = camRay.GetPoint(dist);
                hitPos.y = 0;

                List<GameObject> curSelectedUnits = new List<GameObject>(m_selectableManager.GetCurrentSelectedObjects());

                for (int i = 0; i < curSelectedUnits.Count; i++)
                {
                    curSelectedUnits[i].GetComponent<Unit_Base>().agent.SetDestination(hitPos);
                }
            }
        }
    }
}