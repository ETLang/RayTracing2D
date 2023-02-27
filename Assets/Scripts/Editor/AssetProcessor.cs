using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

class RT2DAssetPostprocessor : AssetPostprocessor
{
    void OnPostprocessMaterial(Material material)
    {
        material.SetInt("_MaterialIndex", material.GetInstanceID());
    }
}