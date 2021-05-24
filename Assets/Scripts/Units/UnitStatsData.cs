using System.Collections;
using UnityEngine;

public class UnitStatsData : ScriptableObject
{
    public float maxHealth;

    [Tooltip("Base damage dealt per attack")]
    public float baseDamage;

    public float visionDistance;
    public float attackDistance;

    [Tooltip("Time of each attack in seconds")]
    public float attackTime;

    [Tooltip("Time between attacks in seconds")]
    public float attackDelay;

    public float moveSpeed;
}