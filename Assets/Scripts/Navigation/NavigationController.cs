using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class NavigationController : MonoBehaviour
{
    public Tilemap tileGrid;
    public Vector2Int manualGridSize;
    public bool useTileGridSize;
    [SerializeField] private FlowField m_flowField;

    public LayerMask obstacleLayers;

    [Header("Debug")]
    public bool debug;
    public bool showGrid = true;

    public enum DebugModeType { CostField, IntegrationField, FlowField }
    public DebugModeType debugMode;

    private Dictionary<string, byte> m_layerCosts;
    private Dictionary<int, FlowField> m_cahcedFields;

    public Vector3 gridOffset;

    private void Awake()
    {
        m_layerCosts = new Dictionary<string, byte>();

        for (int i = 0; i < 32; i++)
        {
            if(obstacleLayers == (obstacleLayers | (1 << i)))
            {
                m_layerCosts.Add(LayerMask.LayerToName(i), 255);
            }
        }

        m_cahcedFields = new Dictionary<int, FlowField>();
    }

    private void Start()
    {
        if (useTileGridSize)
        {
            gridOffset = new Vector3((tileGrid.cellBounds.size.x * tileGrid.cellSize.x) * 0.5f, (tileGrid.cellBounds.size.y * tileGrid.cellSize.y) * 0.5f, 0f);
            transform.position = tileGrid.transform.position - gridOffset;
        }
        else
        {
            gridOffset = -transform.position;
            gridOffset.z = 0;
        }

        CreateFlowField();
    }

    private void OnDrawGizmosSelected()
    {
        if(debug && m_flowField != null && m_flowField.cells != null)
        {
            Color prevColor = Gizmos.color;
            Gizmos.color = Color.gray;
            Vector2 startPos = transform.position;

            if (showGrid)
            {
                for (int x = 0; x < m_flowField.gridSize.x; x++)
                {
                    for (int y = 0; y < m_flowField.gridSize.y; y++)
                    {
                        Vector2 minPos = new Vector2(m_flowField.cells[x, y].worldPosition.x - m_flowField.cellRadius, m_flowField.cells[x, y].worldPosition.y - m_flowField.cellRadius);
                        Vector2 maxPos = new Vector2(m_flowField.cells[x, y].worldPosition.x + m_flowField.cellRadius, m_flowField.cells[x, y].worldPosition.y + m_flowField.cellRadius);

                        Gizmos.DrawLine(minPos, new Vector3(minPos.x, maxPos.y, 0f));
                        Gizmos.DrawLine(minPos, new Vector3(maxPos.x, minPos.y, 0f));
                        Gizmos.DrawLine(new Vector3(minPos.x, maxPos.y, 0f), maxPos);
                        Gizmos.DrawLine(maxPos, new Vector3(maxPos.x, minPos.y, 0f));
                    }
                }
            }

#if UNITY_EDITOR
            Gizmos.color = Color.white;

            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.MiddleCenter;

            switch (debugMode)
            {
                case DebugModeType.CostField:
                    foreach (var cell in m_flowField.cells)
                    {
                        if (cell.cost < 2) continue;

                        UnityEditor.Handles.Label(cell.worldPosition, cell.cost.ToString(), style);
                    }
                    break;
                case DebugModeType.IntegrationField:
                    foreach (var cell in m_flowField.cells)
                    {
                        if (cell.bestCost > 20) continue;

                        UnityEditor.Handles.Label(cell.worldPosition, cell.bestCost.ToString(), style);
                    }
                    break;
                case DebugModeType.FlowField:
                    Gizmos.color = Color.yellow;
                    foreach (var cell in m_flowField.cells)
                    {
                        Gizmos.DrawLine(cell.worldPosition, cell.worldPosition + new Vector2(cell.bestDirection.vector.x, cell.bestDirection.vector.y) * m_flowField.cellRadius);
                    }
                    break;
                default:
                    break;
            }
#endif

            Gizmos.color = prevColor;
        }
    }

    private void Update()
    {
        if(debug && Input.GetMouseButtonUp(0))
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldPos += gridOffset;
            FlowField.Cell destinationCell = m_flowField.GetCell(worldPos);
            m_flowField.CreateIntegrationField(destinationCell);
            m_flowField.CreateFlowField();
        }
    }

    // Template
    private void CreateFlowField()
    {
        Vector2Int size = useTileGridSize ? new Vector2Int(tileGrid.cellBounds.size.x, tileGrid.cellBounds.size.y) : manualGridSize;
        Vector2Int gridSize = size;
        m_flowField = new FlowField(tileGrid.cellSize.x * 0.5f, gridSize);

        m_flowField.CreateGrid(transform.position);
        m_flowField.CreateCostField(m_layerCosts);
    }

    public FlowField GetFlowField(Vector3 fromPosition, Vector3 toPosition)
    {
        int hash = GetGridHash(m_flowField.GetCell(fromPosition).gridIndex, m_flowField.GetCell(toPosition).gridIndex);

        if (!m_cahcedFields.ContainsKey(hash))
        {
            FlowField field = new FlowField(m_flowField);
            m_cahcedFields.Add(hash, field);
            toPosition += gridOffset;
            FlowField.Cell destinationCell = field.GetCell(toPosition);
            field.CreateIntegrationField(destinationCell);
            field.CreateFlowField();
            return field;
        }
        else
        {
            return m_cahcedFields[hash];
        }
    }

    private int GetGridHash(Vector2Int fromCell, Vector2Int toCell)
    {
        int hash = 17 * 23 + fromCell.GetHashCode();
        return hash * 23 + toCell.GetHashCode(); 
    }
}