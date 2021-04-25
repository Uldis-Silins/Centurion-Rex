using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpriteAnimator))]
public class SpriteAnimatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SpriteAnimator spriteAnimator = target as SpriteAnimator;

        if (spriteAnimator.CurrentAnimationType != SpriteAnimatorData.AnimationType.None)
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
  
            if (GUILayout.Button("<"))
            {
                spriteAnimator.PlayPrevFrame();
            }

            var style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
            GUILayout.Label("frame: " + spriteAnimator.CurrentFrame.ToString(), style, GUILayout.ExpandWidth(true));

            if (GUILayout.Button(">"))
            {
                spriteAnimator.PlayNextFrame();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
    }
}
