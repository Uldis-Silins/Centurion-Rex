using System.Collections;
using UnityEngine;

public class AvoidObstacles : Seek
{
    public float avoidDistance = 1f;
    public float lookAhead = 2.5f;
    public LayerMask avoidanceLayers;

    public override void Awake()
    {
        base.Awake();
    }

    public override Steering GetSteering()
    {
        Steering steering = new Steering();
        Vector2 position = transform.position;
        Vector2 rayVector = agent.velocity.normalized * lookAhead;
        Vector2 direction = rayVector;
        RaycastHit2D hit = Physics2D.Raycast(position, direction, lookAhead, avoidanceLayers);

        if (hit)
        {
            position = hit.point + hit.normal * avoidDistance;
            m_moveTarget = position;
            steering = base.GetSteering();
        }

        return steering;
    }
}