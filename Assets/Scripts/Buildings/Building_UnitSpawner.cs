using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building_UnitSpawner : Building_Base
{
    public UnitData[] units;

    public Transform spawnPoint;
    public Transform moveTarget;
    public SpriteRenderer moveTargetSprite;

    public AudioSource audioSource;

    private List<int> m_buildQueue;
    private float m_buildTimer;
    private bool m_inChangeMoveTarget;

    protected override void Awake()
    {
        base.Awake();

        m_buildQueue = new List<int>();
    }

    private void Start()
    {
        if (m_playerController.ownedByPlayer)
        {
            moveTargetSprite.enabled = false;
        }
    }

    private void Update()
    {
        if (m_buildQueue.Count > 0)
        {
            if (m_buildTimer <= 0f)
            {
                int unitIndex = m_buildQueue[0];
                m_buildQueue.RemoveAt(0);

                Spawn(unitIndex);

                if (m_playerController.ownedByPlayer)
                {
                    m_playerController.uiManager.RemoveUnitToQueue(unitIndex);

                    List<UnitData> unitList = new List<UnitData>();

                    for (int i = 0; i < m_buildQueue.Count; i++)
                    {
                        unitList.Add(units[m_buildQueue[i]]);
                    }

                    m_playerController.uiManager.SetBuildQueue(unitList);

                    if (m_buildQueue.Count == 0)
                    {
                        m_playerController.uiManager.UpdateBuyUnitFill(unitIndex, 1f);
                    }
                }

                if (m_buildQueue.Count > 0)
                {
                    m_buildTimer = units[m_buildQueue[0]].buildTime;
                }
            }

            if (m_playerController.ownedByPlayer && m_buildQueue.Count > 0)
            {
                float t = m_buildTimer / units[m_buildQueue[0]].buildTime;
                m_playerController.uiManager.UpdateBuyUnitFill(m_buildQueue[0], 1f - t);
            }

            m_buildTimer -= Time.deltaTime;
        }

        m_playerController.BlockBuildingInteraction = m_inChangeMoveTarget;

        if (m_inChangeMoveTarget && m_playerController.ownedByPlayer && IsSelected && !UI_Helpers.IsPointerOverUIElement())
        {
            Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);

            Plane groundPlane = new Plane(-Vector3.forward, spawnPoint.transform.position.z);
            float enter = 0.0f;

            if (groundPlane.Raycast(camRay, out enter))
            {
                moveTarget.position = camRay.GetPoint(enter);
            }

            if (Input.GetMouseButtonDown(0))
            {
                m_inChangeMoveTarget = false;
            }
        }

        if (m_playerController.ownedByPlayer)
        {
            moveTargetSprite.enabled = IsSelected;
        }
    }

    public void OnSpawnUnitClick(UnitData unit)
    {
        int unitIndex = -1;

        for (int i = 0; i < units.Length; i++)
        {
            if(unit.type == units[i].type)
            {
                unitIndex = i;
                break;
            }
        }

        if (m_playerController.currentResources >= units[unitIndex].price)
        {
            if (m_buildQueue.Count == 0)
            {
                m_buildTimer = units[unitIndex].buildTime;
            }

            m_buildQueue.Add(unitIndex);
            m_playerController.AddResource(-units[unitIndex].price);

            if (m_playerController.ownedByPlayer)
            {
                m_playerController.uiManager.AddUnitToQueue(unitIndex);

                List<UnitData> unitList = new List<UnitData>();

                for (int i = 0; i < m_buildQueue.Count; i++)
                {
                    unitList.Add(units[m_buildQueue[i]]);
                }

                m_playerController.uiManager.SetBuildQueue(unitList);
            }
        }
    }

    public void OnCancelBuildClick(UnitData unit)
    {
        int unitIndex = -1;

        for (int i = 0; i < units.Length; i++)
        {
            if (unit.type == units[i].type)
            {
                unitIndex = i;
                break;
            }
        }

        if (m_buildQueue.Contains(unitIndex))
        {
            m_buildTimer = m_buildQueue.Count > 0 ? units[m_buildQueue[0]].buildTime : 1f;

            if (m_playerController.ownedByPlayer)
            {
                m_playerController.uiManager.RemoveUnitToQueue(unitIndex);
                m_playerController.uiManager.UpdateBuyUnitFill(unitIndex, m_buildTimer);
            }

            m_buildQueue.Remove(unitIndex);
            m_playerController.AddResource(units[unitIndex].price);

            if(m_playerController.ownedByPlayer)
            {
                List<UnitData> unitList = new List<UnitData>();

                for (int i = 0; i < m_buildQueue.Count; i++)
                {
                    unitList.Add(units[m_buildQueue[i]]);
                }

                m_playerController.uiManager.SetBuildQueue(unitList);
            }
        }
    }

    public void OnRemoveFromBuildQueueClick(int buttonIndex)
    {
        if(m_buildQueue.Count >= buttonIndex)
        {
            OnCancelBuildClick(units[m_buildQueue[buttonIndex]]);
        }
    }

    public void OnChangeMoveTargetClick()
    {
        m_inChangeMoveTarget = true;
        m_playerController.BlockBuildingInteraction = true;
    }

    private void Spawn(int unitIndex)
    {
        var spawned = SpawnUnit(units[unitIndex].type);

        if (spawned != null)
        {
            spawned.unitType = units[unitIndex].type;
            m_playerController.AddToOwnedUnits(spawned);
            Vector2 adjustedPosition = Formations.GetAdjustedPosition(moveTarget.position, spawned, spawned.circleCollider.radius);
            spawned.SetMoveTarget(adjustedPosition);
            spawned.SetState(Unit_Base.UnitStateType.Move);

            if(m_playerController.ownedByPlayer)
            {
                audioSource.Play();
            }
        }
    }

    private Unit_Base SpawnUnit(UnitData.UnitType type)
    {
        for (int i = 0; i < units.Length; i++)
        {
            if(units[i].type == type)
            {
                //if (m_playerController.currentResources >= units[i].price)
                //{
                    var instance = Instantiate(units[i].unitPrefab, spawnPoint.position, Quaternion.identity) as GameObject;
                    instance.name = units[i].unitPrefab.name + m_playerController.CurrentPopulation;
                    
                    return instance.GetComponent<Unit_Base>();
                //}
            }
        }

        return null;
    }
}
