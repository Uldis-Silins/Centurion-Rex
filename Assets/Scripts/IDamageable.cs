using System.Collections;
using UnityEngine;

public interface IDamageable
{
    void SetDamage(float damage);
    void Kill();

    float CurrentHealth { get; }
    FactionType Faction { get; }
}