using UnityEditor;
using UnityEngine;

public class MashBoxChallengeSetup : EditorWindow
{
    // Name for the race folder under "Race"
    private string raceName = "Race Track";

    // Internal bookkeeping for next checkpoint index
    private int nextCheckpointIndex = 1;

    // Desired scale for the start/checkpoint objects
    private Vector3 checkpointScale = new Vector3(4f, 4f, 0.125f);

    // (Optional) A field to let the user pick a material from the Project.
    private Material defaultCheckpointMaterial;

    // Add a top-level "Mash Box -> Challenge Setup" menu item that opens this window
    [MenuItem("Mash Box/Challenge Setup")]
    public static void ShowWindow()
    {
        var window = GetWindow<MashBoxChallengeSetup>("Mash Box Setup");
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("Race Setup", EditorStyles.boldLabel);

        // Text field for specifying the Race Name
        raceName = EditorGUILayout.TextField("Race Name", raceName);

        // Let the user adjust the scale if needed
        checkpointScale = EditorGUILayout.Vector3Field("Checkpoint Scale", checkpointScale);

        // Let the user assign or pick a default material
        defaultCheckpointMaterial = (Material)EditorGUILayout.ObjectField(
            "Default Material",
            defaultCheckpointMaterial,
            typeof(Material),
            false
        );

        EditorGUILayout.Space(10f);

        // Button to create (or find) the top-level objects: "Challenges", "Race", and "[raceName]"
        if (GUILayout.Button("Initialize Race Hierarchy"))
        {
            InitializeHierarchy();
        }

        // Button to create or find the "Start Point"
        if (GUILayout.Button("Create/Find Start Point"))
        {
            CreateStartPoint();
        }

        // Next Checkpoint creation
        if (GUILayout.Button("Create Next Checkpoint"))
        {
            CreateNextCheckpoint();
        }
    }

    private void InitializeHierarchy()
    {
        // Make sure we have "Challenges"
        GameObject challengesGO = FindOrCreateRoot("Challenges");

        // Under "Challenges", make sure we have "Race"
        GameObject raceGO = FindOrCreateChild(challengesGO.transform, "Race");

        // Under "Race", make sure we have our [raceName] object
        GameObject userRaceGO = FindOrCreateChild(raceGO.transform, raceName);

        Debug.Log($"Hierarchy ensured: Challenges -> Race -> {raceName}");
    }

    private void CreateStartPoint()
    {
        GameObject raceFolder = EnsureFolders();
        if (raceFolder == null) return;

        // Find or create "Start Point"
        GameObject startPoint = FindOrCreateChild(raceFolder.transform, "Start Point");
        startPoint.transform.localScale = checkpointScale;

        Debug.Log("Start Point created or found under " + raceFolder.name);
    }

    private void CreateNextCheckpoint()
    {
        // Ensure the needed hierarchy
        GameObject raceFolder = EnsureFolders();
        if (raceFolder == null) return;

        // Build a name for the new checkpoint
        string checkpointName = "Checkpoint " + nextCheckpointIndex;

        // Create or find the new checkpoint object
        GameObject checkpointGO = FindOrCreateChild(raceFolder.transform, checkpointName);

        // If there's a "previous" checkpoint, copy its position/rotation
        if (nextCheckpointIndex > 1)
        {
            string lastCheckpointName = "Checkpoint " + (nextCheckpointIndex - 1);
            Transform lastCheckpoint = raceFolder.transform.Find(lastCheckpointName);
            if (lastCheckpoint != null)
            {
                // Position the new checkpoint in the exact same spot
                checkpointGO.transform.position = lastCheckpoint.position;
                checkpointGO.transform.rotation = lastCheckpoint.rotation;
            }
        }

        // Scale and add the cube mesh
        checkpointGO.transform.localScale = checkpointScale;
        AddCubeMesh(checkpointGO);

        // (NEW) Make this new checkpoint selected in the Hierarchy
        Selection.activeGameObject = checkpointGO;

        // Increment the index for the next one
        nextCheckpointIndex++;

        Debug.Log($"Created or found {checkpointName} under {raceFolder.name}");
    }

    /// <summary>
    /// Ensures the given GameObject has:
    ///   - MeshRenderer
    ///   - MeshFilter with a cube mesh
    ///   - BoxCollider
    ///   - A default or user-selected Material
    /// </summary>
    private void AddCubeMesh(GameObject go)
    {
        // Add MeshRenderer if missing
        MeshRenderer meshRenderer = go.GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = go.AddComponent<MeshRenderer>();
        }

        // Add MeshFilter if missing
        MeshFilter meshFilter = go.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            meshFilter = go.AddComponent<MeshFilter>();
        }

        // If no mesh is assigned, grab one from a temporary primitive
        if (meshFilter.sharedMesh == null)
        {
            GameObject tempCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            MeshFilter tempMeshFilter = tempCube.GetComponent<MeshFilter>();
            meshFilter.sharedMesh = tempMeshFilter.sharedMesh;
            DestroyImmediate(tempCube);
        }

        // Assign material (either the user-provided one or a simple default)
        if (meshRenderer.sharedMaterial == null)
        {
            if (defaultCheckpointMaterial != null)
            {
                meshRenderer.sharedMaterial = defaultCheckpointMaterial;
            }
            else
            {
                // Create a basic Standard material as fallback
                meshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));
                meshRenderer.sharedMaterial.name = "DefaultCheckpointMaterial";
            }
        }

        // (Optional) add a BoxCollider
        BoxCollider boxCollider = go.GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            boxCollider = go.AddComponent<BoxCollider>();
        }
    }

    /// <summary>
    /// Ensures we have the hierarchy up to [raceName] folder, returns it.
    /// </summary>
    private GameObject EnsureFolders()
    {
        // 1) Challenges
        GameObject challengesGO = FindOrCreateRoot("Challenges");
        // 2) Race (child of Challenges)
        GameObject raceGO = FindOrCreateChild(challengesGO.transform, "Race");
        // 3) [raceName] (child of Race)
        GameObject userRaceGO = FindOrCreateChild(raceGO.transform, raceName);
        return userRaceGO;
    }

    private GameObject FindOrCreateRoot(string rootName)
    {
        GameObject rootGO = GameObject.Find(rootName);
        if (!rootGO)
        {
            rootGO = new GameObject(rootName);
            Undo.RegisterCreatedObjectUndo(rootGO, "Create " + rootName);
        }
        return rootGO;
    }

    private GameObject FindOrCreateChild(Transform parent, string childName)
    {
        Transform t = parent.Find(childName);
        if (t != null)
            return t.gameObject;

        GameObject newChild = new GameObject(childName);
        newChild.transform.SetParent(parent, false);
        Undo.RegisterCreatedObjectUndo(newChild, "Create " + childName);
        return newChild;
    }
}
