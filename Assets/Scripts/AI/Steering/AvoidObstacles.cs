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
        Vector3 position = transform.position;
        Vector3 rayVector = agent.velocity.normalized * lookAhead;
        Vector3 direction = rayVector;
        RaycastHit hit;

        if (Physics.Raycast(position, direction, out hit, lookAhead, avoidanceLayers))
        {
            position = hit.point + hit.normal * avoidDistance;
            m_moveTarget = position;
            steering = base.GetSteering();
        }
        return steering;
    }
}