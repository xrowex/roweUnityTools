using UnityEngine;
using UnityEditor;
using System.IO;

public class RoweBakeryGroupWindow : EditorWindow
{
    private int resolutionIndex = 2; // Default to 1024
    private readonly int[] resolutionOptions = new int[] { 256, 512, 1024, 2048, 4096 };
    private string[] resolutionLabels = new string[] { "256", "512", "1024", "2048", "4096" };

    [MenuItem("Rowe/Tools/Lightmap Group Creator")]
    public static void ShowWindow()
    {
        GetWindow<RoweBakeryGroupWindow>("Lightmap Groups");
    }

    private void OnGUI()
    {
        GUILayout.Label("Bakery Lightmap Group Setup", EditorStyles.boldLabel);
        GUILayout.Space(5);

        GUILayout.Label("Resolution");
        resolutionIndex = GUILayout.SelectionGrid(resolutionIndex, resolutionLabels, resolutionLabels.Length, EditorStyles.radioButton);

        GUILayout.Space(10);

        if (GUILayout.Button("Create Lightmap Groups from Selection"))
        {
            CreateGroups(resolutionOptions[resolutionIndex]);
        }
    }

    private static void CreateGroups(int selectedResolution)
    {
        string groupFolder = "Assets/BakeryGroups/";
        if (!AssetDatabase.IsValidFolder(groupFolder))
        {
            AssetDatabase.CreateFolder("Assets", "BakeryGroups");
        }
    
        foreach (var obj in Selection.gameObjects)
        {
            if (obj == null) continue;
    
            var groupSelector = obj.GetComponent<BakeryLightmapGroupSelector>();
            if (groupSelector == null)
                groupSelector = obj.AddComponent<BakeryLightmapGroupSelector>();
    
            groupSelector.instanceResolutionOverride = false; // important: preserve xatlas support
    
            string assetName = $"LMGroup_{obj.name}";
            string path = $"{groupFolder}{assetName}.asset";
    
            BakeryLightmapGroup groupAsset;
    
            if (groupSelector.lmgroupAsset == null)
            {
                groupAsset = ScriptableObject.CreateInstance<BakeryLightmapGroup>();
                groupAsset.resolution = selectedResolution;
    
                AssetDatabase.CreateAsset(groupAsset, path);
                groupSelector.lmgroupAsset = groupAsset;
    
                Debug.Log($"[RoweBakery] Created Bakery lightmap group asset for {obj.name} at {path}");
            }
            else
            {
                groupAsset = groupSelector.lmgroupAsset as BakeryLightmapGroup;
                if (groupAsset != null)
                {
                    groupAsset.resolution = selectedResolution;
                    EditorUtility.SetDirty(groupAsset);
                    Debug.Log($"[RoweBakery] Updated resolution for existing group asset on {obj.name}");
                }
            }
    
            EditorUtility.SetDirty(groupSelector);
        }
    
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

}
