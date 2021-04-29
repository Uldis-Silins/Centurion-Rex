using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Unit_Base : MonoBehaviour
{
    public enum UnitStateType { None, Idle, Move, Attack, Die }

    protected delegate void StateHandler();

    public SpriteAnimator anim;
    public Unit_Health health;

    public UnitData.UnitType unitType;

    public SpriteRenderer soldierRenderer;
    public CircleCollider2D circleCollider;

    public AudioSource soundSource;
    public AudioClip walkClip;
    public AudioClip attackClip;

    public float visionDistance;
    public Transform fovTransform;

    [SerializeField] protected Agent m_agent;
    [SerializeField] protected Arrive m_seeker;
    [SerializeField] protected Avoid m_avoider;
    [SerializeField] protected AvoidObstacles m_obstacleAvoider;
    [SerializeField] protected Pursue m_pursuer;
    [SerializeField] protected Separate m_separator;

    private Rigidbody2D m_rigidbody;

    private NavigationController m_navigationController;

    protected Camera m_mainCam;

    protected Vector3 m_startScale;
    protected IDamageable m_attackTarget;
    protected Vector3 m_moveTarget;
    protected bool m_hasMoveTarget;

    protected StateHandler m_currentStateHandler = null;

    private Vector2Int m_prevAnimDirection;

    public bool HasAttackTarget { get { return m_attackTarget != null && m_attackTarget.CurrentHealth > 0 && m_attackTarget.DamageableGameObject != null; } }
    public virtual float AttackDistance { get; }

    public bool HasMoveTarget { get { return m_hasMoveTarget; } }
    public Vector3 MoveTarget { get { return m_moveTarget; } }

    protected virtual void Awake()
    {
        m_mainCam = Camera.main;
        m_rigidbody = circleCollider.GetComponent<Rigidbody2D>();

        m_startScale = soldierRenderer.transform.localScale;

        if (fovTransform != null)
        {
            fovTransform.localScale = new Vector3(visionDistance * 2f, 1f, visionDistance * 2f);
        }

        m_navigationController = GameObject.FindObjectOfType<NavigationController>();
    }

    protected virtual void Update()
    {
        Debug.Assert(m_currentStateHandler != null, "No state set.");
        m_currentStateHandler();
    }

    protected virtual void LateUpdate()
    {
        // billboard
        //soldierRenderer.transform.rotation = m_mainCam.transform.rotation;
        if (m_agent.velocity.sqrMagnitude > 0.1f)
        {
            anim.PlayAnimation(GetMoveAnimation());
        }
        else
        {
            anim.PlayAnimation(GetIdleAnimation());
        }
    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up, GetMoveDirection().ToString());
#endif
    }

    private void OnDrawGizmosSelected()
    {
        Color prevColor = Gizmos.color;

        Gizmos.color = Color.red;

        if (HasAttackTarget)
        {
            Gizmos.DrawLine(transform.position, m_attackTarget.DamageableGameObject.transform.position);
        }

        Gizmos.DrawWireSphere(transform.position, AttackDistance);

        Gizmos.color = Color.green;

        if (HasMoveTarget)
        {
            Gizmos.DrawLine(transform.position, m_moveTarget);
        }

        Gizmos.DrawWireSphere(transform.position, visionDistance);

        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(transform.position, m_avoider.collisionRadius);
        Vector2 pos = transform.position;
        Gizmos.DrawLine(transform.position, pos + m_agent.velocity * m_obstacleAvoider.lookAhead);

        Gizmos.color = prevColor;
    }

    public abstract void SetState(UnitStateType type);

    public virtual void SetAttackTarget(IDamageable target)
    {
        m_attackTarget = target;
    }

    public virtual void SetMoveTarget(Vector3 targetPosition)
    {
        const float MIN_NAVIGATION_DISTANCE = 2.5f;
        float dist = Vector2.Distance(transform.position, targetPosition);
        Collider2D hit = Physics2D.Linecast(transform.position, targetPosition, m_navigationController.obstacleLayers).collider;

        targetPosition = GetAdjustedPosition(targetPosition);

        if (dist > MIN_NAVIGATION_DISTANCE && hit != circleCollider)
        {
            Debug.Log("Requesting navigation");
            m_seeker.flowField = m_navigationController.GetFlowField(transform.position, targetPosition);
            m_seeker.gridWorldOffset = m_navigationController.gridOffset;
        }

        m_seeker.SetDestination(targetPosition);
        m_moveTarget = targetPosition;
        m_hasMoveTarget = true;
    }

    public bool CanSeeUnit(Unit_Base unit)
    {
        return Vector3.Distance(transform.position, unit.transform.position) <= visionDistance;
    }

    private Vector2Int GetMoveDirection()
    {
        Vector2Int direction = Vector2Int.zero;

        if (Vector3.Dot(Vector3.right, m_agent.velocity) < -0.5f)
        {
            direction.x = -1;
        }
        else if (Vector3.Dot(Vector3.right, m_agent.velocity) > 0.5f)
        {
            direction.x = 1;
        }

        if (Vector3.Dot(Vector3.up, m_agent.velocity) < -0.5f)
        {
            direction.y = -1;
        }
        else if (Vector3.Dot(Vector3.up, m_agent.velocity) > 0.5f)
        {
            direction.y = 1;
        }

        return direction;
    }

    protected SpriteAnimatorData.AnimationType GetMoveAnimation()
    {
        Vector2Int direction = GetMoveDirection();

        if (direction != Vector2Int.zero)
        {
            m_prevAnimDirection = direction;
        }

        if (direction.x == 0 && direction.y == 0)
        {
            return SpriteAnimatorData.AnimationType.IdleLeft;
        }

        if (direction.x == 1 && direction.y == 1)
        {
            return SpriteAnimatorData.AnimationType.WalkRightUp;
        }
        else if (direction.x == -1 && direction.y == 1)
        {
            return SpriteAnimatorData.AnimationType.WalkLeftUp;
        }
        else if (direction.x == 1 && direction.y == -1)
        {
            return SpriteAnimatorData.AnimationType.WalkRightDown;
        }
        else if (direction.x == -1 && direction.y == -1)
        {
            return SpriteAnimatorData.AnimationType.WalkLeftDown;
        }
        else if (direction.x == -1 && direction.y == 0)
        {
            return SpriteAnimatorData.AnimationType.WalkLeft;
        }
        else if (direction.x == 1 && direction.y == 0)
        {
            return SpriteAnimatorData.AnimationType.WalkRight;
        }
        else if (direction.x == 0 && direction.y == 1)
        {
            return SpriteAnimatorData.AnimationType.WalkUp;
        }
        else if (direction.x == 0 && direction.y == -1)
        {
            return SpriteAnimatorData.AnimationType.WalkDown;
        }

        throw new System.Exception("No animation for direction " + direction + " not found.");
    }

    protected SpriteAnimatorData.AnimationType GetAttackAnimation(Vector3 targetPosition)
    {
        Vector3 dir = (transform.position - targetPosition).normalized;

        if (Vector3.Dot(Vector3.right, dir) > 0)
        {
            return SpriteAnimatorData.AnimationType.AttackLeft;
        }
        else if (Vector3.Dot(Vector3.right, dir) < 0)
        {
            return SpriteAnimatorData.AnimationType.AttackRight;
        }

        throw new System.Exception("Attack animation for direction " + dir + " not found");
    }

    protected SpriteAnimatorData.AnimationType GetIdleAnimation()
    {
        if(m_prevAnimDirection.x <= 0)
        {
            return SpriteAnimatorData.AnimationType.IdleLeft;
        }
        else
        {
            return SpriteAnimatorData.AnimationType.IdleRight;
        }
    }

    private Vector2 GetAdjustedPosition(Vector3 worldPosition, float distance = 0f)
    {
        float checkRadius = circleCollider.radius;
        Vector2 adjustedPosition = worldPosition + (worldPosition - transform.position).normalized * distance;

        LayerMask mask = LayerMask.GetMask("Unit", "Obstacle", "Building");

        Collider2D[] hits = Physics2D.OverlapCircleAll(adjustedPosition, checkRadius, mask);

        if (hits.Length == 0)
        {
            return adjustedPosition;
        }

        List<Vector2> circlePositions = GetPositionListCircle(adjustedPosition, checkRadius * 3.0f, 12);

        for (int i = 0; i < circlePositions.Count; i++)
        {
            if((hits = Physics2D.OverlapCircleAll(circlePositions[i], checkRadius, mask)).Length == 0)
            {
                return circlePositions[i];
            }
        }

        return adjustedPosition + new Vector2(Random.insideUnitCircle.x * hits.Length * checkRadius, Random.insideUnitCircle.y * hits.Length * checkRadius);
    }

    private List<Vector2> GetPositionListCircle(Vector2 startPos, float[] dist, int[] posCount)
    {
        List<Vector2> positions = new List<Vector2>();
        positions.Add(startPos);

        for (int i = 0; i < dist.Length; i++)
        {
            positions.AddRange(GetPositionListCircle(startPos, dist[i], posCount[i]));
        }

        return positions;
    }

    private List<Vector2> GetPositionListCircle(Vector2 startPos, float dist, int posCount)
    {
        List<Vector2> positions = new List<Vector2>();

        for (int i = 0; i < posCount; i++)
        {
            float angle = i * (360f / posCount);
            Vector2 dir = Quaternion.Euler(0f, 0f, angle) * Vector3.right;
            positions.Add(startPos + dir * dist);
        }

        return positions;
    }
}