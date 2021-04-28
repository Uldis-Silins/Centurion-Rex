using System.Collections;
using UnityEngine;

public class AvoidObstacles : Seek
{
    public float avoidDistance = 1f;
    public float lookAhead = 2.5f;
    public LayerMask avoidanceLayers;

    private float m_radius;

    public override void Awake()
    {
        base.Awake();

        m_radius = GetComponent<CircleCollider2D>().radius;
    }

    public override void Update()
    {
        base.Update();
    }

    public override Steering GetSteering()
    {
        Steering steering = new Steering();
        Vector2 position = transform.position;
        Vector2 direction = agent.velocity.normalized * lookAhead;
        Vector2 basePos = agent.velocity.normalized * m_radius;

        Vector2[] offsets = new Vector2[]
        {
            new Vector2(basePos.y, -basePos.x),
            new Vector2(-basePos.y, basePos.x)
        };

        for (int i = 0; i < 2; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(position + offsets[i], direction, lookAhead, avoidanceLayers);
            Debug.DrawLine(position + offsets[i], position + offsets[i] + direction * lookAhead, Color.cyan);

            if (hit)
            {
                position = hit.point + hit.normal * avoidDistance;
                m_moveTarget = position;
                steering = base.GetSteering();
            }
        }

        return steering;
    }
}