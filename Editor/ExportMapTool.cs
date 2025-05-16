using System;
using UnityEditor;
using System.IO;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public static class ExportMapTool
{
    private const string ASSET_BUNDLES_BUILD_PATH = "AssetBundles";

    [MenuItem("BMX Streets/Quick Map Export")]
    public static void ExportMap()
    {
        ExportMap(null, EditorPrefs.GetBool("BMXStreets_UseVersionNumbering", true));
    }

    public static void ExportMap(string override_asset_bundle_name, bool use_version_numbering)
    {
        var scene = SceneManager.GetActiveScene();

        EditorSceneManager.SaveScene(scene);

        var bundle_name = scene.name;

        if (use_version_numbering)
        {
            var version = EditorPrefs.GetInt($"{scene.name}_version", 1);

            version++;

            EditorPrefs.SetInt($"{scene.name}_version", version);

            bundle_name = $"{scene.name}";
        }

        if (string.IsNullOrEmpty(override_asset_bundle_name) == false)
        {
            bundle_name = override_asset_bundle_name;
        }

        var build = new AssetBundleBuild
        {
            assetBundleName = bundle_name,
            assetNames = new[] { scene.path }
        };

        if (!Directory.Exists(ASSET_BUNDLES_BUILD_PATH))
            Directory.CreateDirectory(ASSET_BUNDLES_BUILD_PATH);

        BuildPipeline.BuildAssetBundles(ASSET_BUNDLES_BUILD_PATH, new[] { build }, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows);

        var map_dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "BMX Streets/Maps");
        var bundle_path = Path.Combine(Application.dataPath.Replace("/Assets", "/AssetBundles"), build.assetBundleName);
        var dest_path = Path.Combine(map_dir, build.assetBundleName);

        Debug.Log($"Copying {bundle_path} to {dest_path}");

        File.Copy(bundle_path, dest_path, overwrite: true);
        File.Delete(bundle_path);

        EditorSceneManager.OpenScene(scene.path);
    }

}