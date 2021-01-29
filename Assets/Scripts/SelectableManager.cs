using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class SelectableManager : MonoBehaviour
{
    [SerializeField] private Camera m_selectionCamera;

    private List<ISelecteble> m_registeredSelectables;
    private List<GameObject> m_registeredObjects;   // GameObject mirror of list<selectable>

    private static SelectableManager m_instance;

    //public static SelectableManager Instance
    //{
    //    get
    //    {
    //        return m_instance;
    //    }
    //}

    private void Awake()
    {
        if(m_instance != null)
        {
            Debug.LogError("Only one instance of SelectableManager allowed; Destroying..");
            Destroy(m_instance);
        }

        m_instance = this;

        m_registeredSelectables = new List<ISelecteble>();
        m_registeredObjects = new List<GameObject>();
    }

    public void RegisterSelectable(ISelecteble selectable, GameObject selectableObject)
    {
        if(!m_registeredSelectables.Contains(selectable))
        {
            Assert.IsFalse(m_registeredObjects.Contains(selectableObject));

            m_registeredSelectables.Add(selectable);
            m_registeredObjects.Add(selectableObject);
        }
        else
        {
            throw new System.ArgumentException("m_registeredSelectables already contains " + selectable);
        }

        Assert.AreEqual(m_registeredObjects.Count, m_registeredSelectables.Count);
    }

    public void UnregisterSelectable(ISelecteble selectable)
    {
        if(m_registeredSelectables.Contains(selectable))
        {
            int index = m_registeredSelectables.IndexOf(selectable);
            m_registeredSelectables.Remove(selectable);
            m_registeredObjects.RemoveAt(index);
        }
        else
        {
            throw new System.ArgumentException("m_registeredSelectables does not contain " + selectable);
        }

        Assert.AreEqual(m_registeredObjects.Count, m_registeredSelectables.Count);
    }

    public void UnregisterSelectable(GameObject selectable)
    {
        if (m_registeredObjects.Contains(selectable))
        {
            int index = m_registeredObjects.IndexOf(selectable);
            m_registeredObjects.Remove(selectable);
            m_registeredSelectables.RemoveAt(index);
        }
        else
        {
            throw new System.ArgumentException("m_registeredObjects does not contain " + selectable);
        }

        Assert.AreEqual(m_registeredObjects.Count, m_registeredSelectables.Count);
    }

    public Vector2[] GetAllScreenPositions()
    {
        Vector2[] positions = new Vector2[m_registeredObjects.Count];

        for (int i = 0; i < m_registeredObjects.Count; i++)
        {
            positions[i] = m_selectionCamera.WorldToScreenPoint(m_registeredObjects[i].transform.position);
        }

        return positions;
    }

    public ISelecteble GetSelectableAt(int index)
    {
        return m_registeredSelectables[index];
    }

    public GameObject GetObjectAt(int index)
    {
        return m_registeredObjects[index];
    }

    public IEnumerable<GameObject> GetCurrentSelectedObjects()
    {
        Stack<GameObject> selected = new Stack<GameObject>();

        for (int i = 0; i < m_registeredSelectables.Count; i++)
        {
            if(m_registeredSelectables[i].IsSelected)
            {
                selected.Push(GetObjectAt(i));
            }
        }

        return selected;
    }
}
