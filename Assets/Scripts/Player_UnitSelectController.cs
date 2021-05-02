using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player_UnitSelectController : MonoBehaviour
{
    public SelectableManager selectableManager;
    public RectTransform selectionBox;
    public UI_HudManager hudManager;

    public LayerMask hitTestLayers;

    private Vector2 m_startDragPosition;

    private readonly float m_dragThreshold = 5;
    private bool m_inDrag;

    private void Update()
    {
        if(Player_Controller.currentGameState != GameState.Playing)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            m_startDragPosition = Input.mousePosition;

            selectableManager.DeselectAll();

            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, Mathf.Infinity, hitTestLayers);

            if (hit.collider != null)
            {
                var hitSelectable = selectableManager.GetSelectable(hit.collider.gameObject);

                if (hitSelectable != null && hitSelectable is Unit_Base)
                {
                    hitSelectable.Select();
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
                if ((new Vector2(Input.mousePosition.x, Input.mousePosition.y) - m_startDragPosition).magnitude > m_dragThreshold)
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

                var unitScreenPositions = selectableManager.GetAllScreenPositions();

                for (int i = 0; i < unitScreenPositions.Length; i++)
                {
                    Vector2 pos = unitScreenPositions[i];
                    ISelecteble selectable = selectableManager.GetSelectableAt(i);

                    if (selectable is Unit_Base)
                    {
                        if(pos.x > min.x && pos.x < max.x && pos.y > min.y && pos.y < max.y)
                        {
                            selectable.Select();
                        }
                        else
                        {
                            selectable.Deselect();
                        }
                        
                    }
                }

                hudManager.OnSelectionChanged();
            }
        }
    }
}
