using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Unit_Base : MonoBehaviour
{
    [SerializeField] protected SpriteRenderer m_soldierRenderer;

    protected Camera m_mainCam;

    protected virtual void Awake()
    {
        m_mainCam = Camera.main;
    }

    protected virtual void LateUpdate()
    {
        // billboard
        m_soldierRenderer.transform.rotation = m_mainCam.transform.rotation;
    }
}
