using System.Collections;
using UnityEngine;

public class Separate : Flee
{
    public float radius = 0.5f;
    public float tickRate = 0.2f;
    public LayerMask unitLayers;

    private float m_timer;

    public override void Awake()
    {
        base.Awake();
    }

    public override void Update()
    {
        if(m_timer <= 0f)
        {
            m_timer = tickRate;

            Collider2D[] hits = Physics2D.OverlapCircleAll(m_position, radius, unitLayers);
            
            if(hits.Length > 1)
            {
                Vector2 closestPos = Vector2.one * float.MaxValue;
                float closestDist = float.MaxValue;

                for (int i = 0; i < hits.Length; i++)
                {
                    if (hits[i].gameObject == gameObject) continue;

                    Vector2 pos = hits[i].transform.position;
                    float dist = (pos - closestPos).sqrMagnitude;

                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestPos = pos;
                    }
                }

                m_moveTarget = closestPos;
            }
        }

        m_timer -= Time.deltaTime;

        base.Update();
    }

    public override Steering GetSteering()
    {
        return base.GetSteering();
    }
}