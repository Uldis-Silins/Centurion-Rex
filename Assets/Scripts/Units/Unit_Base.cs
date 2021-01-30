using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class Unit_Base : MonoBehaviour
{
    public NavMeshAgent agent;
    public NavMeshObstacle obstacle;
    public Animator anim;

    public SpriteRenderer soldierRenderer;

    public float visionDistance;
    public Transform fovTransform;

    protected Camera m_mainCam;

    private Vector3 m_startScale;
    protected KeyValuePair<GameObject, IDamageable> m_currentTarget;

    private readonly int m_velocityAnimID = Animator.StringToHash("velocity");

    public bool HasAttackTarget { get { return m_currentTarget.Key != null && m_currentTarget.Value != null; } }
    public virtual float AttackDistance { get; }

    protected virtual void Awake()
    {
        m_mainCam = Camera.main;

        m_startScale = soldierRenderer.transform.localScale;
        
        if(fovTransform != null)
        {
            fovTransform.localScale = new Vector3(visionDistance * 2f, 1f, visionDistance * 2f);
        }
    }

    protected virtual void Update()
    {
        if (agent.velocity.magnitude > 0.25f)
        {
            if (Vector3.Dot(Vector3.right, agent.velocity) < 0)
            {
                soldierRenderer.transform.localScale = new Vector3(-m_startScale.x, m_startScale.y, m_startScale.z);
            }
            else
            {
                soldierRenderer.transform.localScale = new Vector3(m_startScale.x, m_startScale.y, m_startScale.z);
            }
        }

        //if(agent.enabled &&!agent.isStopped && agent.velocity.sqrMagnitude < 0.5f && agent.remainingDistance <= agent.stoppingDistance)
        //{
        //    agent.enabled = false;
        //    //obstacle.enabled = true;
        //}

        anim.SetFloat(m_velocityAnimID, Mathf.Clamp(agent.velocity.magnitude, 0.2f, 1f));
    }

    protected virtual void LateUpdate()
    {
        // billboard
        soldierRenderer.transform.rotation = m_mainCam.transform.rotation;
    }

    public virtual void SetAttackState(IDamageable target, GameObject obj)
    {
        m_currentTarget = new KeyValuePair<GameObject, IDamageable>(obj, target);
    }
}
