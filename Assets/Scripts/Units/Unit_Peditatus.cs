using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit_Peditatus : Unit_Base, ISelecteble
{
    private Color m_startColor;
    private readonly int m_colorPropID = Shader.PropertyToID("_Color");

    public bool IsSelected { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        m_startColor = m_soldierRenderer.material.GetColor(m_colorPropID);
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();
    }

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
