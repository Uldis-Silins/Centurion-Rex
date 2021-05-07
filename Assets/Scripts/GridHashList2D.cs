using System.Collections.Generic;
using UnityEngine;

public class GridHashList2D
{
    public class Node
    {
        public Vector2 position;
        public Vector2 dimensions;
        public Vector2Int[] indices;

        public int queryID = -1;

        public Node(Vector2 position, Vector2 dimensions)
        {
            this.position = position;
            this.dimensions = dimensions;
            indices = null;
        }
    }

    [SerializeField, Tooltip("Min and max positions of grid area")]
    private Rect m_bounds;

    [SerializeField, Tooltip("Grid size (cell count)")]
    private Vector2Int m_dimensions;

    private Dictionary<int, HashSet<Node>> m_cells;

    private int m_queryIDs = 0;

    public GridHashList2D(Rect bounds, Vector2Int dimensions)
    {
        m_bounds = bounds;
        m_dimensions = dimensions;

        m_cells = new Dictionary<int, HashSet<Node>>();
        m_queryIDs = 0;
    }

    public Node Add(Vector2 position, Vector2 dimensions)
    {
        Node node = new Node(position, dimensions);
        Insert(node);

        return node;
    }

    public List<Node> Find(Vector2 position, Vector2 bounds)
    {
        float posX = position.x;
        float posY = position.y;
        float w = bounds.x;
        float h = bounds.y;

        Debug.DrawLine(new Vector3(posX - w / 2, posY - h / 2), new Vector3(posX - w / 2, posY + h / 2), Color.green);
        Debug.DrawLine(new Vector3(posX - w / 2, posY - h / 2), new Vector3(posX + w / 2, posY - h / 2), Color.green);
        Debug.DrawLine(new Vector3(posX + w / 2, posY + h / 2), new Vector3(posX - w / 2, posY + h / 2), Color.green);
        Debug.DrawLine(new Vector3(posX + w / 2, posY + h / 2), new Vector3(posX + w / 2, posY - h / 2), Color.green);

        Vector2Int minIndex = GetCellIndex(posX - w / 2, posY - h / 2);
        Vector2Int maxIndex = GetCellIndex(posX + w / 2, posY + h / 2);

        List<Node> foundNodes = new List<Node>();
        int queryID = m_queryIDs++;

        for (int x = minIndex[0], xn = maxIndex[0]; x <= xn; ++x)
        {
            for (int y = minIndex[1], yn = maxIndex[1]; y <= yn; ++y)
            {
                int key = CreateKey(x, y);

                if (m_cells.ContainsKey(key))
                {
                    foreach (var item in m_cells[key])
                    {
                        if (item.queryID != queryID)
                        {
                            item.queryID = queryID;
                            foundNodes.Add(item);
                        }
                    }
                }
            }
        }

        return foundNodes;
    }

    public void Update(Node node)
    {
        float posX = node.position.x;
        float posY = node.position.y;
        float w = node.dimensions.x;
        float h = node.dimensions.y;

        Vector2Int minIndex = GetCellIndex(posX - w / 2, posY - h / 2);
        Vector2Int maxIndex = GetCellIndex(posX + w / 2, posY + h / 2);

        if (node.indices[0] == minIndex && node.indices[1] == maxIndex)
        {
            return;
        }

        Remove(node);
        Insert(node);
    }

    public void Remove(Node node)
    {
        Vector2Int minIndex = node.indices[0];
        Vector2Int maxIndex = node.indices[1];

        for (int x = minIndex[0], xn = maxIndex[0]; x <= xn; ++x)
        {
            for (int y = minIndex[1], yn = maxIndex[1]; y <= yn; ++y)
            {
                int key = CreateKey(x, y);
                m_cells[key].Remove(node);
            }
        }
    }

    private void Insert(Node node)
    {
        float posX = node.position.x;
        float posY = node.position.y;
        float w = node.dimensions.x;
        float h = node.dimensions.y;

        Vector2Int minIndex = GetCellIndex(posX - w / 2, posY - h / 2);
        Vector2Int maxIndex = GetCellIndex(posX + w / 2, posY + h / 2);

        node.indices = new Vector2Int[] { minIndex, maxIndex };

        for (int x = minIndex[0], xn = maxIndex[0]; x <= xn; ++x)
        {
            for (int y = minIndex[1], yn = maxIndex[1]; y <= yn; ++y)
            {
                int key = CreateKey(x, y);

                if (!m_cells.ContainsKey(key))
                {
                    m_cells.Add(key, new HashSet<Node>());
                }

                m_cells[key].Add(node);
            }
        }
    }

    /// <summary>
    /// Index of cell from world position.
    /// </summary>
    private Vector2Int GetCellIndex(float posX, float posY)
    {
        float x = ((posX - m_bounds.x) / m_bounds.width);
        float y = ((posY - m_bounds.y) / m_bounds.height);

        int xIndex = Mathf.FloorToInt(x * (m_dimensions[0] - 1));
        int yIndex = Mathf.FloorToInt(y * (m_dimensions[1] - 1));

        return new Vector2Int(xIndex, yIndex);
    }

    /// <summary>
    /// Create hash of cell.
    /// </summary>
    /// <param name="x">Cell x position</param>
    /// <param name="y">Cell y position</param>
    /// <returns>Hash of cell x and y positions.</returns>
    private int CreateKey(int x, int y)
    {
        int hash = 17 * 23 + x;
        return hash * 23 + y;
    }
}