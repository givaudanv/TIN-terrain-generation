using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TINMapGenerator))]
[CanEditMultipleObjects]
public class TINMapGeneratorEditor : Editor
{
    SerializedProperty resourcePath;
    SerializedProperty chunkPrefab;

    private void OnEnable()
    {
        resourcePath = serializedObject.FindProperty("resourcePath");
        chunkPrefab = serializedObject.FindProperty("chunkPrefab");
    }

    public override void OnInspectorGUI()
    {
        TINMapGenerator tmg = target as TINMapGenerator;
        serializedObject.Update();

        EditorGUILayout.PropertyField(resourcePath);
        EditorGUILayout.PropertyField(chunkPrefab);

        if (GUILayout.Button("Generate Map"))
        {
            tmg.GenerateMap();
        }

        if (GUILayout.Button("Clean Map"))
        {
            tmg.DeleteChildren();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
