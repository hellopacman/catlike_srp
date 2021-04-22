using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public partial class CameraRenderer
{
    // SRPDefaultUnlit and passes with no LightMode tag are also rendered by Universal Render Pipeline
    // 2021-4-22 那么为什么仅指定了SRPDefaultUnlit之后，builtin unlit/Color shader(没有任何lightMode tag)就能用了呢？
    // 是不是
    //     当没有在DrawingSettings指定任何shaderTag时，任何shader都没用的吧？
    //     当指定了SRPDefaultUnlit后， SRPDefaultUnlit和没有lightMode tag的shader就能用了?
    static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
    
    private const string bufferName = "MyCameraRenderer";

    private CommandBuffer buffer = new CommandBuffer
    {
        name = bufferName
    };

    private CullingResults cullingResults;
    
    private ScriptableRenderContext context;
    private Camera camera;
    private CullingResults _cullingResults;

    public void Render(ScriptableRenderContext context, Camera camera)
    {
        this.context = context;
        this.camera = camera;

        PrepareBuffer();
        PrepareForSceneWindow();  // As this might add geometry to the scene it has to be done before culling.
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
        // 如果在SetupCameraProperties之前执行ClearRenderTarget，会导致unity使用Hidden/InternalClear渲染全屏quad的方式去做clear，效率较低
        // 先执行SetupCameraProperties的话Unity才会使用Clear (color+Z+stencil)，the quick way
        context.SetupCameraProperties(camera);

        CameraClearFlags flags = camera.clearFlags;
        
        // ClearRenderTarget内部自带Begin/EndSample，奇怪的是它怎么知道bufferName的
        buffer.ClearRenderTarget(flags <= CameraClearFlags.Depth, 
            flags == CameraClearFlags.Color, 
            flags == CameraClearFlags.Color ? camera.backgroundColor.linear : Color.clear);
        buffer.BeginSample(SampleName);
        
        ExecuteBuffer();
    }

    bool Cull()
    {
        if (camera.TryGetCullingParameters(out ScriptableCullingParameters p))
        {
            cullingResults = context.Cull(ref p);
            return true;
        }

        return false;
    }
    
    
    void DrawVisibleGeometry()
    {
        // -- 不透明
        // The camera's transparency sort mode is used to determine whether to use orthographic or distance based sorting.
        var sortingSettings = new SortingSettings(camera)
        {
            criteria = SortingCriteria.CommonOpaque
        }; 
        
        var drawingSettings = new DrawingSettings(
            unlitShaderTagId, sortingSettings
            );
        var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
        
        context.DrawRenderers(
            cullingResults, ref drawingSettings, ref filteringSettings);
        
        
        // -- 天空球
        context.DrawSkybox(camera);
        
        // -- 透明
        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;
        context.DrawRenderers(
            cullingResults, ref drawingSettings, ref filteringSettings
            );
    }

    void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    void Submit()
    {
        buffer.EndSample(SampleName);
        ExecuteBuffer();
        context.Submit();
    }

    
}
