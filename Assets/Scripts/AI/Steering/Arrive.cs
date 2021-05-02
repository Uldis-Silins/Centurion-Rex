using System.Collections;
using UnityEngine;

public class Arrive : AgentBehaviour
{
    public float targetRadius;
    public float slowRadius;
    public float cellRadius;
    public float timeToTarget = 0.1f;
    public FlowField flowField;
    public Vector3 gridWorldOffset;

    [Header("Debug")]
    public bool debugFlowField;
    public enum DebugModeType { CostField, IntegrationField, FlowField }
    public DebugModeType debugMode;

    public bool IsMoving { get; private set; }

    public override void Awake()
    {
        base.Awake();
    }

    public override void Update()
    {
        base.Update();
    }

    private void OnDrawGizmosSelected()
    {
        if (debugFlowField && flowField != null && flowField.cells != null)
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


            Gizmos.color = Color.white;

            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.MiddleCenter;
#if UNITY_EDITOR
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


    public override Steering GetSteering()
    {
        Steering steering = new Steering();
        Vector2 direction = m_moveTarget - m_position;    // Replaced by flow field

        if (flowField != null && flowField.cells != null)
        {
            FlowField.Cell curCell = flowField.GetCell(transform.position + gridWorldOffset);
            Vector2 cellDirection = new Vector2(curCell.bestDirection.vector.x, curCell.bestDirection.vector.y);
            direction = Vector2.Lerp(direction, cellDirection, RemainingDistance / (cellRadius * 2));
        }

        float distance = direction.magnitude;
        float targetSpeed;

        IsMoving = true;

        if (distance < targetRadius)
        {
            IsMoving = false;
            return steering;
        }
        else if (distance > slowRadius)
        {
            targetSpeed = agent.maxSpeed;
        }
        else
        {
            targetSpeed = agent.maxSpeed * distance / slowRadius;
        }

        Vector2 desiredVelocity = direction;
        desiredVelocity.Normalize();
        desiredVelocity *= targetSpeed;
        steering.linear = desiredVelocity - agent.velocity;
        steering.linear /= timeToTarget;

        if (steering.linear.magnitude > agent.maxAccel)
        {
            steering.linear.Normalize();
            steering.linear *= agent.maxAccel;
        }

        return steering;
    }
}