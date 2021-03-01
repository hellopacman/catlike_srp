using UnityEngine;
using UnityEngine.Rendering;

public class CameraRenderer
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

		if (!Cull())
		{
			return;
		}
			
		Setup();
		DrawVisibleGeometry();
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

		buffer.ClearRenderTarget(true, true, Color.clear);

		buffer.BeginSample(bufferName);
		ExecuteBuffer();
		
	}

	void Submit()
	{
		buffer.EndSample(bufferName);
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