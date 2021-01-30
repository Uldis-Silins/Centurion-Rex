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

    private BuildingPanel m_curBuildingPanel;

    private void Start()
    {
        for (int i = 0; i < buildingPanels.Length; i++)
        {
            buildingPanels[i].containerPanel.gameObject.SetActive(false);
        }

        pauseMenu.gameObject.SetActive(false);
        winMenu.gameObject.SetActive(false);
        gameOverMenu.gameObject.SetActive(false);
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
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        UnityEngine.SceneManagement.SceneManager.LoadScene(currentSceneName);
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