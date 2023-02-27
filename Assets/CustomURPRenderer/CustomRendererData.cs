using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[CreateAssetMenu(fileName = "CustomRenderer", menuName = "Create Custom Renderer", order = 50)]
public class CustomRendererData : ScriptableRendererData
{
    protected override ScriptableRenderer Create()
    {
        return new CustomRenderer(this);
    }
}