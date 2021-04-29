using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class NavigationController : MonoBehaviour
{
    public Tilemap tileGrid;
    public Vector2Int manualGridSize;
    public bool useTileGridSize;
    public FlowField flowField;

    [Header("Debug")]
    public bool debug;
    public bool showGrid = true;

    public enum DebugModeType { CostField, IntegrationField, FlowField }
    public DebugModeType debugMode;

    private Dictionary<string, byte> m_layerCosts;

    public Vector3 gridOffset;

    private void Awake()
    {
        m_layerCosts = new Dictionary<string, byte>();
        m_layerCosts.Add("Obstacle", 255);
        m_layerCosts.Add("Building", 255);
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
        if(debug && flowField != null && flowField.cells != null)
        {
            Color prevColor = Gizmos.color;
            Gizmos.color = Color.gray;
            Vector2 startPos = transform.position;

            if (showGrid)
            {
                for (int x = 0; x < flowField.gridSize.x; x++)
                {
                    for (int y = 0; y < flowField.gridSize.y; y++)
                    {
                        Vector2 minPos = new Vector2(flowField.cells[x, y].worldPosition.x - flowField.cellRadius, flowField.cells[x, y].worldPosition.y - flowField.cellRadius);
                        Vector2 maxPos = new Vector2(flowField.cells[x, y].worldPosition.x + flowField.cellRadius, flowField.cells[x, y].worldPosition.y + flowField.cellRadius);

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
                    foreach (var cell in flowField.cells)
                    {
                        if (cell.cost < 2) continue;

                        UnityEditor.Handles.Label(cell.worldPosition, cell.cost.ToString(), style);
                    }
                    break;
                case DebugModeType.IntegrationField:
                    foreach (var cell in flowField.cells)
                    {
                        if (cell.bestCost > 20) continue;

                        UnityEditor.Handles.Label(cell.worldPosition, cell.bestCost.ToString(), style);
                    }
                    break;
                case DebugModeType.FlowField:
                    Gizmos.color = Color.yellow;
                    foreach (var cell in flowField.cells)
                    {
                        Gizmos.DrawLine(cell.worldPosition, cell.worldPosition + new Vector2(cell.bestDirection.vector.x, cell.bestDirection.vector.y) * flowField.cellRadius);
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
            FlowField.Cell destinationCell = flowField.GetCell(worldPos);
            flowField.CreateIntegrationField(destinationCell);
            flowField.CreateFlowField();
        }
    }

    private void CreateFlowField()
    {
        Vector2Int size = useTileGridSize ? new Vector2Int(tileGrid.cellBounds.size.x, tileGrid.cellBounds.size.y) : manualGridSize;
        Vector2Int gridSize = size;
        flowField = new FlowField(tileGrid.cellSize.x * 0.5f, gridSize);

        flowField.CreateGrid(transform.position);
        flowField.CreateCostField(m_layerCosts);
    }

    public void SetDestination(Vector3 worldPos)
    {
        CreateFlowField();
        worldPos += gridOffset;
        FlowField.Cell destinationCell = flowField.GetCell(worldPos);
        flowField.CreateIntegrationField(destinationCell);
        flowField.CreateFlowField();
    }
}