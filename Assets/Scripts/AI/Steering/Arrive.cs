using System.Collections;
using UnityEngine;

public class Arrive : AgentBehaviour
{
    public float targetRadius;
    public float slowRadius;
    public float timeToTarget = 0.1f;
    public FlowField flowField;
    public Vector3 gridWorldOffset;

    public bool IsMoving { get { return (m_moveTarget - m_position).magnitude >= targetRadius; } }

    public override Steering GetSteering()
    {
        Steering steering = new Steering();
        Vector2 direction = m_moveTarget - m_position;    // Replaced by flow field

        if (flowField.cells != null)
        {
            FlowField.Cell curCell = flowField.GetCell(transform.position + gridWorldOffset);
            Vector2 cellDirection = new Vector2(curCell.bestDirection.vector.x, curCell.bestDirection.vector.y);
            direction = Vector2.Lerp(cellDirection, direction, RemainingDistance / slowRadius);
        }

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

        Vector2 desiredVelocity = direction;
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