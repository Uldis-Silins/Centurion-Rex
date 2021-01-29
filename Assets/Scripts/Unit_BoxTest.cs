using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit_BoxTest : MonoBehaviour, ISelecteble
{
    public SelectableManager selectableManager;

    [SerializeField] private SpriteRenderer m_soldierRenderer;
    private Color m_startColor;
    private readonly int m_colorPropID = Shader.PropertyToID("_Color");

    public bool IsSelected { get; private set; }

    private void Awake()
    {
        m_startColor = m_soldierRenderer.material.GetColor(m_colorPropID);
    }

    private void Start()
    {
        selectableManager.RegisterSelectable(this, gameObject);
        Debug.Log("Registering " + name);
    }

    //private void OnDisable()
    //{
    //    selectableManager.UnregisterSelectable(this);
    //}

    public void Select()
    {
        IsSelected = true;
        m_soldierRenderer.material.SetColor(m_colorPropID, Color.green);
    }

    public void Deselect()
    {
        IsSelected = false;
        m_soldierRenderer.material.SetColor(m_colorPropID, m_startColor);
    }
}
