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

    private Queue<int> m_buildQueue;
    private float m_buildTimer;

    private void Awake()
    {
        m_buildQueue = new Queue<int>();
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
        if (m_playerController.ownedByPlayer && m_buildQueue.Count > 0)
        {
            if (m_buildTimer <= 0f)
            {
                int unitIndex = m_buildQueue.Dequeue();
                Spawn(unitIndex);
                m_playerController.uiManager.RemoveUnitToQueue(unitIndex);

                if (m_buildQueue.Count > 0)
                {
                    m_buildTimer = units[m_buildQueue.Peek()].buildTime;
                }
            }

            if (m_buildQueue.Count > 0)
            {
                m_playerController.uiManager.UpdateBuyUnitFill(m_buildQueue.Peek(), m_buildTimer / units[m_buildQueue.Peek()].buildTime);
            }

            m_buildTimer -= Time.deltaTime;
        }

        if(IsSelected)
        {
            if(Input.GetMouseButtonUp(1))
            {
                Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);

                Plane groundPlane = new Plane(-Vector3.forward, spawnPoint.transform.position.z);
                float enter = 0.0f;

                if(groundPlane.Raycast(camRay, out enter))
                {
                    moveTarget.position = camRay.GetPoint(enter);
                }
            }
        }

        if (m_playerController.ownedByPlayer)
        {
            moveTargetSprite.enabled = IsSelected;
        }
    }

    public void OnSpawnUnitClick(int unitIndex)
    {
        if (m_playerController.ownedByPlayer)
        {
            if (m_playerController.currentResources >= units[unitIndex].price)
            {
                if (m_buildQueue.Count == 0)
                {
                    m_buildTimer = units[unitIndex].buildTime;
                }

                m_buildQueue.Enqueue(unitIndex);
                m_playerController.uiManager.AddUnitToQueue(unitIndex);
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
            m_playerController.AddToOwnedUnits(spawned);
            Vector2 adjustedPosition = GetAdjustedPosition(moveTarget.position, spawned.circleCollider.radius);
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
                if (m_playerController.currentResources >= units[i].price)
                {
                    var instance = Instantiate(units[i].unitPrefab, spawnPoint.position, Quaternion.identity) as GameObject;
                    instance.name = units[i].unitPrefab.name + m_playerController.CurrentPopulation;
                    m_playerController.AddResource(-units[i].price);
                    return instance.GetComponent<Unit_Base>();
                }
            }
        }

        return null;
    }

    private Vector2 GetAdjustedPosition(Vector3 worldPosition, float checkRadius, float distance = 0f)
    {
        Vector2 adjustedPosition = worldPosition + (worldPosition - transform.position).normalized * distance;

        LayerMask mask = LayerMask.GetMask("Unit", "Obstacle", "Building");

        Collider2D[] hits = Physics2D.OverlapCircleAll(adjustedPosition, checkRadius, mask);

        if (hits.Length == 0)
        {
            return adjustedPosition;
        }

        List<Vector2> circlePositions = GetPositionListCircle(adjustedPosition, checkRadius * 3.0f, 12);

        for (int i = 0; i < circlePositions.Count; i++)
        {
            if ((hits = Physics2D.OverlapCircleAll(circlePositions[i], checkRadius, mask)).Length == 0)
            {
                return circlePositions[i];
            }
        }

        return adjustedPosition + new Vector2(Random.insideUnitCircle.x * hits.Length * checkRadius, Random.insideUnitCircle.y * hits.Length * checkRadius);
    }

    private List<Vector2> GetPositionListCircle(Vector2 startPos, float[] dist, int[] posCount)
    {
        List<Vector2> positions = new List<Vector2>();
        positions.Add(startPos);

        for (int i = 0; i < dist.Length; i++)
        {
            positions.AddRange(GetPositionListCircle(startPos, dist[i], posCount[i]));
        }

        return positions;
    }

    private List<Vector2> GetPositionListCircle(Vector2 startPos, float dist, int posCount)
    {
        List<Vector2> positions = new List<Vector2>();

        for (int i = 0; i < posCount; i++)
        {
            float angle = i * (360f / posCount);
            Vector2 dir = Quaternion.Euler(0f, 0f, angle) * Vector3.right;
            positions.Add(startPos + dir * dist);
        }

        return positions;
    }
}
