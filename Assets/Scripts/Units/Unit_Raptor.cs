using System.Collections.Generic;
using UnityEngine;

public class Unit_Raptor : Unit_Base, ISelecteble
{
    public float attackDistance = 15f;
    public float attackDamage = 5f;
    public float attacksDelay = 0.5f;
    public float accuracy = 1f;

    public Projectile projectilePrefab;

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

        //if (m_attackTarget.Key != null && m_attackTarget.Value != null)
        //{
        //    Vector3 targetPos = m_attackTarget.Key.transform.position;
        //    targetPos.y = transform.position.y;

        //    if (Vector3.Distance(targetPos, transform.position) > attackDistance)
        //    {
        //        anim.SetBool("attack", false);
        //        Vector3 dir = (targetPos - transform.position).normalized;
        //        m_seeker.SetDestination(targetPos + dir * attackDistance * 0.75f);
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

        //        if (!anim.GetBool("attack") && m_attackTimer < 0.15f)
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
        //            Quaternion lookRot = Quaternion.LookRotation(m_attackTarget.Key.transform.position - transform.position);
        //            Projectile instance = Instantiate(projectilePrefab, transform.position, lookRot);
        //            instance.Spawn(transform.position, m_attackTarget.Key.transform.position + new Vector3(Random.insideUnitCircle.x * accuracy, 0f, Random.insideUnitCircle.y * accuracy), attackDamage, m_attackTarget.Value, gameObject);

        //            //m_currentTarget.Value.SetDamage(attackDamage);
        //            m_attackTimer = attacksDelay;
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

    public override void SetAttackTarget(IDamageable target, GameObject obj)
    {
        base.SetAttackTarget(target, obj);

        m_attackTimer = attacksDelay;
    }

    #region State Handlers
    protected void EnterState_Idle()
    {
        anim.PlayAnimation(GetIdleAnimation());
        m_currentStateHandler = State_Idle;
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
        m_currentStateHandler = m_states[targetState];
    }

    protected void EnterState_Move()
    {
        Debug.Log("EnterState_Move");
        Debug.Assert(m_hasMoveTarget, "MoveState: No move target set.");
        m_seeker.SetDestination(m_moveTarget);
        m_avoider.enabled = true;
        m_obstacleAvoider.enabled = true;

        anim.PlayAnimation(GetMoveAnimation());

        m_currentStateHandler = State_Move;
    }

    protected void State_Move()
    {
        if (Vector3.Distance(m_seeker.MoveTarget, m_moveTarget) > m_seeker.targetRadius)
        {
            Debug.Log("Set new move pos");
            m_seeker.SetDestination(m_moveTarget);
        }

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

        if (!m_hasMoveTarget || !m_seeker.IsMoving)
        {
            Debug.Log("Stopped moving, goto idle");
            ExitState_Move(UnitStateType.Idle);
            return;
        }
    }

    protected void ExitState_Move(UnitStateType targetState)
    {
        m_avoider.enabled = false;  // stay enabled and change wight?
        m_obstacleAvoider.enabled = false;
        m_hasMoveTarget = false;
        m_seeker.Stop();

        m_currentStateType = targetState;
        m_currentStateHandler = m_states[targetState];
    }

    protected void EnterState_Attack()
    {
        anim.PlayAnimation(GetAttackAnimation(m_attackTarget.Key.transform.position));
        m_attackTimer = attacksDelay;
        m_isDamageApplied = false;
        m_currentStateHandler = State_Attack;
    }

    protected void State_Attack()
    {
        if (m_currentStateType != UnitStateType.Attack)
        {
            ExitState_Attack(m_currentStateType);
        }

        SpriteAnimatorData.AnimationType animType = GetAttackAnimation(m_attackTarget.Key.transform.position);

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

            Quaternion lookRot = Quaternion.LookRotation(m_attackTarget.Key.transform.position - transform.position);
            Projectile instance = Instantiate(projectilePrefab, transform.position, lookRot);
            instance.Spawn(transform.position, m_attackTarget.Key.transform.position + new Vector3(Random.insideUnitCircle.x * accuracy, 0f, Random.insideUnitCircle.y * accuracy), attackDamage, m_attackTarget.Value, gameObject);

            m_attackTimer = attacksDelay;
        }

        m_attackTimer -= Time.deltaTime;
    }

    protected void ExitState_Attack(UnitStateType targetState)
    {
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
