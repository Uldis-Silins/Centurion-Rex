using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class Player_Controller : MonoBehaviour
{
    public bool ownedByPlayer;
    public FactionType enemyFaction;

    public UI_GameManager uiManager;
    public UI_HudManager hudManager;
    public List<Building> spawnedBuildings;

    public LayerMask buildingLayer;

    [SerializeField] private SelectableManager selectableManager;
    [SerializeField] private DamageableManager damageableManager;

    public static GameState currentGameState;

    private List<Unit_Base> m_ownedUnits;

    private Stack<IDamageable> m_waitingForKill;

    private Camera m_mainCam;

    public List<Unit_Base> OwnedUnits { get { return m_ownedUnits; } }

    public int CurrentPopulation { get { return m_ownedUnits.Count; } }

    private void Awake()
    {
        m_ownedUnits = new List<Unit_Base>();

        if(ownedByPlayer)
        {
            NavMesh.avoidancePredictionTime = 0.5f;
        }

        m_waitingForKill = new Stack<IDamageable>();
        m_mainCam = Camera.main;

    }

    private void OnEnable()
    {
        Building_Health.onBuildingDestroyed += HandleBuildingDestroyed;
    }

    private void OnDisable()
    {
        Building_Health.onBuildingDestroyed -= HandleBuildingDestroyed;
    }

    private void Start()
    {
        currentGameState = GameState.Playing;
    }

    private void Update()
    {
        if (ownedByPlayer && Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentGameState == GameState.Playing)
            {
                hudManager.ChangeCursor(UI_HudManager.CursorType.None);
                uiManager.pauseMenu.gameObject.SetActive(true);
                currentGameState = GameState.Pause;
                Time.timeScale = 0f;
            }
            else if(currentGameState == GameState.Pause)
            {
                hudManager.ChangeCursor(UI_HudManager.CursorType.Default);
                uiManager.pauseMenu.gameObject.SetActive(false);
                currentGameState = GameState.Playing;
                Time.timeScale = 1f;
            }
        }

        while (m_waitingForKill.Count > 0)
        {
            IDamageable kill = m_waitingForKill.Pop();
            
            GameObject killedObj = damageableManager.GetObject(kill);
            damageableManager.UnregisterDamageable(kill);
            kill.onKilled -= HandleUnitKilled;
            Destroy(killedObj);
        }

        if (ownedByPlayer && Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            Ray ray = m_mainCam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, buildingLayer))
            {
                GameObject selectable = hit.collider.gameObject;

                if (selectable.GetComponent<ISelecteble>() != null)
                {
                    for (int i = 0; i < spawnedBuildings.Count; i++)
                    {
                        if (spawnedBuildings[i].selecteble == selectable)
                        {
                            uiManager.ShowBuildingMenu(spawnedBuildings[i].type);
                        }
                    }
                }
            }
            else
            {
                uiManager.CloseBuildingMenu();
            }
        }
    }

    public void AddToOwnedUnits(Unit_Base unit)
    {
        m_ownedUnits.Add(unit);

        var health = unit.GetComponent<Unit_Health>();

        unit.agent.avoidancePriority = m_ownedUnits.Count * 5;

        if (health != null)
        {
            health.onKilled += HandleUnitKilled;
            damageableManager.RegisterDamageable(health, unit.gameObject);
        }

        if(ownedByPlayer && selectableManager != null)
        {
            selectableManager.RegisterSelectable(unit.GetComponent<ISelecteble>(), unit.gameObject);
        }
    }

    private void HandleUnitKilled(IDamageable damageable)
    {
        GameObject killedObj = damageableManager.GetObject(damageable);

        for (int i = m_ownedUnits.Count - 1; i >= 0; i--)
        {
            if(m_ownedUnits[i].gameObject == killedObj)
            {
                if(selectableManager != null)
                {
                    selectableManager.UnregisterSelectable(killedObj);
                }

                m_waitingForKill.Push(damageable);

                m_ownedUnits.RemoveAt(i);
                break;
            }
        }
    }

    private void HandleBuildingDestroyed(Building_Health health)
    {
        if(ownedByPlayer && currentGameState == GameState.Playing && health.buildingType == BuildingType.Base)
        {
            if(health.owningFaction == FactionType.Enemy)
            {
                uiManager.ShowWinMenu();
                currentGameState = GameState.Win;
            }
            else if(health.owningFaction == FactionType.Player)
            {
                uiManager.ShowGameOverMenu();
                currentGameState = GameState.GameOver;
            }
        }
    }

    [System.Serializable]
    public class Building
    {
        public GameObject selecteble;
        public BuildingType type;
    }
}

public enum FactionType { None, Player, Enemy }
public enum BuildingType { None, Base, UnitSpawn, ResourceProduction }
public enum GameState { None, Playing, Pause, Win, GameOver }