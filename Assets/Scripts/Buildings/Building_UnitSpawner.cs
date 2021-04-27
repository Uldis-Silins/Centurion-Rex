﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building_UnitSpawner : MonoBehaviour, ISelecteble
{
    public UnitData[] units;

    public Transform spawnPoint;
    public Transform moveTarget;
    public SpriteRenderer moveTargetSprite;

    public AudioSource audioSource;

    [SerializeField] private Player_Controller playerController;

    private Queue<int> m_buildQueue;
    private float m_buildTimer;

    public bool IsSelected { get; protected set; }
    public GameObject SelectableGameObject { get { return this.gameObject; } }

    private void Awake()
    {
        m_buildQueue = new Queue<int>();
    }

    private void Start()
    {
        if (playerController.ownedByPlayer)
        {
            moveTargetSprite.enabled = false;
        }
    }

    private void Update()
    {
        if (playerController.ownedByPlayer && m_buildQueue.Count > 0)
        {
            if (m_buildTimer <= 0f)
            {
                int unitIndex = m_buildQueue.Dequeue();
                Spawn(unitIndex);
                playerController.uiManager.RemoveUnitToQueue(unitIndex);

                if (m_buildQueue.Count > 0)
                {
                    m_buildTimer = units[m_buildQueue.Peek()].buildTime;
                }
            }

            if (m_buildQueue.Count > 0)
            {
                playerController.uiManager.UpdateBuyUnitFill(m_buildQueue.Peek(), m_buildTimer / units[m_buildQueue.Peek()].buildTime);
            }

            m_buildTimer -= Time.deltaTime;
        }

        if(IsSelected)
        {
            if(Input.GetMouseButtonUp(1))
            {
                Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);

                Plane groundPlane = new Plane(Vector3.up, spawnPoint.transform.position.y);
                float enter = 0.0f;

                if(groundPlane.Raycast(camRay, out enter))
                {
                    moveTarget.position = camRay.GetPoint(enter);
                }
            }
        }
    }

    public void OnSpawnUnitClick(int unitIndex)
    {
        if (playerController.ownedByPlayer)
        {
            if (playerController.currentResources >= units[unitIndex].price)
            {
                if (m_buildQueue.Count == 0)
                {
                    m_buildTimer = units[unitIndex].buildTime;
                }

                m_buildQueue.Enqueue(unitIndex);
                playerController.uiManager.AddUnitToQueue(unitIndex);
            }
        }
        else
        {
            Spawn(unitIndex);
        }
    }

    private void Spawn(int unitIndex)
    {
        var spawned = SpawnUnit(units[unitIndex].type);

        if (spawned != null)
        {
            spawned.unitType = units[unitIndex].type;
            playerController.AddToOwnedUnits(spawned);
            spawned.SetMoveTarget(moveTarget.position);
            spawned.SetState(Unit_Base.UnitStateType.Move);

            if(playerController.ownedByPlayer)
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
                if (playerController.currentResources >= units[i].price)
                {
                    var instance = Instantiate(units[i].unitPrefab, spawnPoint.position, Quaternion.identity) as GameObject;
                    instance.name = units[i].unitPrefab.name + playerController.CurrentPopulation;
                    playerController.AddResource(-units[i].price);
                    return instance.GetComponent<Unit_Base>();
                }
            }
        }

        return null;
    }

    public void Select()
    {
        IsSelected = true;

        moveTargetSprite.enabled = true;
    }

    public void Deselect()
    {
        IsSelected = false;

        moveTargetSprite.enabled = false;
    }
}
