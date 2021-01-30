using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class DamageableManager : MonoBehaviour
{
    private List<IDamageable> m_registeredDamageables;
    private List<GameObject> m_registeredObjects;   // GameObject mirror of list<damageable>

    private static DamageableManager m_instance;

    private void Awake()
    {
        if (m_instance != null)
        {
            Debug.LogError("Only one instance of DamageableManager allowed; Destroying..");
            Destroy(m_instance);
        }

        m_instance = this;

        m_registeredDamageables = new List<IDamageable>();
        m_registeredObjects = new List<GameObject>();
    }

    public void RegisterDamageable(IDamageable damageable, GameObject damageableObject)
    {
        if (!m_registeredDamageables.Contains(damageable))
        {
            Assert.IsFalse(m_registeredObjects.Contains(damageableObject));

            m_registeredDamageables.Add(damageable);
            m_registeredObjects.Add(damageableObject);
        }
        else
        {
            throw new System.ArgumentException("m_registeredSelectables already contains " + damageable);
        }

        Assert.AreEqual(m_registeredObjects.Count, m_registeredDamageables.Count);
    }

    public void UnregisterDamageable(IDamageable damageable)
    {
        if (m_registeredDamageables.Contains(damageable))
        {
            int index = m_registeredDamageables.IndexOf(damageable);
            m_registeredDamageables.Remove(damageable);
            m_registeredObjects.RemoveAt(index);
        }
        else
        {
            throw new System.ArgumentException("m_registeredSelectables does not contain " + damageable);
        }

        Assert.AreEqual(m_registeredObjects.Count, m_registeredDamageables.Count);
    }

    public void UnregisterDamageable(GameObject damageable)
    {
        if (m_registeredObjects.Contains(damageable))
        {
            int index = m_registeredObjects.IndexOf(damageable);
            m_registeredObjects.Remove(damageable);
            m_registeredDamageables.RemoveAt(index);
        }
        else
        {
            throw new System.ArgumentException("m_registeredObjects does not contain " + damageable);
        }

        Assert.AreEqual(m_registeredObjects.Count, m_registeredDamageables.Count);
    }

    public IDamageable GetDamageable(GameObject obj)
    {
        if(m_registeredObjects.Contains(obj))
        {
            int index = m_registeredObjects.IndexOf(obj);
            return m_registeredDamageables[index];
        }

        return null;
    }
}