using System.Collections.Generic;
using UnityEngine;

public class Player_Controller : MonoBehaviour
{
    [SerializeField] private SelectableManager selectableManager;

    private List<Unit_Base> m_ownedUnits;

    private void Awake()
    {
        m_ownedUnits = new List<Unit_Base>();
    }

    public void AddToOwnedUnits(Unit_Base unit)
    {
        m_ownedUnits.Add(unit);

        if(selectableManager != null)
        {
            selectableManager.RegisterSelectable(unit.GetComponent<ISelecteble>(), unit.gameObject);
        }
    }
}

public enum FactionType { None, Player, Enemy }