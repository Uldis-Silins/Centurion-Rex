using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class Unit_Base : MonoBehaviour
{
    public NavMeshAgent agent;

    [SerializeField] protected SpriteRenderer m_soldierRenderer;

    protected Camera m_mainCam;

    private Vector3 m_startScale;
    protected KeyValuePair<GameObject, IDamageable> m_currentTarget;

    protected virtual void Awake()
    {
        m_mainCam = Camera.main;

        m_startScale = transform.localScale;
    }

    private void Update()
    {
        if(Vector3.Dot(Vector3.right, agent.velocity) < 0)
        {
            transform.localScale = new Vector3(-m_startScale.x, m_startScale.y, m_startScale.z);
        }
        else
        {
            transform.localScale = new Vector3(m_startScale.x, m_startScale.y, m_startScale.z);
        }
    }

    protected virtual void LateUpdate()
    {
        // billboard
        m_soldierRenderer.transform.rotation = m_mainCam.transform.rotation;
    }

    public virtual void SetAttackState(IDamageable target, GameObject obj)
    {
        m_currentTarget = new KeyValuePair<GameObject, IDamageable>(obj, target);
    }
}
