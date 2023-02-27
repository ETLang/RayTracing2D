using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class DebugStuff
{
    static string GetTextureString(Texture tex)
    {
        if (tex == null)
            return "<null>";

        return $"{tex.name} ({tex.width} x {tex.height})";
    }

    [MenuItem("RT2D/Debug/Print Material Properties")]
    static void PrintMaterialProperties()
    {
        var mat = Selection.activeObject as Material;

        if (mat == null)
            return;

        int propCount = mat.shader.GetPropertyCount();

        var sb = new StringBuilder();

        sb.AppendLine($"Material Properties '{mat.name}':");
        for (int i = 0; i < propCount; i++)
        {
            var name = mat.shader.GetPropertyName(i);
            var type = mat.shader.GetPropertyType(i);

            sb.Append($"  {name} - ");

            switch(type)
            {
                case UnityEngine.Rendering.ShaderPropertyType.Color:
                    sb.Append($"Color {mat.GetColor(name)}");
                    break;
                case UnityEngine.Rendering.ShaderPropertyType.Float:
                case UnityEngine.Rendering.ShaderPropertyType.Range:
                    sb.Append($"Float {mat.GetFloat(name)}");
                    break;
                case UnityEngine.Rendering.ShaderPropertyType.Texture:
                    sb.Append($"Texture {GetTextureString(mat.GetTexture(name))}");
                    break;
                case UnityEngine.Rendering.ShaderPropertyType.Vector:
                    sb.Append($"Vector {mat.GetVector(name)}");
                    break;
                default:
                    sb.Append("<Unknown Property Type>");
                    break;
            }

            sb.AppendLine();
        }

        Debug.Log(sb);
    }
}
