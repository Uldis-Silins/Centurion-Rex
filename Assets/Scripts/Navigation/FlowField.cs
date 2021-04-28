using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FlowField
{
    [System.Serializable]
    public class Cell
    {
        public Vector2 worldPosition;
        public Vector2Int gridIndex;
        public byte cost;

        public Cell(Vector2 worldPosition, Vector2Int gridIndex)
        {
            this.worldPosition = worldPosition;
            this.gridIndex = gridIndex;
            cost = 1;
        }

        public void AddCost(byte amount)
        {
            if(cost + amount >= byte.MaxValue)
            {
                cost = byte.MaxValue;
            }

            cost += amount;
        }
    }

    public Cell[,] cells;
    public Vector2Int gridSize;
    public float cellRadius;
    public float cellDiameter;

    public FlowField(float cellRadius, Vector2Int gridSize)
    {
        this.cellRadius = cellRadius;
        cellDiameter = cellRadius * 2.0f;
        this.gridSize = gridSize;
    }

    public void CreateGrid(Vector2 startPosition)
    {
        cells = new Cell[gridSize.x, gridSize.y];

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector2 worldPos = startPosition + new Vector2(x * cellDiameter + cellRadius, y * cellDiameter + cellRadius);
                cells[x, y] = new Cell(worldPos, new Vector2Int(x, y));
            }
        }
    }

    public void CreateCostField(Dictionary<string, byte> layerCosts)
    {
        Vector2 cellHalfExtents = Vector2.one * cellRadius;

        //string[] layers = new string[layerCosts.Keys.Count];
        //layerCosts.Keys.CopyTo(layers, 0);
        string[] layers = new string[] { "Obstacle", "Building" };
        int terrainMask = LayerMask.GetMask(layers);

        foreach (var cell in cells)
        {
            Collider2D[] hits = Physics2D.OverlapBoxAll(cell.worldPosition, cellHalfExtents, 0f, terrainMask);
            bool hasIncreaseCost = false;

            foreach (var hit in hits)
            {
                if (!hasIncreaseCost)
                {
                    cell.AddCost(layerCosts[LayerMask.LayerToName(hit.gameObject.layer)]);
                    hasIncreaseCost = true;
                }
            }
        }
    }
}