using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(SpriteAnimatorData))]
public class SpriteAnimatorDataEditor : Editor
{
    private SerializedProperty m_animationsProperty;
    private ReorderableList m_animationsList;

    private void OnEnable()
    {
        m_animationsProperty = serializedObject.FindProperty("animations");
        m_animationsList = new ReorderableList(serializedObject, m_animationsProperty, true, true, true, true);

        m_animationsList.drawElementCallback += DrawElementHandler;
        m_animationsList.drawHeaderCallback += DrawHeaderHandler;
        m_animationsList.elementHeightCallback += ElementHeightHandler;
        //m_animationsList.onAddCallback += AddElementHandler;
        //m_animationsList.onRemoveCallback += RemoveElementHandler;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        m_animationsList.DoLayoutList();
        serializedObject.ApplyModifiedProperties();
    }

    private void DrawElementHandler(Rect rect, int index, bool isActive, bool isFocused)
    {
        SerializedProperty element = m_animationsList.serializedProperty.GetArrayElementAtIndex(index);

        Rect typeFieldRect = new Rect(rect.x, rect.y, rect.width * 0.2f, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(typeFieldRect, element.FindPropertyRelative("type"), GUIContent.none);

        //Rect speedModFieldRect = new Rect(typeFieldRect.x + typeFieldRect.width, rect.y, rect.width * 0.2f, EditorGUIUtility.singleLineHeight);
        //EditorGUI.PropertyField(speedModFieldRect, element.FindPropertyRelative("playbackSpeedModifier"), true);

        SerializedProperty frames = element.FindPropertyRelative("frames");

        Rect framesFieldRect = new Rect(typeFieldRect.x + typeFieldRect.width + 20.0f, rect.y, rect.width * 0.8f - 20.0f, EditorGUI.GetPropertyHeight(frames, true));
        EditorGUI.PropertyField(framesFieldRect, frames, true);
    }

    private void DrawHeaderHandler(Rect rect)
    {
        EditorGUI.LabelField(rect, "Animations");
    }

    private float ElementHeightHandler(int index)
    {
        SerializedProperty element = m_animationsList.serializedProperty.GetArrayElementAtIndex(index);
        float height = EditorGUIUtility.standardVerticalSpacing;

        SerializedProperty frames = element.FindPropertyRelative("frames");
        height += EditorGUI.GetPropertyHeight(frames, true) + EditorGUIUtility.standardVerticalSpacing;

        return height + EditorGUIUtility.standardVerticalSpacing;
    }

    private void AddElementHandler(ReorderableList list)
    {

    }

    private void RemoveElementHandler(ReorderableList list)
    {

    }
}