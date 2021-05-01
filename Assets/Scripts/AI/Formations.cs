using System.Collections.Generic;
using UnityEngine;

public static class Formations
{
    public static Vector2 GetAdjustedPosition(Vector3 worldPosition, Unit_Base unit, float checkRadius, float distance = 0f)
    {
        Vector2 adjustedPosition = worldPosition + (worldPosition - unit.transform.position).normalized * distance;

        LayerMask mask = LayerMask.GetMask("Unit", "Obstacle", "Building");

        Collider2D[] hits = Physics2D.OverlapCircleAll(adjustedPosition, checkRadius, mask);

        if (hits.Length == 0)
        {
            return adjustedPosition;
        }

        List<Vector2> formationPositions = GetPositionListCircle(adjustedPosition, new float[] { 1f, 2f, 3f, 4f, 5f }, new int[] { 5, 10, 20, 40, 60 });

        for (int i = 0; i < formationPositions.Count; i++)
        {
            if ((hits = Physics2D.OverlapCircleAll(formationPositions[i], checkRadius, mask)).Length == 0)
            {
                return formationPositions[i];
            }
        }

        return adjustedPosition + new Vector2(Random.insideUnitCircle.x * hits.Length * checkRadius, Random.insideUnitCircle.y * hits.Length * checkRadius);
    }

    public static List<Vector2> GetPositionListCircle(Vector2 startPos, float[] dist, int[] posCount)
    {
        List<Vector2> positions = new List<Vector2>();
        positions.Add(startPos);

        for (int i = 0; i < dist.Length; i++)
        {
            positions.AddRange(GetPositionListCircle(startPos, dist[i], posCount[i]));
        }

        return positions;
    }

    private static List<Vector2> GetPositionListCircle(Vector2 startPos, float dist, int posCount)
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