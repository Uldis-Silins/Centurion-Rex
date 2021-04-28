using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UI_UnitSelectionRect : MonoBehaviour
{
    public Image selectionRect;
    public Image healthBar;

    public void SetHealthBar(float curHealth, float maxHealth)
    {
        healthBar.fillAmount = curHealth / maxHealth;
    }
}