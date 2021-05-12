
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class Lighting
{
    private const int maxDirLightCount = 4;

    private static int
        dirLightCountId = Shader.PropertyToID("_DirectionalLightCount"),
        dirLightColorsId = Shader.PropertyToID("_DirectionalLightColors"),
        dirLightDirectionsId = Shader.PropertyToID("_DirectionalLightDirections");
    
    static Vector4[]
        dirLightColors = new Vector4[maxDirLightCount],
        dirLightDirections = new Vector4[maxDirLightCount];
        
    
    private const string bufferName = "Lighting";
    CommandBuffer buffer = new CommandBuffer()
    {
        name = bufferName
    };

    CullingResults cullingResults;

    public void Setup(ScriptableRenderContext context, CullingResults cullingResults)
    {
        this.cullingResults = cullingResults;
        buffer.BeginSample(bufferName);
        SetupLights();
        buffer.EndSample(bufferName);
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }


    void SetupDirectionalLight(int index, ref VisibleLight visibleLight)
    {
        /*
        Light light = RenderSettings.sun;
        buffer.SetGlobalVector(dirLightColorId, light.color.linear * light.intensity);
        buffer.SetGlobalVector(dirLightDirectionId, -light.transform.forward);
        */
        dirLightColors[index] = visibleLight.finalColor;
        // The forward vector can be found via the VisibleLight.localToWorldMatrix property.
        // It's the third column of the matrix and once again has to be negated.
        dirLightDirections[index] = -visibleLight.localToWorldMatrix.GetColumn(2);
    }


    void SetupLights()
    {
        // cullingResults.visibleLights貌似是按照灯光的重要性降序排列的？
        NativeArray<VisibleLight> visibleLights = cullingResults.visibleLights;
        int dirLightCount = 0;
        for (int i = 0; i < visibleLights.Length; i++)
        {
            VisibleLight visibleLight = visibleLights[i];
            if (visibleLight.lightType == LightType.Directional)
            {
                SetupDirectionalLight(dirLightCount++, ref visibleLight);
                if (dirLightCount >= maxDirLightCount)
                {
                    Debug.LogFormat("count of directional lights reach max! {0}>={1}", visibleLights.Length, maxDirLightCount);
                    break;
                }
            }
        }
        
        buffer.SetGlobalInt(dirLightCountId, visibleLights.Length);
        buffer.SetGlobalVectorArray(dirLightColorsId, dirLightColors);
        buffer.SetGlobalVectorArray(dirLightDirectionsId, dirLightDirections);        
    }
    
}
