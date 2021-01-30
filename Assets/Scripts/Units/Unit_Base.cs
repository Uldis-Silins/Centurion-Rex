using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class Unit_Base : MonoBehaviour
{
    public NavMeshAgent agent;
    public NavMeshObstacle obstacle;
    public Animator anim;

    [SerializeField] protected SpriteRenderer m_soldierRenderer;

    protected Camera m_mainCam;

    private Vector3 m_startScale;
    protected KeyValuePair<GameObject, IDamageable> m_currentTarget;

    private readonly int m_velocityAnimID = Animator.StringToHash("velocity");

    public bool HasAttackTarget { get { return m_currentTarget.Key != null && m_currentTarget.Value != null; } }

    protected virtual void Awake()
    {
        m_mainCam = Camera.main;

        m_startScale = transform.localScale;
    }

    protected virtual void Update()
    {
        if (agent.velocity.magnitude > 0.25f)
        {
            if (Vector3.Dot(Vector3.right, agent.velocity) < 0)
            {
                transform.localScale = new Vector3(-m_startScale.x, m_startScale.y, m_startScale.z);
            }
            else
            {
                transform.localScale = new Vector3(m_startScale.x, m_startScale.y, m_startScale.z);
            }
        }

        //if(agent.enabled &&!agent.isStopped && agent.velocity.sqrMagnitude < 0.5f && agent.remainingDistance <= agent.stoppingDistance)
        //{
        //    agent.enabled = false;
        //    //obstacle.enabled = true;
        //}

        anim.SetFloat(m_velocityAnimID, Mathf.Clamp(agent.velocity.magnitude, 0.25f, 1f));
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
