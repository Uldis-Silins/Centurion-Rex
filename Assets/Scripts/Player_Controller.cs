using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;
using UnityEngine.Events;

public class Player_Controller : MonoBehaviour
{
    public UnityAction<Unit_Base> onOwnedUnitAdded = delegate { };
    public UnityAction<Unit_Base> onOwnedUnitRemoved = delegate { };

    public bool ownedByPlayer;
    public OwnerType ownerFaction;
    public OwnerType enemyFaction;

    public UI_GameManager uiManager;
    public UI_HudManager hudManager;
    public List<Building> ownedBuildings;

    public Tilemap walkableTilemap;
    public Renderer levelPlaneRenderer;
    public NavigationController navigation;

    public LayerMask buildingLayer;
    public LayerMask selectableLayer;

    [SerializeField] private SelectableManager selectableManager;
    [SerializeField] private DamageableManager damageableManager;

    public static GameState currentGameState;

    public float currentResources;
    public UnitData[] unitData;

    private List<Unit_Base> m_ownedUnits;
    private GridHashList2D m_unitPositionList;
    private Dictionary<GridHashList2D.Node, Unit_Base> m_unitsByPosition;

    private Dictionary<UnitData.UnitType, List<Unit_Base>> m_unitsByType;

    private Stack<IDamageable> m_waitingForKill;

    private Camera m_mainCam;

    private ISelecteble m_selectedBuilding;

    public List<Unit_Base> OwnedUnits { get { return m_ownedUnits; } }
    public GridHashList2D UnitPositions { get { return m_unitPositionList; } }
    public Dictionary<GridHashList2D.Node, Unit_Base> UnitsByPosition { get { return m_unitsByPosition; } }

    public int CurrentPopulation { get { return m_ownedUnits.Count; } }

    float enemyIncomeRate = 3f;
    float enemyBonusIncome = 1;
    float enemyNextIncomeTime = 0;

    public bool HasBuildingSelected { get { return m_selectedBuilding != null; } }
    public bool BlockBuildingInteraction { get; set; }

    private void Awake()
    {
        m_ownedUnits = new List<Unit_Base>();

        for (int i = 0; i < ownedBuildings.Count; i++)
        {
            if(ownedBuildings[i].selectable == null)
            {
                ownedBuildings[i].selectable = ownedBuildings[i].gameObject.GetComponent<ISelecteble>();
            }
        }

        m_waitingForKill = new Stack<IDamageable>();
        m_mainCam = Camera.main;

        //Vector3 min = walkableTilemap.CellToWorld(walkableTilemap.cellBounds.min);
        //Vector3 max = walkableTilemap.CellToWorld(walkableTilemap.cellBounds.max);
        //m_unitPositionList = new GridHashList2D(new Rect(min.x, min.y, max.x - min.x, max.y - min.y), new Vector2Int(walkableTilemap.cellBounds.size.x, walkableTilemap.cellBounds.size.y));
        m_unitPositionList = new GridHashList2D(new Rect(levelPlaneRenderer.bounds.min.x, levelPlaneRenderer.bounds.min.y, levelPlaneRenderer.bounds.size.x, levelPlaneRenderer.bounds.size.y), navigation.manualGridSize);
        m_unitsByPosition = new Dictionary<GridHashList2D.Node, Unit_Base>();
        m_unitsByType = new Dictionary<UnitData.UnitType, List<Unit_Base>>();
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
        if (ownedByPlayer)
        {
            currentGameState = GameState.Playing;
            uiManager.wineAmountText.text = currentResources.ToString();
        }

        for (int i = 0; i < ownedBuildings.Count; i++)
        {
            if (ownedBuildings[i].selectable != null)
            {
                selectableManager.RegisterSelectable(ownedBuildings[i].selectable);
            }
        }
    }

    public void PauseGame()
    {
        if (currentGameState == GameState.Playing)
        {
            hudManager.ChangeCursor(UI_HudManager.CursorType.None);
            uiManager.pauseMenu.gameObject.SetActive(true);
            currentGameState = GameState.Pause;
            Time.timeScale = 0f;
        }
        else if (currentGameState == GameState.Pause)
        {
            hudManager.ChangeCursor(UI_HudManager.CursorType.Default);
            uiManager.pauseMenu.gameObject.SetActive(false);
            currentGameState = GameState.Playing;
            Time.timeScale = 1f;
        }
    }

    private void Update()
    {
        if (!ownedByPlayer && Time.time > enemyNextIncomeTime)
        {
            currentResources += enemyBonusIncome;
            enemyNextIncomeTime += enemyIncomeRate;
        }

        if (ownedByPlayer && Input.GetKeyDown(KeyCode.Escape))
        {
            PauseGame();
        }

        while (m_waitingForKill.Count > 0)
        {
            IDamageable kill = m_waitingForKill.Pop();
            
            GameObject killedObj = damageableManager.GetObject(kill);
            damageableManager.UnregisterDamageable(kill);
            kill.onKilled -= HandleUnitKilled;
            Destroy(killedObj);
        }

        if (ownedByPlayer && Input.GetMouseButtonDown(0) && !UI_Helpers.IsPointerOverUIElement() && !BlockBuildingInteraction)
        {
            RaycastHit2D hit = Physics2D.Raycast(m_mainCam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, Mathf.Infinity, selectableLayer);

            if (hit.collider != null && (buildingLayer & (1 << hit.collider.transform.parent.gameObject.layer)) != 0)
            {
                ISelecteble selectable = selectableManager.GetSelectable(hit.collider.gameObject);

                if (selectable != null)
                {
                    for (int i = 0; i < ownedBuildings.Count; i++)
                    {
                        if (ownedBuildings[i].selectable == selectable)
                        {
                            m_selectedBuilding = selectable;
                            selectable.Select();
                            uiManager.ShowBuildingMenu(ownedBuildings[i].type);

                            if(ownedBuildings[i].type == BuildingType.UnitSpawn)
                            {
                                for (int j = 0; j < uiManager.buyUnitsButtons.Length; j++)
                                {
                                    uiManager.buyUnitsButtons[j].button.interactable = uiManager.buyUnitsButtons[j].unit.price <= currentResources;
                                    uiManager.buyUnitsButtons[j].progressBarFill.color = uiManager.buyUnitsButtons[j].unit.price <= currentResources ? Color.white : Color.gray;
                                }
                            }

                            break;
                        }
                    }

                    if ((selectable as Building_Base).spriteRenderer.enabled)
                    {
                        hudManager.SelectBuilding(selectable);
                    }
                }
            }
            else
            {
                if (m_selectedBuilding != null)
                {
                    m_selectedBuilding.Deselect();
                    uiManager.CloseBuildingMenu();
                }

                hudManager.SelectBuilding(null);
            }
        }

        foreach (var node in m_unitsByPosition)
        {
            node.Key.position = m_unitsByPosition[node.Key].transform.position;
            m_unitPositionList.Update(node.Key);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Color prevColor = Gizmos.color;
        Gizmos.color = Color.cyan;
        Vector3 min = walkableTilemap.CellToWorld(walkableTilemap.cellBounds.min);
        Vector3 max = walkableTilemap.CellToWorld(walkableTilemap.cellBounds.max);
        Vector3 size = new Vector3(max.x - min.x, max.y - min.y, 1f);

        Gizmos.DrawWireCube(new Vector3(min.x + size.x / 2, min.y + size.y / 2, size.z), size);

        Gizmos.color = Color.magenta;

        if (m_unitsByPosition != null)
        {
            foreach (var node in m_unitsByPosition)
            {
                Gizmos.DrawWireCube(node.Key.position, node.Key.dimensions);
            }
        }

        if (m_unitPositionList != null)
        {
            Gizmos.color = Color.gray;
            Rect visiRect = m_unitPositionList.Bounds;
            Vector2Int visiSize = m_unitPositionList.Dimensions;
            Vector2 cellSize = new Vector2(visiRect.size.x / visiSize.x, visiRect.size.y / visiSize.y);

            bool showGrid = true;

            if (showGrid)
            {
                for (int x = 0; x < visiSize.x; x++)
                {
                    for (int y = 0; y < visiSize.y; y++)
                    {
                        Vector2 minPos = new Vector2(visiRect.x - cellSize.x * 0.5f + x * cellSize.x, visiRect.y - cellSize.y * 0.5f + y * cellSize.y);
                        Vector2 maxPos = new Vector2(visiRect.x + cellSize.x * 0.5f + x * cellSize.x, visiRect.y + cellSize.y * 0.5f + y * cellSize.y);

                        Gizmos.DrawLine(minPos, new Vector3(minPos.x, maxPos.y, 0f));
                        Gizmos.DrawLine(minPos, new Vector3(maxPos.x, minPos.y, 0f));
                        Gizmos.DrawLine(new Vector3(minPos.x, maxPos.y, 0f), maxPos);
                        Gizmos.DrawLine(maxPos, new Vector3(maxPos.x, minPos.y, 0f));
                    }
                }
            }
        }

        Gizmos.color = prevColor;
    }

    private void OnDrawGizmos()
    {
        Color prevColor = Gizmos.color;
#if UNITY_EDITOR
        Gizmos.color = Color.green;

        if (m_unitsByPosition != null)
        {
            foreach (var item in m_unitsByPosition)
            {
                UnityEditor.Handles.Label((Vector2)item.Value.transform.position + Vector2.up * 3f, m_unitPositionList.PrintKey(item.Key));
                UnityEditor.Handles.Label((Vector2)item.Value.transform.position + Vector2.up * 5f, item.Key.queryID.ToString());
            }
        }
#endif
        Gizmos.color = prevColor;
    }

    public void AddToOwnedUnits(Unit_Base unit)
    {
        m_ownedUnits.Add(unit);
        GridHashList2D.Node pos = m_unitPositionList.Add(unit.transform.position, Vector2.one * unit.circleCollider.radius * 2f);
        m_unitsByPosition.Add(pos, unit);

        var health = unit.GetComponent<Unit_Health>();

        if (health != null)
        {
            health.onKilled += HandleUnitKilled;
            damageableManager.RegisterDamageable(health, unit.gameObject);
        }

        if(ownedByPlayer && selectableManager != null)
        {
            selectableManager.RegisterSelectable(unit.GetComponent<ISelecteble>());
        }

        if(!m_unitsByType.ContainsKey(unit.unitType))
        {
            m_unitsByType.Add(unit.unitType, new List<Unit_Base>());
        }

        m_unitsByType[unit.unitType].Add(unit);

        onOwnedUnitAdded.Invoke(unit);
    }

    public void AddResource(float amount)
    {
        currentResources += amount;

        if (ownedByPlayer)
        {
            for (int i = 0; i < uiManager.buyUnitsButtons.Length; i++)
            {
                uiManager.buyUnitsButtons[i].button.interactable = uiManager.buyUnitsButtons[i].unit.price <= currentResources;
                uiManager.buyUnitsButtons[i].progressBarFill.color = unitData[i].price <= currentResources ? Color.white : Color.gray;
            }

            uiManager.wineAmountText.text = currentResources.ToString();
        } 
    }

    public List<ISelecteble> GetOwnedBuildingsByType(BuildingType type)
    {
        List<ISelecteble> buildings = new List<ISelecteble>();

        for (int i = 0; i < ownedBuildings.Count; i++)
        {
            if(ownedBuildings[i].type == type)
            {
                buildings.Add(ownedBuildings[i].selectable);
            }
        }

        return buildings;
    }

    private void HandleUnitKilled(IDamageable damageable)
    {
        GameObject killedObj = damageableManager.GetObject(damageable);

        for (int i = m_ownedUnits.Count - 1; i >= 0; i--)
        {
            if (m_ownedUnits[i].gameObject == killedObj)
            {
                if (ownerFaction == OwnerType.Player)
                {
                    Debug.Assert(selectableManager != null, "SelectableManager is null");
                    ISelecteble selectable = selectableManager.GetSelectable(killedObj);
                    Debug.Assert(selectable != null, killedObj.name + " is not a selectable");
                    selectableManager.UnregisterSelectable(selectable);
                }

                m_waitingForKill.Push(damageable);

                List<GridHashList2D.Node> closestNodes = m_unitPositionList.Find(m_ownedUnits[i].transform.position, Vector2.one * m_ownedUnits[i].circleCollider.radius);
                Debug.Assert(closestNodes.Count > 0, m_ownedUnits[i].name + ": not found at position");
                GridHashList2D.Node foundNode = null;

                for (int j = 0; j < closestNodes.Count; j++)
                {
                    if(m_unitsByPosition[closestNodes[j]] == m_ownedUnits[i])
                    {
                        foundNode = closestNodes[j];
                        break;
                    }
                }

                if(foundNode != null)
                {
                    m_unitPositionList.Remove(foundNode);
                    m_unitsByPosition.Remove(foundNode);
                }
                else
                {
                    Debug.LogError(m_ownedUnits[i].gameObject.name + ": node not found");
                }

                m_unitsByType[m_ownedUnits[i].unitType].Remove(m_ownedUnits[i]);
                
                onOwnedUnitRemoved(m_ownedUnits[i]);
                m_ownedUnits.RemoveAt(i);
                break;
            }
        }
    }

    private void HandleBuildingDestroyed(Building_Health health)
    {
        if(ownedByPlayer && currentGameState == GameState.Playing && health.buildingType == BuildingType.Base)
        {
            if(health.owningFaction == OwnerType.Enemy)
            {
                uiManager.ShowWinMenu();
                currentGameState = GameState.Win;
            }
            else if(health.owningFaction == OwnerType.Player)
            {
                uiManager.ShowGameOverMenu();
                currentGameState = GameState.GameOver;
            }
        }
    }

    [System.Serializable]
    public class Building
    {
        public ISelecteble selectable;
        public BuildingType type;

        [SerializeField] private GameObject m_gameObject;

        public GameObject gameObject { get { return m_gameObject; } }

        public Building(ISelecteble selectable, BuildingType type)
        {
            this.selectable = selectable;
            this.type = type;
            m_gameObject = (selectable as MonoBehaviour).gameObject;
        }
    }
}

public enum OwnerType { None, Player, Enemy }
public enum BuildingType { None, Base, UnitSpawn, ResourceProduction }
public enum GameState { None, Playing, Pause, Win, GameOver }