using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit_Peditatus : Unit_Base, ISelecteble
{
    public float attackDistance = 2f;
    public float attackDamage = 5f;
    public float attacksDelay = 0.5f;

    private Color m_startColor;
    private readonly int m_colorPropID = Shader.PropertyToID("_Color");

    private float m_attackTimer;

    public bool IsSelected { get; private set; }
    public override float AttackDistance { get { return attackDistance; } }

    protected override void Awake()
    {
        base.Awake();

        m_startColor = soldierRenderer.material.GetColor(m_colorPropID);
    }

    protected override void Update()
    {
        base.Update();

        anim.SetBool("attack", false);

        if (m_currentTarget.Key != null && m_currentTarget.Value != null)
        {
            Vector3 targetPos = m_currentTarget.Key.transform.position;
            targetPos.y = transform.position.y;

            if (Vector3.Distance(targetPos, transform.position) > attackDistance && !seeker.IsMoving)
            {
                Vector3 dir = (targetPos - transform.position).normalized;
                seeker.SetDestination(targetPos - dir * attackDistance * 0.5f);
            }
            else
            {
                seeker.Stop();

                if (m_attackTimer < 0.2f)
                {
                    anim.SetBool("attack", true);
                }

                if (m_attackTimer < 0f)
                {
                    m_currentTarget.Value.SetDamage(attackDamage, gameObject);
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
        //m_soldierRenderer.material.SetColor(m_colorPropID, Color.green);
    }

    public void Deselect()
    {
        IsSelected = false;
        //m_soldierRenderer.material.SetColor(m_colorPropID, m_startColor);
    }

    public override void SetAttackState(IDamageable target, GameObject obj)
    {
        base.SetAttackState(target, obj);

        m_attackTimer = attacksDelay;
    }
}
