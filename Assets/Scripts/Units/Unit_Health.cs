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
    private readonly float m_damageFlashTime = 0.25f;

    [SerializeField] private float m_damageableRadius;

    private Unit_Base m_lastAttacker;

    public float CurrentHealth { get; protected set; }
    public GameObject DamageableGameObject { get { return this.gameObject; } }
    public FactionType Faction { get { return owningFaction; } }
    public float DamageableRadius { get { return m_damageableRadius; } }

    /// <summary>
    /// SideFX auto clear
    /// </summary>
    public Unit_Base Attacker
    {
        get
        {
            Unit_Base attacker = m_lastAttacker;
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
            m_sprite.color = Color.Lerp(Color.white, Color.red, m_damageTimer / m_damageFlashTime);
            m_damageTimer -= Time.deltaTime;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Color prevColor = Gizmos.color;
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(DamageableGameObject.transform.position, DamageableRadius);
        Gizmos.color = prevColor;
    }

    public void SetDamage(float damage, Unit_Base attacker)
    {
        CurrentHealth -= damage;

        m_damageTimer = m_damageFlashTime;
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