using System.Collections.Generic;
using UnityEngine;

public class UI_HudManager : MonoBehaviour
{
    public enum CursorType { None, Default, Attack, Capture }

    [System.Serializable]
    public class CursorSprite
    {
        public CursorType type;
        public Texture2D tex;
    }

    public Canvas hudCanvas;
    public UI_UnitSelectionRect selectionRectPrefab;

    public SelectableManager selectableManager;

    public CursorSprite[] cursorSprites;

    private List<UI_UnitSelectionRect> m_spawnedSelectionRects;
    private List<Unit_Base> m_currentSelectebles;

    private Camera m_mainCam;

    private CursorType m_curCursorType;

    private void Awake()
    {
        m_mainCam = Camera.main;
        m_spawnedSelectionRects = new List<UI_UnitSelectionRect>();
    }

    private void Update()
    {
        if(m_currentSelectebles != null)
        {
            UpdateSelectionRects();

            if(m_currentSelectebles.Count != selectableManager.CurrentSelectedCount)
            {
                OnSelectionChanged();
            }
        }
    }

    public void SelectUnits(List<Unit_Base> selectables)
    {
        m_currentSelectebles = selectables;

        for (int i = 0; i < m_spawnedSelectionRects.Count; i++)
        {
            m_spawnedSelectionRects[i].gameObject.SetActive(false);
        }

        if (selectables == null)
        {
            for (int i = 0; i < m_spawnedSelectionRects.Count; i++)
            {
                m_spawnedSelectionRects[i].gameObject.SetActive(false);
            }
        }
        else
        {
            if(m_currentSelectebles.Count > m_spawnedSelectionRects.Count)
            {
                int count = m_currentSelectebles.Count - m_spawnedSelectionRects.Count;

                for (int i = 0; i < count; i++)
                {
                    m_spawnedSelectionRects.Add(Instantiate<UI_UnitSelectionRect>(selectionRectPrefab, hudCanvas.transform));
                }
            }

            for (int i = 0; i < m_currentSelectebles.Count; i++)
            {
                m_spawnedSelectionRects[i].gameObject.SetActive(true);
                //m_spawnedSelectionRects[i].selectionRect.rectTransform.sizeDelta = new Vector2(m_currentSelectebles[i].soldierRenderer.sprite.texture.width, m_currentSelectebles[i].soldierRenderer.sprite.texture.height);
            }

            UpdateSelectionRects();
        }
    }

    public void OnSelectionChanged()
    {
        var selected = new List<GameObject>(selectableManager.GetCurrentSelectedObjects());

        if (selected != null && selected.Count > 0)
        {
            List<Unit_Base> selectedList = new List<Unit_Base>();

            for (int i = 0; i < selected.Count; i++)
            {
                Unit_Base unit = selected[i].GetComponent<Unit_Base>();
                selectedList.Add(unit);
            }

            SelectUnits(selectedList);
        }
        else
        {
            SelectUnits(null);
        }
    }

    public void ChangeCursor(CursorType type)
    {
        if(m_curCursorType != type)
        {
            m_curCursorType = type;

            for (int i = 0; i < cursorSprites.Length; i++)
            {
                if(cursorSprites[i].type == type)
                {
                    Cursor.SetCursor(cursorSprites[i].tex, Vector2.zero, CursorMode.ForceSoftware);
                    break;
                }
            }
        }
    }

    private void UpdateSelectionRects()
    {
        for (int i = 0; i < m_currentSelectebles.Count; i++)
        {
            Unit_Base curSelected = m_currentSelectebles[i];
            (m_spawnedSelectionRects[i].transform as RectTransform).anchoredPosition = m_mainCam.WorldToScreenPoint(curSelected.soldierRenderer.transform.position);
            m_spawnedSelectionRects[i].selectionRect.rectTransform.sizeDelta = curSelected.soldierRenderer.sprite.pixelsPerUnit * curSelected.transform.localScale * 0.5f;

            float width = m_spawnedSelectionRects[i].selectionRect.rectTransform.sizeDelta.x;
            m_spawnedSelectionRects[i].healthBar.rectTransform.sizeDelta = new Vector2(width, width * 0.1f);

            m_spawnedSelectionRects[i].SetHealthBar(m_currentSelectebles[i].health.CurrentHealth, m_currentSelectebles[i].health.maxHealth);
        }
    }
}