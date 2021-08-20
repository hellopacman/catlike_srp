using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public partial class CameraRenderer
{
    private const string s_bufferName = "CustomCamRendererCmdBuffer";
    private static ShaderTagId s_unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
    
    
    private ScriptableRenderContext _context;
    private Camera _camera;
    private CommandBuffer _buffer = new CommandBuffer()
    {
        name = s_bufferName
    };

    private CullingResults _cullingResults; 
    

    public void Render(ScriptableRenderContext context, Camera camera)
    {
        _context = context;
        _camera = camera;
    
        PrepareBuffer();
        PrepareForSceneWindow();
        if (!Cull())
            return;
        
        Setup();
        DrawVisibleGeometry();
        DrawUnsupportedShaders();
        DrawGizmos();
        Submit();        
    }

    void Setup()
    {
        // 先SetupCameraProperties再ClearRenderTarget，这样底层会调用Clear()。否则会调用Draw GL，全屏Quad+Hidden/InternalClear，较慢
        _context.SetupCameraProperties(_camera);
        CameraClearFlags flags = _camera.clearFlags;
        _buffer.ClearRenderTarget(flags <= CameraClearFlags.Depth, 
            flags == CameraClearFlags.Color, 
            flags == CameraClearFlags.Color ? _camera.backgroundColor.linear : Color.clear);
        _buffer.BeginSample(sampleName);
        ExcuteBuffer();
    }
    
    
    void DrawVisibleGeometry()
    {
        // opaque
        SortingSettings sortingSettings = new SortingSettings(_camera)
        {
            criteria = SortingCriteria.CommonOpaque
        };
        DrawingSettings drawingSettings = new DrawingSettings(s_unlitShaderTagId, sortingSettings);
        // If you call new FilteringSettings() without any parameters, Unity creates an empty FilteringSettings struct.
        // An empty struct contains no data and all internal values default to 0; for example, it has a layerMask value of 0, and so on.
        // Unless you overwrite its properties, this empty struct tells Unity to exclude all objects.
        FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
        _context.DrawRenderers(_cullingResults, ref drawingSettings, ref filteringSettings);
        
        // skybox
        _context.DrawSkybox(_camera);

        // transparent
        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;
        _context.DrawRenderers(_cullingResults, ref drawingSettings, ref filteringSettings);

    }


    void Submit()
    {
        _buffer.EndSample(sampleName);
        ExcuteBuffer();
        _context.Submit();
    }

    void ExcuteBuffer()
    {
        _context.ExecuteCommandBuffer(_buffer);
        _buffer.Clear();
    }

    bool Cull()
    {
        if (_camera.TryGetCullingParameters(out ScriptableCullingParameters p))
        {
            _cullingResults = _context.Cull(ref p);
            return true;
        }

        return false;
    }

}
