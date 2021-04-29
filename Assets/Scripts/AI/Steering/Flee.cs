using System.Collections;
using UnityEngine;

public class Flee : AgentBehaviour
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
        steering.linear = m_position - m_moveTarget;
        steering.linear.Normalize();
        steering.linear *= maxAccel;
        return steering;
    }
}