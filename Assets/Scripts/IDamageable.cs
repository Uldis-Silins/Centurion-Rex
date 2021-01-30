using System.Collections.Generic;
using System;
using UnityEngine;

public interface IDamageable
{
    event Action<IDamageable> onKilled;

    void SetDamage(float damage);
    void Kill();

    float CurrentHealth { get; }
    FactionType Faction { get; }
}