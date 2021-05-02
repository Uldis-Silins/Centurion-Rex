using System.Collections.Generic;
using System;
using UnityEngine;

public interface IDamageable
{
    event Action<IDamageable> onKilled;

    void SetDamage(float damage, Unit_Base attacker);
    void Kill();

    float CurrentHealth { get; }
    GameObject DamageableGameObject { get; }
    FactionType Faction { get; }
    float DamageableRadius { get; }
}