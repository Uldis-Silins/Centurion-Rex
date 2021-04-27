using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SwizzleYZEditor : ScriptableObject
{
    [MenuItem("Tools/Swizzle YZ")]
    private static void SwizzleYZ()
    {
        for (int i = 0; i < Selection.objects.Length; i++)
        {
            GameObject go = Selection.objects[i] as GameObject;
            float y = go.transform.position.y;
            float z = go.transform.position.z;
            float t = y;
            y = z;
            z = t;
            go.transform.position = new Vector3(go.transform.position.x, y, z);
        }
    }
}
