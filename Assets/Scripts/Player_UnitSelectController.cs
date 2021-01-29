using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player_UnitSelectController : MonoBehaviour
{
    public SelectableManager selectableManager;
    public RectTransform selectionBox;

    private Vector2 m_startDragPosition;

    private readonly float m_singleSelectDistance = 15.0f;
    private readonly float m_dragThreshold = 5;
    private bool m_inDrag;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            m_startDragPosition = Input.mousePosition;

            var unitScreenPositions = selectableManager.GetAllScreenPositions();

            for (int i = 0; i < unitScreenPositions.Length; i++)
            {
                if(Vector2.Distance(Input.mousePosition, unitScreenPositions[i]) < m_singleSelectDistance)
                {
                    selectableManager.GetSelectableAt(i).Select();
                }
                else
                {
                    selectableManager.GetSelectableAt(i).Deselect();
                }
            }
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

                    if (pos.x > min.x && pos.x < max.x && pos.y > min.y && pos.y < max.y)
                    {
                        selectableManager.GetSelectableAt(i).Select();
                    }
                    else
                    {
                        selectableManager.GetSelectableAt(i).Deselect();
                    }
                }
            }
        }
    }
}
