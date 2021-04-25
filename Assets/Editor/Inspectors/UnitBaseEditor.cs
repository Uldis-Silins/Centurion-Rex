using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Unit_Base))]
public class UnitBaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
    }
}