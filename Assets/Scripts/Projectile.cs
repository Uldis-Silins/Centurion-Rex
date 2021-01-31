using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    public float speed = 20f;
    public float maxScaleMod = 2f;

    public LayerMask unitLayer;

    private float m_flyTime;
    private float m_flyTimer;
    private bool m_isFlying;

    private float m_destroyTimer;
    private readonly float m_destroyTime = 1f;

    private Vector3 m_prevPosition;

    private GameObject m_owner;

    private Vector3 m_startPosition, m_targetPosition;
    private Vector3 m_defaultScale;

    private float m_damage;
    private IDamageable m_target;

    private void Update()
    {
        if(m_isFlying)
        {
            if(m_flyTimer >= m_flyTime)
            {
                if (m_target.CurrentHealth > 0)
                {
                    m_target.SetDamage(m_damage, m_owner);
                }

                m_isFlying = false;
            }

            float t = m_flyTimer / m_flyTime;
            transform.position = Vector3.Lerp(m_startPosition, m_targetPosition, t);

            if (t < 0.5f)
            {
                transform.localScale = Vector3.Lerp(m_defaultScale, m_defaultScale * maxScaleMod, t * 2);
                //spriteRenderer.transform.eulerAngles = Vector3.Lerp(Vector3.right * Random.Range(25f, 45f), Vector3.right * 90f, t * 2);
            }
            else
            {
                transform.localScale = Vector3.Lerp(m_defaultScale * maxScaleMod, m_defaultScale, t - (m_flyTime * 0.5f));
                //spriteRenderer.transform.eulerAngles = Vector3.Lerp(Vector3.right * 90f, Vector3.right * Random.Range(125f, 155f), t * 2);
            }

            m_flyTimer += Time.deltaTime;
        }
        else
        {
            m_destroyTimer -= Time.deltaTime;

            spriteRenderer.color = Color.Lerp(Color.clear, Color.white, m_destroyTimer / m_destroyTime);

            if(m_destroyTimer <= 0f)
            {
                Despawn();
            }
        }
    }

    public void Spawn(Vector3 startPosition, Vector3 targetPosition, float damage, IDamageable target, GameObject owner)
    {
        m_startPosition = startPosition;
        m_targetPosition = targetPosition;
        m_flyTime = Vector3.Distance(startPosition, targetPosition) / speed;

        if (m_flyTime <= 0f) m_flyTime = 0.1f;

        m_defaultScale = transform.localScale;

        m_destroyTimer = m_destroyTime;

        m_damage = damage;
        m_target = target;

        m_owner = owner;

        m_prevPosition = startPosition;

        m_isFlying = true;
    }

    public void Despawn()
    {
        Destroy(gameObject);
    }
}
