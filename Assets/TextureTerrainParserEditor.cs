using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TextureTerrainParser))]
[CanEditMultipleObjects]
public class TextureTerrainEditor : Editor
{
    SerializedProperty terrainTexture;
    SerializedProperty resolution;

    private void OnEnable()
    {
        terrainTexture = serializedObject.FindProperty("terrainTexture");
        resolution = serializedObject.FindProperty("resolution");
    }

    public override void OnInspectorGUI()
    {
        TextureTerrainParser ttp = target as TextureTerrainParser;

        serializedObject.Update();

        EditorGUILayout.PropertyField(terrainTexture);
        EditorGUILayout.PropertyField(resolution);

        if (GUILayout.Button("Create Terrain"))
        {
            ttp.CreateTerrain();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
