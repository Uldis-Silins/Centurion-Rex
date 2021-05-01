using System.Collections;
using UnityEngine;

public class AvoidObstacles : Seek
{
    public float avoidDistance = 1f;
    public float lookAhead = 2.5f;
    public LayerMask avoidanceLayers;

    public float tickRate = 0.2f;

    private float m_radius;
    private float m_timer;
    private RaycastHit2D m_hit;
    private bool m_hasHit;

    public override void Awake()
    {
        base.Awake();

        m_radius = GetComponent<CircleCollider2D>().radius;
    }

    public override void Update()
    {
        if (m_timer <= 0f)
        {
            m_timer = tickRate;

            m_hasHit = false;

            Vector2 position = transform.position;
            Vector2 direction = agent.velocity.normalized * lookAhead;

            Vector2[] offsets = new Vector2[]
            {
            direction.normalized,
            (direction +  new Vector2(direction.y, -direction.x)).normalized * 0.5f,
            (direction + new Vector2(-direction.y, direction.x)).normalized * 0.5f
            };

            for (int i = 0; i < offsets.Length; i++)
            {
                RaycastHit2D hit = Physics2D.Raycast(position, offsets[i], lookAhead * offsets[i].magnitude, avoidanceLayers);

                if (hit)
                {
                    m_hasHit = true;
                    m_hit = hit;

                    break;
                }
            }
        }

        m_timer -= Time.deltaTime;

        base.Update();
    }

    public override Steering GetSteering()
    {
        Steering steering = new Steering();
        Vector2 position = m_position;

        if (m_hasHit && m_hit)
        {
            position = m_hit.point + m_hit.normal * avoidDistance;
            //position = m_hit.point + new Vector2(m_hit.normal.y, -m_hit.normal.x) * avoidDistance;

            m_moveTarget = position;
            steering = base.GetSteering();
        }

        return steering;
    }
}