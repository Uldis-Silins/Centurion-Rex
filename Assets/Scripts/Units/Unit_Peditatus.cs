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
    private bool m_isDamageApplied;

    private Dictionary<UnitStateType, StateHandler> m_states;
    private UnitStateType m_currentStateType;

    public bool IsSelected { get; private set; }
    public GameObject SelectableGameObject { get { return this.gameObject; } }
    public override float AttackDistance { get { return attackDistance; } }

    protected override void Awake()
    {
        base.Awake();

        m_startColor = soldierRenderer.material.GetColor(m_colorPropID);

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
        Debug.Log("SetState: " + type.ToString());
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
        anim.PlayAnimation(GetIdleAnimation());
        m_currentStateHandler = State_Idle;
        Debug.Log("EnterState_Idle");
    }

    protected void State_Idle()
    {
        if (m_currentStateType != UnitStateType.Idle)
        {
            ExitState_Idle(m_currentStateType);
        }
    }

    protected void ExitState_Idle(UnitStateType targetState)
    {
        m_currentStateType = targetState;
        m_currentStateHandler = m_states[targetState];
    }

    protected void EnterState_Move()
    {
        Debug.Log("EnterState_Move");
        Debug.Assert(m_hasMoveTarget, "MoveState: No move target set.");
        //m_avoider.enabled = true;
        //m_obstacleAvoider.enabled = true;
        m_pursuer.enabled = false;

        anim.PlayAnimation(GetMoveAnimation());

        m_currentStateHandler = State_Move;
    }

    protected void State_Move()
    {
        if (m_hasMoveDirectionChanged)
        {
            Debug.Log("Move direction changed");
            anim.PlayAnimation(GetMoveAnimation());
        }

        if (m_currentStateType != UnitStateType.Move)
        {
            Debug.Log("State changed fro move to " + m_currentStateType);
            ExitState_Move(m_currentStateType);
            return;
        }

        if (!m_seeker.IsMoving)
        {
            if (HasAttackTarget)
            {
                if (Vector2.Distance(transform.position, m_attackTarget.DamageableGameObject.transform.position) <= attackDistance)
                {
                    ExitState_Move(UnitStateType.Attack);
                    return;
                }
            }

            m_hasMoveTarget = false;
            Debug.Log("Stopped moving, goto idle");
            ExitState_Move(UnitStateType.Idle);
            return;
        }
    }

    protected void ExitState_Move(UnitStateType targetState)
    {
        //m_avoider.enabled = false;  // stay enabled and change wight?
        //m_obstacleAvoider.enabled = false;
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
        m_pursuer.enabled = true;
        m_pursuer.target = m_attackTarget.DamageableGameObject.GetComponent<Agent>();
        m_currentStateHandler = State_Attack;
    }

    protected void State_Attack()
    {
        if(!HasAttackTarget)
        {
            ExitState_Attack(UnitStateType.Idle);
        }

        if (m_currentStateType != UnitStateType.Attack)
        {
            ExitState_Attack(m_currentStateType);
        }

        if (Vector2.Distance(transform.position, m_attackTarget.DamageableGameObject.transform.position) > attackDistance)
        {
            m_pursuer.SetDestination((m_attackTarget.DamageableGameObject.transform.position - transform.position).normalized * attackDistance);

            if (m_hasMoveDirectionChanged)
            {
                anim.PlayAnimation(GetMoveAnimation());
            }
        }

        SpriteAnimatorData.AnimationType animType = GetAttackAnimation(m_attackTarget.DamageableGameObject.transform.position);

        if (anim.CurrentAnimationType != animType)
        {
            anim.PlayAnimation(animType, false, false);
        }

        if (!m_isDamageApplied && m_attackTimer < attacksDelay / 2f)
        {
            m_attackTarget.SetDamage(attackDamage, gameObject);

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
            m_isDamageApplied = false;
            m_attackTimer = attacksDelay;
        }

        m_attackTimer -= Time.deltaTime;
    }

    protected void ExitState_Attack(UnitStateType targetState)
    {
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
