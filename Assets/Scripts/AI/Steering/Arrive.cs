using System.Collections;
using UnityEngine;

public class Arrive : AgentBehaviour
{
    public float targetRadius;
    public float slowRadius;
    public float cellRadius;
    public float timeToTarget = 0.1f;
    public FlowField flowField;
    public Vector3 gridWorldOffset;

    public bool IsMoving { get; private set; }

    public override Steering GetSteering()
    {
        Steering steering = new Steering();
        Vector2 direction = m_moveTarget - m_position;    // Replaced by flow field

        if (flowField != null && flowField.cells != null)
        {
            FlowField.Cell curCell = flowField.GetCell(transform.position + gridWorldOffset);
            Vector2 cellDirection = new Vector2(curCell.bestDirection.vector.x, curCell.bestDirection.vector.y);
            direction = Vector2.Lerp(direction, cellDirection, RemainingDistance / (cellRadius * 2));
        }

        float distance = direction.magnitude;
        float targetSpeed;

        IsMoving = true;

        if (distance < targetRadius)
        {
            IsMoving = false;
            return steering;
        }
        else if (distance > slowRadius)
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