using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridController : MonoBehaviour
{
    public Tilemap tileGrid;
    public FlowField flowField;

    private Dictionary<string, byte> m_layerCosts;

    private void Awake()
    {
        m_layerCosts = new Dictionary<string, byte>();
        m_layerCosts.Add("Obstacle", 255);
        m_layerCosts.Add("Building", 255);
    }

    private void Start()
    {
        transform.position = tileGrid.transform.position - new Vector3((tileGrid.cellBounds.size.x * tileGrid.cellSize.x) * 0.5f, (tileGrid.cellBounds.size.y * tileGrid.cellSize.y) * 0.5f, 0f);
        CreateFlowField();
    }

    private void OnDrawGizmosSelected()
    {
        if(flowField != null && flowField.cells != null)
        {
            Color prevColor = Gizmos.color;
            Gizmos.color = Color.gray;
            Vector2 startPos = transform.position;

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

#if UNITY_EDITOR
            Gizmos.color = Color.white;

            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.MiddleCenter;

            foreach (var cell in flowField.cells)
            {
                if (cell.cost < 2) continue;

                UnityEditor.Handles.Label(cell.worldPosition, cell.cost.ToString(), style);
            }
#endif

            Gizmos.color = prevColor;
        }
    }

    private void CreateFlowField()
    {
        Vector2Int gridSize = new Vector2Int(tileGrid.cellBounds.size.x, tileGrid.cellBounds.size.y);
        flowField = new FlowField(tileGrid.cellSize.x * 0.5f, gridSize);
        flowField.CreateGrid(transform.position);
        flowField.CreateCostField(m_layerCosts);
    }
}