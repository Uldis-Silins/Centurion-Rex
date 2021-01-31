using System.Collections;
using UnityEngine;

public class Arrive : AgentBehaviour
{
    public float targetRadius;
    public float slowRadius;
    public float timeToTarget = 0.1f;

    public bool IsMoving { get { return (m_moveTarget - transform.position).magnitude >= slowRadius; } }

    public override Steering GetSteering()
    {
        Steering steering = new Steering();
        Vector3 direction = m_moveTarget - transform.position;
        float distance = direction.magnitude;
        float targetSpeed;

        if (distance < targetRadius)
        {
            return steering;
        }
        if (distance > slowRadius)
        {
            targetSpeed = agent.maxSpeed;
        }
        else
        {
            targetSpeed = agent.maxSpeed * distance / slowRadius;
        }

        Vector3 desiredVelocity = direction;
        desiredVelocity.Normalize();
        desiredVelocity *= targetSpeed;
        steering.linear = desiredVelocity - agent.velocity;
        steering.linear /= timeToTarget;

        if (steering.linear.magnitude > agent.maxAccel)
        {
            steering.linear.Normalize();
            steering.linear *= agent.maxAccel;
        }

        return steering;
    }
}