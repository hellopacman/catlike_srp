using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Profiling;

public partial class CameraRenderer
{
    partial void DrawGizmos();
    partial void DrawUnsupportedShaders();
    partial void PrepareForSceneWindow();
    partial void PrepareBuffer();
    
#if UNITY_EDITOR
    private string sampleName { get; set; }

    private static ShaderTagId[] s_legacyShaderTagIds =
    {
        new ShaderTagId("Always"),
        new ShaderTagId("ForwardBase"),
        new ShaderTagId("PrepassBase"),
        new ShaderTagId("Vertex"),
        new ShaderTagId("VertexLMRGBM"),
        new ShaderTagId("VertexLM")
    };

    
    private static Material s_errorMaterial;
    partial void DrawUnsupportedShaders()
    {
        if (s_errorMaterial == null)
        {
            s_errorMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"));
        }
        
        DrawingSettings drawingSettings = new DrawingSettings(s_legacyShaderTagIds[0], new SortingSettings(_camera))
        {
            overrideMaterial = s_errorMaterial
        };
        for (int i = 1; i < s_legacyShaderTagIds.Length; i++)
        {
            drawingSettings.SetShaderPassName(i, s_legacyShaderTagIds[i]);
        }
        
        FilteringSettings filteringSettings = FilteringSettings.defaultValue;
        _context.DrawRenderers(_cullingResults, ref drawingSettings, ref filteringSettings);
    }


    partial void DrawGizmos()
    {
        if (Handles.ShouldRenderGizmos())
        {
            _context.DrawGizmos(_camera, GizmoSubset.PreImageEffects);
            _context.DrawGizmos(_camera, GizmoSubset.PostImageEffects);
        }
    }

    partial void PrepareForSceneWindow()
    {
        if (_camera.cameraType == CameraType.SceneView)
        {
            ScriptableRenderContext.EmitWorldGeometryForSceneView(_camera);
        }
    }

    partial void PrepareBuffer()
    {
        Profiler.BeginSample("EditorOnly");
        _buffer.name = sampleName = _camera.name;
        Profiler.EndSample();
    }
#else
    const string sampleName = s_bufferName
    
#endif    
    
}
