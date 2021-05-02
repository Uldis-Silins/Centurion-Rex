using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_GameManager : MonoBehaviour
{
    [System.Serializable]
    public class BuildingPanel
    {
        public RectTransform containerPanel;
        public BuildingType buildingType;
    }

    public UI_HudManager hudManager;
    public BuildingPanel[] buildingPanels;
    public RectTransform pauseMenu;
    public RectTransform winMenu;
    public RectTransform gameOverMenu;
    public RectTransform resourcePanel;

    public Button[] buyUnitsButtons;
    public Image[] buyUnitFill;
    public Text[] buyUnitButtonTexts;

    public Image[] buildQueue;

    public Text wineAmountText;

    private BuildingPanel m_curBuildingPanel;

    private int[] m_unitQueueAmounts;

    private void Start()
    {
        for (int i = 0; i < buildingPanels.Length; i++)
        {
            buildingPanels[i].containerPanel.gameObject.SetActive(false);
        }

        pauseMenu.gameObject.SetActive(false);
        winMenu.gameObject.SetActive(false);
        gameOverMenu.gameObject.SetActive(false);

        m_unitQueueAmounts = new int[buyUnitButtonTexts.Length];

        for (int i = 0; i < buildQueue.Length; i++)
        {
            buildQueue[i].enabled = false;
        }
    }

    public void CloseBuildingMenu()
    {
        if(m_curBuildingPanel != null)
        {
            m_curBuildingPanel.containerPanel.gameObject.SetActive(false);
        }
    }

    public void ShowBuildingMenu(BuildingType type)
    {
        for (int i = 0; i < buildingPanels.Length; i++)
        {
            if(buildingPanels[i].buildingType == type)
            {
                m_curBuildingPanel = buildingPanels[i];
                m_curBuildingPanel.containerPanel.gameObject.SetActive(true);
                break;
            }
        }
    }

    public void ShowGameOverMenu()
    {
        gameOverMenu.gameObject.SetActive(true);
        hudManager.ChangeCursor(UI_HudManager.CursorType.None);
    }

    public void ShowWinMenu()
    {
        winMenu.gameObject.SetActive(true);
        hudManager.ChangeCursor(UI_HudManager.CursorType.None);
    }

    public void OnRestartClick()
    {
        Player_Controller.currentGameState = GameState.Playing;
        Time.timeScale = 1f;
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        UnityEngine.SceneManagement.SceneManager.LoadScene(currentSceneName);
    }

    public void AddUnitToQueue(int index)
    {
        m_unitQueueAmounts[index]++;
        buyUnitButtonTexts[index].text = m_unitQueueAmounts[index].ToString();
    }

    public void RemoveUnitToQueue(int index)
    {
        m_unitQueueAmounts[index]--;
        buyUnitButtonTexts[index].text = m_unitQueueAmounts[index] == 0 ? " " : m_unitQueueAmounts[index].ToString();
    }

    public void UpdateBuyUnitFill(int buttonIndex, float amount)
    {
        buyUnitFill[buttonIndex].fillAmount = amount;
    }

    public void SetBuildQueue(List<UnitData> units)
    {
        for (int i = 0; i < buildQueue.Length; i++)
        {
            buildQueue[i].enabled = false;
        }

        int num = units.Count > buildQueue.Length ? buildQueue.Length : units.Count;

        for (int i = 0; i < num; i++)
        {
            buildQueue[i].sprite = units[i].uiSprite;
            buildQueue[i].enabled = true;
        }
    }

    public void OnQuitClick()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
      Application.Quit();
#endif
    }
}