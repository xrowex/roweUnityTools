using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.HighDefinition;
using System.Collections.Generic;
using System.IO;

public class HDRPMaterialCreator : EditorWindow
{
    [MenuItem("Rowe/Tools/Create HDRP Lit Materials from Selection")]
    static void CreateMaterials()
    {
        var selected = Selection.objects;
        Dictionary<string, MaterialData> materials = new();

        foreach (var obj in selected)
        {
            if (obj is Texture texture)
            {
                string path = AssetDatabase.GetAssetPath(texture);
                string filename = Path.GetFileNameWithoutExtension(path);
                string folder = Path.GetDirectoryName(path);

                // Get prefix like LedgesSubstance_crete
                string key = filename;
                if (filename.Contains("_BaseMap"))
                    key = filename.Replace("_BaseMap", "");
                else if (filename.Contains("_MaskMap"))
                    key = filename.Replace("_MaskMap", "");
                else if (filename.Contains("_Normal"))
                    key = filename.Replace("_Normal", "");

                if (!materials.ContainsKey(key))
                    materials[key] = new MaterialData { folderPath = folder };

                if (filename.Contains("_BaseMap"))
                    materials[key].baseMap = texture;
                else if (filename.Contains("_MaskMap"))
                    materials[key].maskMap = texture;
                else if (filename.Contains("_Normal"))
                    materials[key].normalMap = texture;
            }
        }

        foreach (var kvp in materials)
        {
            string name = kvp.Key;
            var data = kvp.Value;

            Material mat = new(Shader.Find("HDRP/Lit"));
            if (data.baseMap != null)
                mat.SetTexture("_BaseColorMap", data.baseMap);
            if (data.maskMap != null)
                mat.SetTexture("_MaskMap", data.maskMap);
            if (data.normalMap != null)
            {
                mat.SetTexture("_NormalMap", data.normalMap);
                mat.EnableKeyword("_NORMALMAP");
            }

            string matPath = Path.Combine(data.folderPath, name + ".mat").Replace("\\", "/");
            AssetDatabase.CreateAsset(mat, matPath);
            Debug.Log($"Created HDRP Material: {matPath}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private class MaterialData
    {
        public Texture baseMap;
        public Texture maskMap;
        public Texture normalMap;
        public string folderPath;
    }
}
