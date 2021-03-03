using UnityEngine;
using UnityEngine.Rendering;

public partial class CameraRenderer
{
	static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
	
	private const string bufferName = "Render Camera";
	CommandBuffer buffer = new CommandBuffer
	{
		name = bufferName
	};
	
	ScriptableRenderContext context;
	Camera camera;
	CullingResults cullingResults;

	public void Render(ScriptableRenderContext context, Camera camera)
	{
		this.context = context;
		this.camera = camera;
		
		PrepareBuffer();
		PrepareForSceneWindow();
		
		if (!Cull())
		{
			return;
		}
			
		Setup();
		DrawVisibleGeometry();
		DrawUnsupportedShaders();
		DrawGizmos();
		Submit();
	}

	void DrawVisibleGeometry()
	{
		// opaque
		var sortingSettings = new SortingSettings(camera)
		{
			criteria = SortingCriteria.CommonOpaque
		};
		var drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings);
		var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
		context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);		
		
		// skybox
		context.DrawSkybox(camera);
		
		// trasparent
		sortingSettings.criteria = SortingCriteria.CommonTransparent;
		drawingSettings.sortingSettings = sortingSettings;
		filteringSettings.renderQueueRange = RenderQueueRange.transparent;
		context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);		
		
	}

	void Setup()
	{
		context.SetupCameraProperties(camera);
		CameraClearFlags flags = camera.clearFlags;
		buffer.ClearRenderTarget(flags <= CameraClearFlags.Depth, 
			flags <= CameraClearFlags.Color, 
			flags == CameraClearFlags.Color ? camera.backgroundColor.linear : Color.clear);
		buffer.BeginSample(SampleName);
		ExecuteBuffer();
		
	}

	void Submit()
	{
		buffer.EndSample(SampleName);
		ExecuteBuffer();

		context.Submit();
	}
	
	void ExecuteBuffer () {
		context.ExecuteCommandBuffer(buffer);
		buffer.Clear();
	}

	bool Cull ()
	{
		if (camera.TryGetCullingParameters(out ScriptableCullingParameters p))
		{
			cullingResults = context.Cull(ref p);
			return true;
		}
		return false;
	}

}