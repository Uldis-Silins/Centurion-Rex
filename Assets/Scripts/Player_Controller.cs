using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class Player_Controller : MonoBehaviour
{
    public bool ownedByPlayer;
    public FactionType ownerFaction;
    public FactionType enemyFaction;

    public UI_GameManager uiManager;
    public UI_HudManager hudManager;
    public List<Building> spawnedBuildings;

    public LayerMask buildingLayer;

    [SerializeField] private SelectableManager selectableManager;
    [SerializeField] private DamageableManager damageableManager;

    public static GameState currentGameState;

    public float currentResources;
    public UnitData[] unitData;

    private List<Unit_Base> m_ownedUnits;

    private Stack<IDamageable> m_waitingForKill;

    private Camera m_mainCam;

    private ISelecteble m_selectedBuilding;

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
        if (ownedByPlayer)
        {
            currentGameState = GameState.Playing;
            uiManager.wineAmountText.text = currentResources.ToString();
        }

        //StartCoroutine(CheckUnitOverlap());
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
                GameObject selectableObject = hit.collider.gameObject;
                ISelecteble selectable = selectableObject.GetComponent<ISelecteble>();

                if (selectable != null)
                {
                    for (int i = 0; i < spawnedBuildings.Count; i++)
                    {
                        if (spawnedBuildings[i].selecteble == selectableObject)
                        {
                            m_selectedBuilding = selectable;
                            selectable.Select();
                            uiManager.ShowBuildingMenu(spawnedBuildings[i].type);
                        }
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
            }
        }
    }

    public void AddToOwnedUnits(Unit_Base unit)
    {
        m_ownedUnits.Add(unit);

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
    }

    public void AddResource(float amount)
    {
        currentResources += amount;

        if (ownedByPlayer)
        {
            uiManager.wineAmountText.text = currentResources.ToString();
        }

        for (int i = 0; i < unitData.Length; i++)
        {
            uiManager.buyUnitsButtons[i].interactable = unitData[i].price <= currentResources;
            uiManager.buyUnitsButtons[i].transform.GetChild(0).GetComponent<UnityEngine.UI.Image>().color = unitData[i].price <= currentResources ? Color.white : Color.gray;
        }
    }

    private void HandleUnitKilled(IDamageable damageable)
    {
        GameObject killedObj = damageableManager.GetObject(damageable);

        for (int i = m_ownedUnits.Count - 1; i >= 0; i--)
        {
            if (m_ownedUnits[i].gameObject == killedObj)
            {
                Debug.Assert(selectableManager != null, "SelectableManager is null");

                ISelecteble selectable = selectableManager.GetSelectable(killedObj);

                Debug.Assert(selectable != null, killedObj.name + " is not a selectable");

                selectableManager.UnregisterSelectable(selectable);

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

    //private IEnumerator CheckUnitOverlap()
    //{
    //    while (true)
    //    {
    //        for (int i = 0; i < m_ownedUnits.Count; i++)
    //        {
    //            Vector3 pos = m_ownedUnits[i].HasMoveTarget ? m_ownedUnits[i].MoveTarget : m_ownedUnits[i].transform.position;
    //            List<Arrive> overlapped = CheckUnitOverlap(pos, m_ownedUnits[i]);
    //            List<Vector3> targetPositions = GetPositionListCircle(pos, new float[] { 0.5f, 1f, 2f }, new int[] { 5, 10, 20 });

    //            if (overlapped.Count > 0)
    //            {
    //                foreach (var unit in overlapped)
    //                {
    //                    if (unit != m_ownedUnits[i])
    //                    {
    //                        unit.SetDestination(targetPositions[i % targetPositions.Count]);
    //                    }
    //                }
    //                yield return new WaitForSeconds(0.2f);
    //            }
    //        }

    //        yield return null;
    //    }
    //}

    //private List<Arrive> CheckUnitOverlap(Vector3 pos, Unit_Base caller)
    //{
    //    Collider[] hits = Physics.OverlapSphere(pos, 0.25f, 1 << LayerMask.NameToLayer("Unit"));
    //    List<Arrive> overlappedUnits = new List<Arrive>();

    //    if (hits.Length > 2)
    //    {
    //        hits = Physics.OverlapSphere(pos, 2.5f, 1 << LayerMask.NameToLayer("Unit"));
    //    }

    //    for (int i = 0; i < hits.Length; i++)
    //    {
    //        if (hits[i].gameObject == caller.gameObject) continue;

    //        Arrive hitAgent = hits[i].gameObject.GetComponent<Arrive>();
    //        if (hitAgent != null && !hitAgent.IsMoving && hitAgent.gameObject.GetComponent<Unit_Health>().Faction == ownerFaction)
    //        {
    //            overlappedUnits.Add(hitAgent);
    //        }
    //    }

    //    return overlappedUnits;
    //}

    private List<Vector3> GetPositionListCircle(Vector3 startPos, float[] dist, int[] posCount)
    {
        List<Vector3> positions = new List<Vector3>();
        positions.Add(startPos);

        for (int i = 0; i < dist.Length; i++)
        {
            positions.AddRange(GetPositionListCircle(startPos, dist[i], posCount[i]));
        }

        return positions;
    }

    private List<Vector3> GetPositionListCircle(Vector3 startPos, float dist, int posCount)
    {
        List<Vector3> positions = new List<Vector3>();

        for (int i = 0; i < posCount; i++)
        {
            float angle = i * (360f / posCount);
            Vector3 dir = Quaternion.Euler(0f, angle, 0f) * Vector3.right;
            positions.Add(startPos + dir * dist);
        }

        return positions;
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