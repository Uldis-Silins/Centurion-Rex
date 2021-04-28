using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UI_Helpers
{
    public static bool IsPointerOverUIElement()
    {
        return IsPointerOverUIElement(GetMouseHits());
    }

    public static bool IsPointerOverCanvasElement(Canvas canvas)
    {
        RectTransform[] hits;

        if (GetPointerOverUIElements(GetMouseHits(), out hits))
        {
            for (int i = 0; i < hits.Length; i++)
            {
                if(hits[i].IsChildOf(canvas.transform))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool IsPointerOverUIElement(List<RaycastResult> raycastHits)
    {
        for (int i = 0; i < raycastHits.Count; i++)
        {
            RaycastResult curRaysastResult = raycastHits[i];

            if (curRaysastResult.gameObject.layer == LayerMask.NameToLayer("UI"))
            {
                return true;
            }
        }

        return false;
    }

    private static bool GetPointerOverUIElements(List<RaycastResult> raycastHits, out RectTransform[] transforms)
    {
        transforms = new RectTransform[raycastHits.Count];

        for (int i = 0; i < raycastHits.Count; i++)
        {
            RaycastResult curRaysastResult = raycastHits[i];

            if (curRaysastResult.gameObject.layer == LayerMask.NameToLayer("UI"))
            {
                transforms[i] = curRaysastResult.gameObject.transform as RectTransform;
                return true;
            }
        }

        return false;
    }

    private static List<RaycastResult> GetMouseHits()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raysastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raysastResults);
        return raysastResults;
    }
}