using System.Collections;
using UnityEngine;

public class Unit_Trex : Unit_Base, ISelecteble
{
    public float attackDistance = 2f;
    public float attackDamage = 5f;
    public float attacksDelay = 0.5f;

    public ParticleSystem fireParticles;

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

        if (m_currentTarget.Key != null && m_currentTarget.Value != null)
        {
            Vector3 targetPos = m_currentTarget.Key.transform.position;
            targetPos.y = transform.position.y;

            if (Vector3.Distance(targetPos, transform.position) > attackDistance)
            {
                anim.SetBool("attack", false);
                Vector3 dir = (targetPos - transform.position).normalized;
                seeker.SetDestination(targetPos - dir * attackDistance * 0.5f);
            }
            else
            {
                seeker.Stop();

                if (!anim.GetBool("attack") && m_attackTimer < attacksDelay / 2f)
                {
                    anim.SetBool("attack", true);

                    if (!soundSource.isPlaying)
                    {
                        soundSource.clip = attackClip;
                        soundSource.loop = false;
                        soundSource.Play();
                    }
                }

                if (m_attackTimer < 0f)
                {
                    m_currentTarget.Value.SetDamage(attackDamage, gameObject);
                    m_attackTimer = attacksDelay;
                    fireParticles.transform.position = m_currentTarget.Key.transform.position;
                    fireParticles.Play();
                    anim.SetBool("attack", false);
                }

                m_attackTimer -= Time.deltaTime;
            }
        }
        else
        {
            anim.SetBool("attack", false);
        }
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();
    }

    public void Select()
    {
        IsSelected = true;
        //soldierRenderer.material.SetColor(m_colorPropID, Color.green);
    }

    public void Deselect()
    {
        IsSelected = false;
        //soldierRenderer.material.SetColor(m_colorPropID, m_startColor);
    }

    public override void SetAttackState(IDamageable target, GameObject obj)
    {
        base.SetAttackState(target, obj);

        m_attackTimer = attacksDelay;
    }
}
