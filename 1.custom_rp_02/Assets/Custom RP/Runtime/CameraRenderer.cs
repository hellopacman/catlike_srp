using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public partial class CameraRenderer
{
    private const string bufferName = "TheCameraRenderer";

    private CommandBuffer buffer = new CommandBuffer
    {
        name = bufferName
    };
    
    private ScriptableRenderContext context;
    private Camera camera;
    private CullingResults _cullingResults;

    public void Render(ScriptableRenderContext context, Camera camera)
    {
        this.context = context;
        this.camera = camera;
    }
}
