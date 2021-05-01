using System.Collections;
using UnityEngine;

public class Seek : AgentBehaviour
{
    public override void Awake()
    {
        base.Awake();
    }

    public override void Update()
    {
        base.Update();
    }

    public override Steering GetSteering()
    {
        Steering steering = new Steering();
        steering.linear = MoveTarget - m_position;
        steering.linear.Normalize();
        steering.linear = steering.linear * agent.maxAccel;
        return steering;
    }
}