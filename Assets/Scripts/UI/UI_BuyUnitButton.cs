using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UI_BuyUnitButton : MonoBehaviour, IPointerClickHandler
{
    public Button button;
    public Building_UnitSpawner barracks;
    public UnitData unit;
    public Image progressBarFill;

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
