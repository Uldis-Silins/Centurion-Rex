using System.Collections;
using UnityEngine;
using System;

public class Unit_Health : MonoBehaviour, IDamageable
{
    public event Action<IDamageable> onKilled = delegate { };

    [Range(1f, 100f)] public float maxHealth;
    public FactionType owningFaction;

    public GameObject ragdoll;

    [SerializeField] private SpriteRenderer m_sprite;
    private float m_damageTimer;

    private GameObject m_lastAttacker;

    public float CurrentHealth { get; protected set; }
    public GameObject DamageableGameObject { get { return this.gameObject; } }
    public FactionType Faction { get { return owningFaction; } }

    /// <summary>
    /// SideFX auto clear
    /// </summary>
    public GameObject Attacker
    {
        get
        {
            GameObject attacker = m_lastAttacker;
            m_lastAttacker = null;
            return attacker;
        }
    }

    private void Start()
    {
        CurrentHealth = maxHealth;
    }

    private void Update()
    {
        if (m_damageTimer > 0f)
        {
            m_sprite.color = Color.Lerp(Color.white, Color.red, m_damageTimer / 0.5f);
            m_damageTimer -= Time.deltaTime;
        }
    }

    public void SetDamage(float damage, GameObject attacker)
    {
        CurrentHealth -= damage;

        m_damageTimer = 0.5f;
        m_sprite.color = Color.red;

        m_lastAttacker = attacker;

        if (CurrentHealth <= 0)
        {
            Kill();
        }
    }

    public void Kill()
    {
        onKilled.Invoke(this);

        if (ragdoll != null)
        {
            Instantiate(ragdoll, transform.position, transform.rotation);
        }
    }
}