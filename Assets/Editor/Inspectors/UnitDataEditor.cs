using UnityEditor;
using UnityEngine;
using System.IO;

[CustomEditor(typeof(UnitData))]
public class UnitDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        UnitData data = target as UnitData;

        if (data.statsData == null)
        {
            if (GUILayout.Button("Create Stats"))
            {
                UnitStatsData asset = ScriptableObject.CreateInstance<UnitStatsData>();
                data.statsData = asset;

                string path = AssetDatabase.GetAssetPath(Selection.activeObject);
                string directory = Path.GetDirectoryName(path);
                string filename = Path.GetFileNameWithoutExtension(path) + "Data.asset";

                AssetDatabase.CreateAsset(asset, Path.Combine(directory, filename));
                AssetDatabase.SaveAssets();
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = asset;
            }
        }
        else
        {
            GUILayout.Space(10);
            GUILayout.Label("STATS:");

            SerializedObject statsObject = new SerializedObject(data.statsData);
            SerializedProperty statsProperty = statsObject.GetIterator();

            while (statsProperty.NextVisible(true))
            {
                string value = PropertyValueToString(statsProperty);

                if (!string.IsNullOrEmpty(value))
                {
                    GUILayout.Label(statsProperty.displayName + ": " + value);
                }
            }
        }
    }

    private string PropertyValueToString(SerializedProperty prop)
    {
        string propType = prop.type;

        if (propType == "float" || propType == "int" || propType == "double")
        {
            return prop.floatValue.ToString();
        }
        else if (propType == "string")
        {
            return prop.stringValue;
        }
        else if (propType == "bool")
        {
            return prop.boolValue.ToString();
        }

        return null;
    }
}