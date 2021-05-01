using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class SelectableManager : MonoBehaviour
{
    [SerializeField] private Camera m_selectionCamera;

    private List<ISelecteble> m_registeredSelectables;

    private static SelectableManager m_instance;

    //public static SelectableManager Instance
    //{
    //    get
    //    {
    //        return m_instance;
    //    }
    //}

    public int CurrentSelectedCount { get { return GetCurrentSelectedObjects().Count; } }

    private void Awake()
    {
        if(m_instance != null)
        {
            Debug.LogError("Only one instance of SelectableManager allowed; Destroying..");
            Destroy(m_instance);
        }

        m_instance = this;

        m_registeredSelectables = new List<ISelecteble>();
    }

    public void RegisterSelectable(ISelecteble selectable)
    {
        if(!m_registeredSelectables.Contains(selectable))
        {
            m_registeredSelectables.Add(selectable);
        }
        else
        {
            throw new System.ArgumentException("m_registeredSelectables already contains " + selectable);
        }
    }

    public void UnregisterSelectable(ISelecteble selectable)
    {
        if(m_registeredSelectables.Contains(selectable))
        {
            int index = m_registeredSelectables.IndexOf(selectable);
            m_registeredSelectables.Remove(selectable);
        }
        else
        {
            throw new System.ArgumentException("m_registeredSelectables does not contain " + selectable);
        }
    }

    public Vector2[] GetAllScreenPositions()
    {
        Vector2[] positions = new Vector2[m_registeredSelectables.Count];

        for (int i = 0; i < m_registeredSelectables.Count; i++)
        {
            positions[i] = m_selectionCamera.WorldToScreenPoint(m_registeredSelectables[i].SelectableGameObject.transform.position);
        }

        return positions;
    }

    public ISelecteble GetSelectableAt(int index)
    {
        return m_registeredSelectables[index];
    }

    public bool IsObjectSelectable(GameObject obj)
    {
        return obj.transform.parent.GetComponent<ISelecteble>() != null;
    }

    public ISelecteble GetSelectable(GameObject obj)
    {
        for (int i = 0; i < m_registeredSelectables.Count; i++)
        {
            if((m_registeredSelectables[i] as MonoBehaviour).gameObject == obj || m_registeredSelectables[i].SelectableGameObject == obj)
            {
                return m_registeredSelectables[i];
            }
        }

        return null;
    }

    public void DeselectAll()
    {
        for (int i = 0; i < m_registeredSelectables.Count; i++)
        {
            m_registeredSelectables[i].Deselect();
        }
    }

    public List<ISelecteble> GetCurrentSelected()
    {
        List<ISelecteble> selected = new List<ISelecteble>();

        for (int i = 0; i < m_registeredSelectables.Count; i++)
        {
            if(m_registeredSelectables[i].IsSelected)
            {
                selected.Add(m_registeredSelectables[i]);
            }
        }

        return selected;
    }

    public List<GameObject> GetCurrentSelectedObjects()
    {
        List<GameObject> selected = new List<GameObject>();

        for (int i = 0; i < m_registeredSelectables.Count; i++)
        {
            if(m_registeredSelectables[i].IsSelected)
            {
                selected.Add((m_registeredSelectables[i] as MonoBehaviour).gameObject);
            }
        }

        return selected;
    }
}
