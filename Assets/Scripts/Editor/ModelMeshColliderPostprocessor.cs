using UnityEditor;
using UnityEngine;

/// <summary>
/// Automatically adds MeshCollider to all mesh children when a model is imported.
/// Only applies to models in the "Models/Iteration 2" folder whose name contains "Level".
/// Adjust the path/name filter in ShouldProcess() as needed.
/// </summary>
public class ModelMeshColliderPostprocessor : AssetPostprocessor
{
    void OnPostprocessModel(GameObject root)
    {
        if (!ShouldProcess(assetPath))
            return;

        foreach (var meshFilter in root.GetComponentsInChildren<MeshFilter>())
        {
            if (meshFilter.GetComponent<MeshCollider>() == null)
            {
                meshFilter.gameObject.AddComponent<MeshCollider>();
            }
        }
    }

    bool ShouldProcess(string path)
    {
        // Only process models in the Iteration 2 folder that are level geometry
        return path.Contains("Models/Iteration 2") && path.Contains("Level");
    }
}
