using UnityEngine;
using UnityEditor;
using UnityEditor.Unwrapping;
using System.Collections.Generic;
using System.Diagnostics;
using System;

public class LightmapUVGenerator : EditorWindow
{
    private float angleError = 8f;
    private float areaError = 15f;
    private float hardAngle = 88f;
    private float packMargin = 0.003f;

    [MenuItem("Rowe/Tools/Generate Lightmap UVs")]
    public static void ShowWindow()
    {
        GetWindow<LightmapUVGenerator>("Lightmap UV Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Lightmap UV Generation", EditorStyles.boldLabel);

        angleError = EditorGUILayout.Slider("Angle Error", angleError, 1f, 75f);
        areaError = EditorGUILayout.Slider("Area Error", areaError, 1f, 75f);
        hardAngle = EditorGUILayout.Slider("Hard Angle", hardAngle, 1f, 180f);
        packMargin = EditorGUILayout.Slider("Pack Margin", packMargin, 0f, 0.1f);

        if (GUILayout.Button("Generate UVs for Selected"))
        {
            GenerateUVs();
        }
    }

    private void GenerateUVs()
    {
        Object[] selectedObjects = Selection.GetFiltered(typeof(GameObject), SelectionMode.Editable | SelectionMode.Deep);

        int count = 0;
        foreach (Object obj in selectedObjects)
        {
            GameObject go = obj as GameObject;
            if (go == null) continue;

            MeshFilter[] meshFilters = go.GetComponentsInChildren<MeshFilter>();
            foreach (MeshFilter mf in meshFilters)
            {
                Mesh mesh = mf.sharedMesh;
                if (mesh == null) continue;

                UnwrapParam param = new UnwrapParam
                {
                    angleError = angleError,
                    areaError = areaError,
                    hardAngle = hardAngle,
                    packMargin = packMargin
                };

                Unwrapping.GenerateSecondaryUVSet(mesh, param);
                count++;
            }
        }

        Debug.Log($"[RoweMod] Generated lightmap UVs for {count} mesh(es).");
        AssetDatabase.SaveAssets();
    }
}
