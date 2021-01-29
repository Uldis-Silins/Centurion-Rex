using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class Unit_Base : MonoBehaviour
{
    public NavMeshAgent agent;

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
