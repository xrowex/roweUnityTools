using UnityEngine;
using UnityEditor;

public class TerrainMaskMapPacker : EditorWindow
{
    Texture2D metallicTex;
    Texture2D aoTex;
    Texture2D heightTex;
    Texture2D smoothnessTex;

    [MenuItem("Tools/Rowe//Terrain Mask Map Packer")]
    static void Init()
    {
        TerrainMaskMapPacker window = (TerrainMaskMapPacker)GetWindow(typeof(TerrainMaskMapPacker));
        window.titleContent = new GUIContent("Terrain Mask Map Packer");
        window.minSize = new Vector2(340, 200);
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Assign Terrain Textures", EditorStyles.boldLabel);

        metallicTex = (Texture2D)EditorGUILayout.ObjectField("Metallic (R)", metallicTex, typeof(Texture2D), false);
        aoTex = (Texture2D)EditorGUILayout.ObjectField("Ambient Occlusion (G)", aoTex, typeof(Texture2D), false);
        heightTex = (Texture2D)EditorGUILayout.ObjectField("Height Map (B)", heightTex, typeof(Texture2D), false);
        smoothnessTex = (Texture2D)EditorGUILayout.ObjectField("Smoothness (A)", smoothnessTex, typeof(Texture2D), false);

        GUILayout.Space(10);
        if (GUILayout.Button("Generate Terrain Mask Map"))
        {
            GenerateTerrainMaskMap();
        }
    }

    void GenerateTerrainMaskMap()
    {
        if (metallicTex == null || aoTex == null || heightTex == null || smoothnessTex == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign all textures.", "OK");
            return;
        }

        int width = metallicTex.width;
        int height = metallicTex.height;

        if (aoTex.width != width || heightTex.width != width || smoothnessTex.width != width ||
            aoTex.height != height || heightTex.height != height || smoothnessTex.height != height)
        {
            EditorUtility.DisplayDialog("Error", "All textures must have the same dimensions.", "OK");
            return;
        }

        Texture2D terrainMaskMap = new Texture2D(width, height, TextureFormat.RGBA32, false, true);

        Color[] metallicPixels = metallicTex.GetPixels();
        Color[] aoPixels = aoTex.GetPixels();
        Color[] heightPixels = heightTex.GetPixels();
        Color[] smoothnessPixels = smoothnessTex.GetPixels();

        for (int i = 0; i < metallicPixels.Length; i++)
        {
            Color packedPixel = new Color(
                metallicPixels[i].r,
                aoPixels[i].r,
                heightPixels[i].r,
                smoothnessPixels[i].r
            );

            terrainMaskMap.SetPixel(i % width, i / width, packedPixel);
        }

        terrainMaskMap.Apply();

        string path = EditorUtility.SaveFilePanelInProject(
            "Save Terrain Mask Map",
            "TerrainMaskMap",
            "png",
            "Select save location for Terrain Mask Map."
        );

        if (!string.IsNullOrEmpty(path))
        {
            System.IO.File.WriteAllBytes(path, terrainMaskMap.EncodeToPNG());
            AssetDatabase.Refresh();

            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.sRGBTexture = false;
            importer.SaveAndReimport();

            EditorUtility.DisplayDialog("Success", "Terrain Mask Map successfully created!", "OK");
        }
    }
}
