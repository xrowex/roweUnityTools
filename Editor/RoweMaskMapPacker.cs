using UnityEngine;
using UnityEditor;

public class MaskMapPacker : EditorWindow
{
    Texture2D metallicTex;
    Texture2D aoTex;
    Texture2D detailMaskTex;
    Texture2D smoothnessTex;

    [MenuItem("Tools/HDRP Mask Map Packer")]
    static void Init()
    {
        MaskMapPacker window = (MaskMapPacker)GetWindow(typeof(MaskMapPacker));
        window.titleContent = new GUIContent("Mask Map Packer");
        window.minSize = new Vector2(300, 180);
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Assign Textures to Channels", EditorStyles.boldLabel);

        metallicTex = (Texture2D)EditorGUILayout.ObjectField("Metallic (R)", metallicTex, typeof(Texture2D), false);
        aoTex = (Texture2D)EditorGUILayout.ObjectField("Ambient Occlusion (G)", aoTex, typeof(Texture2D), false);
        detailMaskTex = (Texture2D)EditorGUILayout.ObjectField("Detail Mask (B)", detailMaskTex, typeof(Texture2D), false);
        smoothnessTex = (Texture2D)EditorGUILayout.ObjectField("Smoothness (A)", smoothnessTex, typeof(Texture2D), false);

        if (GUILayout.Button("Generate Mask Map"))
        {
            GenerateMaskMap();
        }
    }

    void GenerateMaskMap()
    {
        if (metallicTex == null || aoTex == null || smoothnessTex == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign at least Metallic, AO, and Smoothness textures.", "OK");
            return;
        }

        int width = metallicTex.width;
        int height = metallicTex.height;

        Texture2D maskMap = new Texture2D(width, height, TextureFormat.RGBA32, false, true);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float r = metallicTex.GetPixel(x, y).r;
                float g = aoTex.GetPixel(x, y).r;
                float b = detailMaskTex ? detailMaskTex.GetPixel(x, y).r : 0f;
                float a = smoothnessTex.GetPixel(x, y).r;

                Color maskPixel = new Color(r, g, b, a);
                maskMap.SetPixel(x, y, maskPixel);
            }
        }

        maskMap.Apply();

        string path = EditorUtility.SaveFilePanelInProject("Save Mask Map", "MaskMap", "png", "Please enter file name to save the mask map texture.");

        if (!string.IsNullOrEmpty(path))
        {
            byte[] pngData = maskMap.EncodeToPNG();
            System.IO.File.WriteAllBytes(path, pngData);
            AssetDatabase.Refresh();

            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.sRGBTexture = false;
            importer.SaveAndReimport();

            EditorUtility.DisplayDialog("Success", "Mask Map created successfully!", "OK");
        }
    }
}
