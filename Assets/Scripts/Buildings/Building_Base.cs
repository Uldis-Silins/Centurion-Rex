using System.Collections;
using UnityEngine;

public class Building_Base : MonoBehaviour, ISelecteble
{
    public SpriteRenderer spriteRenderer;
    public Building_Health health;

    [SerializeField] protected Player_Controller m_playerController;

    [SerializeField] private GameObject m_selectableObject;

    public bool IsSelected { get; protected set; }
    public GameObject SelectableGameObject { get { return m_selectableObject; } }

    public void Select()
    {
        IsSelected = true;
    }

    public void Deselect()
    {
        IsSelected = false;
    }
}