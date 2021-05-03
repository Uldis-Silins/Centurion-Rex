using System.Collections;
using UnityEngine;

public abstract class Building_Base : MonoBehaviour, ISelecteble
{
    public SpriteRenderer spriteRenderer;
    public Building_Health health;

    [SerializeField] protected Player_Controller m_playerController;

    [SerializeField] private GameObject m_selectableObject;
    private Bounds m_selectableBounds;

    public bool IsSelected { get; protected set; }
    public GameObject SelectableGameObject { get { return m_selectableObject; } }
    public Bounds SelectableBounds { get { return m_selectableBounds; } }

    protected virtual void Awake()
    {
        m_selectableBounds = m_selectableObject.GetComponent<Collider2D>().bounds;
    }

    public void Select()
    {
        IsSelected = true;
    }

    public void Deselect()
    {
        IsSelected = false;
    }
}