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
        public ushort bestCost;
        public FlowDirection bestDirection;

        public Cell(Vector2 worldPosition, Vector2Int gridIndex)
        {
            this.worldPosition = worldPosition;
            this.gridIndex = gridIndex;
            cost = 1;
            bestCost = ushort.MaxValue;
            bestDirection = FlowDirection.None;
        }

        public void AddCost(byte amount)
        {
            if(cost + amount >= byte.MaxValue)
            {
                cost = byte.MaxValue;
            }

            cost += amount;
        }

        public Cell DeepCopy()
        {
            Cell cell = new Cell(worldPosition, gridIndex);
            cell.cost = cost;
            cell.bestCost = bestCost;
            cell.bestDirection = bestDirection;

            return cell;
        }

        public override int GetHashCode()
        {
            int hash = 17 * 31 + gridIndex.x;
            return 31 * hash + gridIndex.y;
        }
    }

    public Cell[,] cells;
    public Vector2Int gridSize;
    public float cellRadius;
    public float cellDiameter;
    public Cell destinationCell;

    public FlowField(float cellRadius, Vector2Int gridSize)
    {
        this.cellRadius = cellRadius;
        cellDiameter = cellRadius * 2.0f;
        this.gridSize = gridSize;
    }

    public FlowField(FlowField from)
    {
        gridSize = from.gridSize;
        cells = new Cell[gridSize.x, gridSize.y];

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                cells[x, y] = from.cells[x, y].DeepCopy();
            }
        }

        cellRadius = from.cellRadius;
        cellDiameter = from.cellDiameter;
        destinationCell = from.destinationCell == null ? null : new Cell(from.destinationCell.worldPosition, from.destinationCell.gridIndex);
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

        string[] layers = new string[layerCosts.Keys.Count];
        layerCosts.Keys.CopyTo(layers, 0);
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

    public void CreateIntegrationField(Cell destinationCell)
    {
        this.destinationCell = destinationCell;
        this.destinationCell.cost = 0;
        this.destinationCell.bestCost = 0;

        Queue<Cell> cellsToCheck = new Queue<Cell>();
        cellsToCheck.Enqueue(destinationCell);

        while (cellsToCheck.Count > 0)
        {
            Cell cell = cellsToCheck.Dequeue();
            List<Cell> neighbors = GetNeighborCells(cell.gridIndex, FlowDirection.cardinalDirections);

            for (int i = 0; i < neighbors.Count; i++)
            {
                Cell neighbor = neighbors[i];

                if(neighbor.cost == byte.MaxValue) { continue; }    // impassable

                if(neighbor.cost + cell.bestCost < neighbor.bestCost)
                {
                    neighbor.bestCost = (ushort)(neighbor.cost + cell.bestCost);
                    cellsToCheck.Enqueue(neighbor);
                }
            }
        }
    }

    public void CreateFlowField()
    {
        foreach (Cell cell in cells)
        {
            List<Cell> neighbors = GetNeighborCells(cell.gridIndex, FlowDirection.directions);

            int bestCost = cell.bestCost;

            foreach (Cell neighbor in neighbors)
            {
                if(neighbor.bestCost < bestCost)
                {
                    bestCost = neighbor.bestCost;
                    cell.bestDirection = FlowDirection.GetDirection(neighbor.gridIndex - cell.gridIndex);
                }
            }
        }
    }

    private List<Cell> GetNeighborCells(Vector2Int cellIndex, List<FlowDirection> directions)
    {
        List<Cell> neighbors = new List<Cell>();

        for (int i = 0; i < directions.Count; i++)
        {
            Cell neighbor = GetCellAtDirection(cellIndex, directions[i]);

            if(neighbor != null)
            {
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }

    private Cell GetCellAtDirection(Vector2Int origin, Vector2Int displacement)
    {
        Vector2Int pos = origin + displacement;

        if(pos.x < 0 || pos.x >= gridSize.x || pos.y < 0 || pos.y >= gridSize.y)
        {
            return null;
        }

        return cells[pos.x, pos.y];
    }

    public Cell GetCell(Vector2 worldPosition)
    {
        float relativeX = worldPosition.x / (gridSize.x * cellDiameter);
        float relativeY = worldPosition.y / (gridSize.y * cellDiameter);

        relativeX = Mathf.Clamp01(relativeX);
        relativeY = Mathf.Clamp01(relativeY);

        int x = Mathf.Clamp(Mathf.FloorToInt(gridSize.x * relativeX), 0, gridSize.x - 1);
        int y = Mathf.Clamp(Mathf.FloorToInt(gridSize.y * relativeY), 0, gridSize.y - 1);

        return cells[x, y];
    }

    public class FlowDirection
    {
        public readonly Vector2Int vector;

        private FlowDirection(int x, int y)
        {
            vector = new Vector2Int(x, y);
        }

        public static implicit operator Vector2Int(FlowDirection direction)
        {
            return direction.vector;
        }

        public static FlowDirection GetDirection(Vector2Int vector)
        {
            FlowDirection direction = None;

            for (int i = 0; i < directions.Count; i++)
            {
                if(directions[i].vector == vector)
                {
                    direction = directions[i];
                    break;
                }
            }

            return direction;
        }

        public static readonly FlowDirection None = new FlowDirection(0, 0);
        public static readonly FlowDirection North = new FlowDirection(0, 1);
        public static readonly FlowDirection South = new FlowDirection(0, -1);
        public static readonly FlowDirection East = new FlowDirection(1, 0);
        public static readonly FlowDirection West = new FlowDirection(-1, 0);
        public static readonly FlowDirection NorthEast = new FlowDirection(1, 1);
        public static readonly FlowDirection NorthWest = new FlowDirection(-1, 1);
        public static readonly FlowDirection SouthEast = new FlowDirection(1, -1);
        public static readonly FlowDirection SouthWest = new FlowDirection(-1, -1);

        public static readonly List<FlowDirection> cardinalDirections = new List<FlowDirection>
        {
            North,
            East,
            South,
            West
        };

        public static readonly List<FlowDirection> directions = new List<FlowDirection>
        {
            None,
            North,
            East,
            South,
            West,
            NorthEast,
            NorthWest,
            SouthEast,
            SouthWest
        };
    }
}