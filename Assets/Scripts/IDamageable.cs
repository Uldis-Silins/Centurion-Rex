using System.Collections.Generic;
using System;
using UnityEngine;

public interface IDamageable
{
    event Action<IDamageable> onKilled;

    void SetDamage(float damage, GameObject attacker);
    void Kill();

    float CurrentHealth { get; }
    GameObject DamageableGameObject { get; }
    FactionType Faction { get; }
}