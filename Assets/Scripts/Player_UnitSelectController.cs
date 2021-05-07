using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player_UnitSelectController : MonoBehaviour
{
    public SelectableManager selectableManager;
    public Player_Controller ownerController;
    public RectTransform selectionBox;
    public UI_HudManager hudManager;
    public Canvas minimapCanvas;

    private Vector2 m_startDragPosition;
    private Camera m_mainCam;

    private readonly float m_dragThreshold = 5;
    private bool m_inDrag;

    private void Awake()
    {
        m_mainCam = Camera.main;
    }

    private void Update()
    {
        if(Player_Controller.currentGameState != GameState.Playing)
        {
            return;
        }

        if(ownerController.BlockBuildingInteraction)
        {
            return;
        }

        bool isMinimapHit = UI_Helpers.IsPointerOverCanvasElement(minimapCanvas);

        if (!isMinimapHit && Input.GetMouseButtonDown(0))
        {
            m_startDragPosition = Input.mousePosition;

            foreach (var item in selectableManager.GetCurrentSelected())
            {
                //if(item is Unit_Base)
                {
                    item.Deselect();
                }
            }

            //RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, Mathf.Infinity, hitTestLayers);
            Vector2 worldPos = m_mainCam.ScreenToWorldPoint(Input.mousePosition);
            var hits = ownerController.UnitPositions.Find(worldPos, Vector2.one * 0.5f);

            if (hits.Count > 0)
            {
                var unit = ownerController.UnitsByPosition[hits[0]];

                if (unit != null && unit is ISelecteble s)
                {
                    s.Select();
                }
            }

            hudManager.OnSelectionChanged();
        }
        else if(Input.GetMouseButtonUp(0))
        {
            if(selectionBox.gameObject.activeInHierarchy)
            {
                selectionBox.gameObject.SetActive(false);
            }

            m_inDrag = false;
        }
        else if(Input.GetMouseButton(0))
        {
            if(!m_inDrag)
            {
                if (!isMinimapHit && (new Vector2(Input.mousePosition.x, Input.mousePosition.y) - m_startDragPosition).magnitude > m_dragThreshold)
                {
                    m_inDrag = true;

                    if (!selectionBox.gameObject.activeInHierarchy)
                    {
                        selectionBox.gameObject.SetActive(true);
                    }
                }
            }

            if (m_inDrag)
            {
                Vector2 dragSize = new Vector2(Input.mousePosition.x - m_startDragPosition.x, Input.mousePosition.y - m_startDragPosition.y);
                Vector2 halfSize = new Vector2(dragSize.x / 2, dragSize.y / 2);
                selectionBox.sizeDelta = new Vector2(Mathf.Abs(dragSize.x), Mathf.Abs(dragSize.y));
                selectionBox.anchoredPosition = m_startDragPosition + halfSize;

                Vector2 min = selectionBox.anchoredPosition - selectionBox.sizeDelta / 2;
                Vector2 max = selectionBox.anchoredPosition + selectionBox.sizeDelta / 2;

                Vector2 worldMin = m_mainCam.ScreenToWorldPoint(min);
                Vector2 worldMax = m_mainCam.ScreenToWorldPoint(max);

                var hits = ownerController.UnitPositions.Find(worldMin + (worldMax - worldMin) * 0.5f, Vector2.one * (worldMax - worldMin));

                foreach (var item in selectableManager.GetCurrentSelected())
                {
                    item.Deselect();
                }

                if (hits.Count > 0)
                {
                    for (int i = 0; i < hits.Count; i++)
                    {
                        var unit = ownerController.UnitsByPosition[hits[i]];

                        if (unit != null && unit is ISelecteble s)
                        {
                            s.Select();
                        }
                    }
                }

                //var unitScreenPositions = selectableManager.GetAllScreenPositions();

                //for (int i = 0; i < unitScreenPositions.Length; i++)
                //{
                //    Vector2 pos = unitScreenPositions[i];
                //    ISelecteble selectable = selectableManager.GetSelectableAt(i);

                //    if (selectable is Unit_Base)
                //    {
                //        if(pos.x > min.x && pos.x < max.x && pos.y > min.y && pos.y < max.y)
                //        {
                //            selectable.Select();
                //        }
                //        else
                //        {
                //            selectable.Deselect();
                //        }

                //    }
                //}

                hudManager.OnSelectionChanged();
            }
        }
    }
}
