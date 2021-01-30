using System.Collections;
using UnityEngine;
using System;

public class Building_Health : MonoBehaviour, IDamageable
{
    public static event Action<Building_Health> onBuildingDestroyed = delegate { };
    public event Action<IDamageable> onKilled = delegate { };

    public BuildingType buildingType;

    [Range(1f, 100f)] public float maxHealth;
    public FactionType owningFaction;
    public DamageableManager damageableManager;

    [SerializeField] private SpriteRenderer m_sprite;
    private float m_damageTimer;

    public float CurrentHealth { get; protected set; }
    public FactionType Faction { get { return owningFaction; } }

    private void Start()
    {
        CurrentHealth = maxHealth;
        damageableManager.RegisterDamageable(this, gameObject);
    }

    private void Update()
    {
        if(m_damageTimer > 0f)
        {
            m_sprite.color = Color.Lerp(Color.white, Color.red, m_damageTimer / 0.5f);
            m_damageTimer -= Time.deltaTime;
        }
    }

    public void SetDamage(float damage)
    {
        CurrentHealth -= damage;

        m_damageTimer = 0.5f;
        m_sprite.color = Color.red;

        if (CurrentHealth < 0)
        {
            Kill();
        }
    }

    public void Kill()
    {
        onBuildingDestroyed.Invoke(this);
        onKilled.Invoke(this);
        damageableManager.UnregisterDamageable(this);
        Destroy(gameObject);
    }
}