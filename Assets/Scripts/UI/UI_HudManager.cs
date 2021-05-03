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
    public UI_UnitSelectionRect unitSelectionRectPrefab;
    public UI_UnitSelectionRect buildingSelectionRectPrefab;

    public SelectableManager selectableManager;

    public CursorSprite[] cursorSprites;

    private List<UI_UnitSelectionRect> m_spawnedSelectionRects;
    private List<Unit_Base> m_currentSelectedUnits;

    private UI_UnitSelectionRect m_buildingSelectionRect;
    private ISelecteble m_currentSelectedBuilding;

    private Camera m_mainCam;

    private CursorType m_curCursorType;

    private void Awake()
    {
        m_mainCam = Camera.main;
        m_spawnedSelectionRects = new List<UI_UnitSelectionRect>();
    }

    private void Start()
    {
        m_buildingSelectionRect = Instantiate(buildingSelectionRectPrefab, hudCanvas.transform);
        m_buildingSelectionRect.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if(m_currentSelectedUnits != null)
        {
            UpdateUnitSelectionRects();

            if(m_currentSelectedUnits.Count != selectableManager.CurrentSelectedCount)
            {
                OnSelectionChanged();
            }
        }

        if (m_currentSelectedBuilding != null)
        {
            UpdateBuildingSelectionRects();
        }
    }

    public void SelectUnits(List<Unit_Base> selectables)
    {
        m_currentSelectedUnits = selectables;

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
            if(m_currentSelectedUnits.Count > m_spawnedSelectionRects.Count)
            {
                int count = m_currentSelectedUnits.Count - m_spawnedSelectionRects.Count;

                for (int i = 0; i < count; i++)
                {
                    m_spawnedSelectionRects.Add(Instantiate<UI_UnitSelectionRect>(unitSelectionRectPrefab, hudCanvas.transform));
                }
            }

            for (int i = 0; i < m_currentSelectedUnits.Count; i++)
            {
                m_spawnedSelectionRects[i].gameObject.SetActive(true);
                //m_spawnedSelectionRects[i].selectionRect.rectTransform.sizeDelta = new Vector2(m_currentSelectebles[i].soldierRenderer.sprite.texture.width, m_currentSelectebles[i].soldierRenderer.sprite.texture.height);
            }

            UpdateUnitSelectionRects();
        }
    }

    public void SelectBuilding(ISelecteble buildingSelectable)
    {
        if(buildingSelectable == null)
        {
            m_buildingSelectionRect.gameObject.SetActive(false);
            m_currentSelectedBuilding = null;
        }
        else
        {
            m_currentSelectedBuilding = buildingSelectable;
            m_buildingSelectionRect.gameObject.SetActive(true);
            UpdateBuildingSelectionRects();
        }
    }

    public void OnSelectionChanged()
    {
        var selected = new List<ISelecteble>(selectableManager.GetCurrentSelected());

        if (selected != null && selected.Count > 0)
        {
            List<Unit_Base> selectedList = new List<Unit_Base>();

            for (int i = 0; i < selected.Count; i++)
            {
                Unit_Base unit = selected[i] as Unit_Base;

                if (unit != null)
                {
                    selectedList.Add(unit);
                }
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

    private void UpdateUnitSelectionRects()
    {
        float scaleRatio = (Screen.width / 1920f) * 0.5f;
        const float HEALTH_BAR_THICKNESS = 5f;

        for (int i = 0; i < m_currentSelectedUnits.Count; i++)
        {
            Unit_Base curSelected = m_currentSelectedUnits[i];
            if(curSelected.gameObject == null || curSelected.soldierRenderer == null)
            {
                m_spawnedSelectionRects[i].gameObject.SetActive(false);
                continue;
            }
            (m_spawnedSelectionRects[i].transform as RectTransform).anchoredPosition = m_mainCam.WorldToScreenPoint(curSelected.soldierRenderer.bounds.center);
            m_spawnedSelectionRects[i].selectionRect.rectTransform.sizeDelta = (curSelected as ISelecteble).SelectableBounds.size * curSelected.soldierRenderer.sprite.pixelsPerUnit * scaleRatio;

            float width = m_spawnedSelectionRects[i].selectionRect.rectTransform.sizeDelta.x;
            m_spawnedSelectionRects[i].healthBar.rectTransform.sizeDelta = new Vector2(width, HEALTH_BAR_THICKNESS);

            m_spawnedSelectionRects[i].SetHealthBar(m_currentSelectedUnits[i].health.CurrentHealth, m_currentSelectedUnits[i].health.maxHealth);
        }
    }

    private void UpdateBuildingSelectionRects()
    {
        float scaleRatio = (Screen.width / 1920f) * 0.5f;
        Building_Base building = m_currentSelectedBuilding as Building_Base;
        m_buildingSelectionRect.selectionRect.rectTransform.anchoredPosition = m_mainCam.WorldToScreenPoint(building.spriteRenderer.bounds.center);
        m_buildingSelectionRect.selectionRect.rectTransform.sizeDelta = building.SelectableBounds.size * building.spriteRenderer.sprite.pixelsPerUnit * scaleRatio;
        m_buildingSelectionRect.healthBar.gameObject.SetActive(building.health != null);

        if (building.health != null)
        {
            const float HEALTH_BAR_THICKNESS = 10f;
            float width = m_buildingSelectionRect.selectionRect.rectTransform.sizeDelta.x;
            m_buildingSelectionRect.healthBar.rectTransform.sizeDelta = new Vector2(width, HEALTH_BAR_THICKNESS);

            m_buildingSelectionRect.SetHealthBar(building.health.CurrentHealth, building.health.maxHealth);
        }
    }
}