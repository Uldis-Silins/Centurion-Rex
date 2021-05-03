using System.Collections.Generic;
using UnityEngine;

public class Unit_Trex : Unit_Base, ISelecteble
{
    public float attackDistance = 2f;
    public float attackDamage = 5f;
    public float attacksDelay = 0.5f;

    public Transform fireParticleParent;
    public ParticleSystem[] fireParticles;

    [SerializeField] private GameObject m_selectableObject;
    private Bounds m_selectableBounds;

    private Color m_startColor;
    private readonly int m_colorPropID = Shader.PropertyToID("_Color");

    private float m_attackTimer;
    private bool m_isDamageApplied;

    private Dictionary<UnitStateType, StateHandler> m_states;
    private UnitStateType m_currentStateType;

    public bool IsSelected { get; private set; }
    public GameObject SelectableGameObject { get { return m_selectableObject; } }
    public Bounds SelectableBounds { get { return m_selectableBounds; } }
    public override float AttackDistance { get { return attackDistance; } }

    protected override void Awake()
    {
        base.Awake();

        m_startColor = soldierRenderer.material.GetColor(m_colorPropID);

        if (m_selectableObject != null)
        {
            m_selectableBounds = m_selectableObject.GetComponent<Collider2D>().bounds;
        }

        m_states = new Dictionary<UnitStateType, StateHandler>();
        m_states.Add(UnitStateType.None, () => { });
        m_states.Add(UnitStateType.Idle, EnterState_Idle);
        m_states.Add(UnitStateType.Move, EnterState_Move);
        m_states.Add(UnitStateType.Attack, EnterState_Attack);
        m_states.Add(UnitStateType.Die, EnterState_Die);
    }

    protected override void Update()
    {
        base.Update();

        //if (m_attackTarget.Key != null && m_attackTarget.Value != null)
        //{
        //    Vector3 targetPos = m_attackTarget.Key.transform.position;
        //    targetPos.y = transform.position.y;

        //    if (Vector3.Distance(targetPos, transform.position) > attackDistance)
        //    {
        //        anim.SetBool("attack", false);
        //        Vector3 dir = (targetPos - transform.position).normalized;
        //        m_seeker.SetDestination(targetPos + dir * attackDistance * 0.5f);
        //    }
        //    else
        //    {
        //        m_seeker.Stop();

        //        if (Vector3.Dot(Vector3.right, transform.position - targetPos) > 0)
        //        {
        //            soldierRenderer.transform.localScale = new Vector3(-m_startScale.x, m_startScale.y, m_startScale.z);
        //        }
        //        else
        //        {
        //            soldierRenderer.transform.localScale = new Vector3(m_startScale.x, m_startScale.y, m_startScale.z);
        //        }

        //        if (!anim.GetBool("attack") && m_attackTimer < attacksDelay / 2f)
        //        {
        //            anim.SetBool("attack", true);

        //            if (!soundSource.isPlaying)
        //            {
        //                soundSource.clip = attackClip;
        //                soundSource.loop = false;
        //                soundSource.Play();
        //            }
        //        }

        //        if (m_attackTimer < 0f)
        //        {
        //            m_attackTarget.Value.SetDamage(attackDamage, gameObject);
        //            m_attackTimer = attacksDelay;
        //            fireParticles.transform.position = m_attackTarget.Key.transform.position;
        //            fireParticles.Play();
        //            anim.SetBool("attack", false);
        //        }

        //        m_attackTimer -= Time.deltaTime;
        //    }
        //}
        //else
        //{
        //    anim.SetBool("attack", false);
        //}
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();
    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f, m_currentStateType.ToString());
#endif
    }

    public override void SetState(UnitStateType type)
    {
        m_currentStateType = type;

        if (m_currentStateHandler == null || m_currentStateType == UnitStateType.None)
        {
            m_currentStateHandler = m_states[type];
        }
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

    public override void SetAttackTarget(IDamageable target)
    {
        base.SetAttackTarget(target);

        m_attackTimer = attacksDelay;
    }

    #region State Handlers
    protected void EnterState_Idle()
    {
        m_seeker.enabled = false;
        m_obstacleAvoider.enabled = true;
        m_separator.enabled = true;
        m_currentStateHandler = State_Idle;
    }

    protected void State_Idle()
    {
        if (m_currentStateType != UnitStateType.Idle)
        {
            ExitState_Idle(m_currentStateType);
            return;
        }

        Unit_Base attacker = health.Attacker;

        if (!IsSelected && attacker != null)
        {
            if (attacker.unitType != UnitData.UnitType.Soldier)
            {
                SetAttackTarget(attacker.health);
                ExitState_Idle(UnitStateType.Attack);
                return;
            }
            else
            {
                SetMoveTarget(transform.position + (transform.position - attacker.transform.position).normalized * attackDistance);
                ExitState_Idle(UnitStateType.Move);
                return;
            }
        }

        m_moveTarget = transform.position;  // for separator
    }

    protected void ExitState_Idle(UnitStateType targetState)
    {
        m_currentStateType = targetState;
        m_currentStateHandler = m_states[targetState];
    }

    protected void EnterState_Move()
    {
        Debug.Assert(m_hasMoveTarget, "MoveState: No move target set.");
        m_seeker.enabled = true;
        m_avoider.enabled = true;
        m_obstacleAvoider.enabled = true;
        m_pursuer.enabled = false;
        m_separator.enabled = false;

        m_currentStateHandler = State_Move;
    }

    protected void State_Move()
    {
        if (m_currentStateType != UnitStateType.Move)
        {
            ExitState_Move(m_currentStateType);
            return;
        }

        if (HasAttackTarget)
        {
            if (Vector2.Distance(transform.position, m_attackTarget.DamageableGameObject.transform.position) <= attackDistance + m_attackTarget.DamageableRadius)
            {
                ExitState_Move(UnitStateType.Attack);
                return;
            }
        }
        else
        {
            Unit_Base attacker = health.Attacker;

            // Move to attack only if not selected and ignore soldiers
            if (!IsSelected && attacker != null && attacker.unitType != UnitData.UnitType.Soldier && Vector2.Distance(attacker.transform.position, transform.position) < attackDistance)
            {
                SetAttackTarget(attacker.health);
                ExitState_Move(UnitStateType.Attack);
                return;
            }
        }

        if (!m_hasMoveTarget || Vector2.Distance(transform.position, m_moveTarget) <= m_seeker.targetRadius * 1.5f)
        {
            ExitState_Move(UnitStateType.Idle);
            return;
        }
    }

    protected void ExitState_Move(UnitStateType targetState)
    {
        m_avoider.enabled = false;  // stay enabled and change wight?
        //m_obstacleAvoider.enabled = false;
        m_moveTarget = transform.position;
        m_hasMoveTarget = false;
        m_seeker.Stop();

        m_currentStateType = targetState;
        m_currentStateHandler = m_states[targetState];
    }

    protected void EnterState_Attack()
    {
        anim.PlayAnimation(GetAttackAnimation(m_attackTarget.DamageableGameObject.transform.position));
        m_attackTimer = attacksDelay;
        m_isDamageApplied = false;

        m_seeker.enabled = false;
        m_pursuer.enabled = true;
        m_obstacleAvoider.enabled = true;
        m_separator.enabled = true;

        m_pursuer.target = m_attackTarget.DamageableGameObject.GetComponent<Agent>();
        m_currentStateHandler = State_Attack;
    }

    protected void State_Attack()
    {
        if (m_currentStateType != UnitStateType.Attack)
        {
            ExitState_Attack(m_currentStateType);
            return;
        }

        if (!HasAttackTarget || m_attackTarget == null || m_attackTarget.DamageableGameObject == null || m_attackTarget.CurrentHealth <= 0)
        {
            ExitState_Attack(UnitStateType.Idle);
            return;
        }

        if (Vector2.Distance(transform.position, m_attackTarget.DamageableGameObject.transform.position) > attackDistance)
        {
            m_pursuer.SetDestination(m_attackTarget.DamageableGameObject.transform.position);
            return;
        }
        else
        {
            m_pursuer.Stop();
        }

        Unit_Base attacker = health.Attacker;

        if (!IsSelected && attacker != null && Vector2.Distance(m_pursuer.MoveTarget, transform.position) < 0.1f)   // Check if pursuer is stopped
        {
            if (attacker.unitType == UnitData.UnitType.Soldier)
            {
                SetMoveTarget(transform.position + (transform.position - attacker.transform.position).normalized * attackDistance);
                ExitState_Idle(UnitStateType.Move);
                return;
            }
        }

        SpriteAnimatorData.AnimationType animType = GetAttackAnimation(m_attackTarget.DamageableGameObject.transform.position);

        if (anim.CurrentAnimationType != animType)
        {
            anim.PlayAnimation(animType, false, false);
        }

        if (!m_isDamageApplied && m_attackTimer < attacksDelay / 2f)
        {
            if (!soundSource.isPlaying)
            {
                soundSource.clip = attackClip;
                soundSource.loop = false;
                soundSource.Play();
            }
        }

        if (m_attackTimer <= 0f)
        {
            anim.PlayAnimation(animType, false, false);

            fireParticleParent.position = m_attackTarget.DamageableGameObject.transform.position;
            fireParticleParent.eulerAngles = new Vector3(0f, 0f, Random.value * 360f);

            for (int i = 0; i < fireParticles.Length; i++)
            {
                fireParticles[i].Play();
            }

            m_attackTarget.SetDamage(attackDamage, this);
            m_attackTimer = attacksDelay;
        }

        m_attackTimer -= Time.deltaTime;
    }

    protected void ExitState_Attack(UnitStateType targetState)
    {
        m_pursuer.enabled = false;
        m_currentStateType = targetState;
        m_currentStateHandler = m_states[targetState];
    }

    protected void EnterState_Die()
    {
        anim.PlayAnimation(SpriteAnimatorData.AnimationType.DiePrimary, false);
        m_currentStateHandler = State_Die;
    }

    protected void State_Die()
    {
        if (m_currentStateType != UnitStateType.Die)
        {
            ExitState_Die(m_currentStateType);
        }

        if (!anim.IsPlaying)
        {
            m_currentStateType = UnitStateType.None;
            ExitState_Die(m_currentStateType);
        }
    }

    protected void ExitState_Die(UnitStateType targetState)
    {
        if (m_currentStateHandler == null)
        {
            m_currentStateHandler = m_states[targetState];
        }

        m_currentStateType = targetState;
    }
    #endregion  // ~State Handlers
}
