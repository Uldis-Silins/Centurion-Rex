using System.Collections;
using UnityEngine;

public class Separate : Flee
{
    public float checkRadius = 0.5f;
    public LayerMask unitLayers;
    public float tickRate = 0.5f;

    private float m_timer;

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
        if (m_timer <= 0f)
        {
            Vector3 dir = Vector3.zero;
            int hitCount = 0;
            Collider2D[] hits = Physics2D.OverlapCircleAll(m_position, checkRadius, unitLayers);

            if (hits.Length > 1)
            {
                for (int i = 0; i < hits.Length; i++)
                {
                    if (hits[i].gameObject == gameObject) continue;

                    dir += hits[i].transform.position - transform.position;
                    hitCount++;
                }

                if (hitCount > 0)
                {
                    dir /= hitCount;
                }
            }
            else
            {
                return new Steering();
            }

            m_moveTarget = m_position + ((Vector2)dir.normalized * checkRadius);
            m_timer = tickRate;
        }

        m_timer -= Time.deltaTime;
        Debug.DrawLine(m_position, m_moveTarget);

        return base.GetSteering();
    }
}