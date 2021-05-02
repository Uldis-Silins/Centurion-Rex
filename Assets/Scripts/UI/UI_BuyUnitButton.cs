using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_BuyUnitButton : MonoBehaviour, IPointerClickHandler
{
    public Building_UnitSpawner barracks;
    public UnitData unit;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            barracks.OnSpawnUnitClick(unit);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            barracks.OnCancelBuildClick(unit);
        }
    }
}
