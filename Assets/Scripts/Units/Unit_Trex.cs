using System.Collections;
using UnityEngine;

public class Unit_Trex : Unit_Base, ISelecteble
{
    public float attackDistance = 2f;
    public float attackDamage = 5f;
    public float attacksDelay = 0.5f;

    private Color m_startColor;
    private readonly int m_colorPropID = Shader.PropertyToID("_Color");

    private float m_attackTimer;

    public bool IsSelected { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        m_startColor = m_soldierRenderer.material.GetColor(m_colorPropID);
    }

    private void Update()
    {
        if (m_currentTarget.Key != null && m_currentTarget.Value != null)
        {
            Vector3 targetPos = m_currentTarget.Key.transform.position;
            targetPos.y = transform.position.y;

            if (Vector3.Distance(targetPos, transform.position) > attackDistance)
            {
                agent.enabled = true;
                obstacle.enabled = false;

                Vector3 dir = (transform.position - m_currentTarget.Key.transform.position).normalized;
                agent.SetDestination(m_currentTarget.Key.transform.position + dir * attackDistance);
            }
            else
            {
                if (m_attackTimer < 0f)
                {
                    m_currentTarget.Value.SetDamage(attackDamage);
                    m_attackTimer = attacksDelay;
                }

                m_attackTimer -= Time.deltaTime;
            }
        }
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

    public override void SetAttackState(IDamageable target, GameObject obj)
    {
        base.SetAttackState(target, obj);

        m_attackTimer = attacksDelay;
    }
}
