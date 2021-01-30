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

    public IEnumerable<KeyValuePair<GameObject, IDamageable> > GetAtPosition(Vector3 position, float radius, FactionType faction)
    {
        List<KeyValuePair<GameObject, IDamageable> > hits = new List<KeyValuePair<GameObject, IDamageable>>();

        for (int i = 0; i < m_registeredObjects.Count; i++)
        {
            position.y = m_registeredObjects[i].transform.position.y;

            if(m_registeredDamageables[i].Faction == faction && Vector3.Distance(position, m_registeredObjects[i].transform.position) <= radius)
            {
                hits.Add(new KeyValuePair<GameObject, IDamageable>(m_registeredObjects[i], m_registeredDamageables[i]));
            }
        }

        hits.Sort(new DistanceComparer(position));

        return hits;
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

    public GameObject GetObject(IDamageable damageable)
    {
        if(m_registeredDamageables.Contains(damageable))
        {
            int index = m_registeredDamageables.IndexOf(damageable);
            return m_registeredObjects[index];
        }

        return null;
    }

    public class DistanceComparer : IComparer<KeyValuePair<GameObject, IDamageable>>
    {
        private Vector3 m_pos;

        public DistanceComparer(Vector3 pos)
        {
            m_pos = pos;
        }

        public int Compare(KeyValuePair<GameObject, IDamageable> x, KeyValuePair<GameObject, IDamageable> y)
        {
            m_pos.y = x.Key.transform.position.y;

            if (Vector3.Distance(x.Key.transform.position, m_pos) < Vector3.Distance(y.Key.transform.position, m_pos))
            {
                return -1;
            }
            else if (Vector3.Distance(x.Key.transform.position, m_pos) > Vector3.Distance(y.Key.transform.position, m_pos))
            {
                return 1;
            }

            return 0;
        }
    }
}