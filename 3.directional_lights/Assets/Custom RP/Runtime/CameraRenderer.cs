using UnityEngine;
using UnityEngine.Rendering;

public partial class CameraRenderer
{
	static ShaderTagId 
		unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit"),
		litShaderTagId = new ShaderTagId("CustomLit");

	private const string bufferName = "Render Camera";
	CommandBuffer buffer = new CommandBuffer
	{
		name = bufferName
	};
	
	ScriptableRenderContext context;
	Camera camera;
	CullingResults cullingResults;
	Lighting lighting = new Lighting();

	public void Render(ScriptableRenderContext context, Camera camera,
		bool useDynamicBatching, bool useGPUInstancing)
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
		lighting.Setup(context, cullingResults);
		DrawVisibleGeometry(useDynamicBatching, useGPUInstancing);
		DrawUnsupportedShaders();
		DrawGizmos();
		Submit();
	}

	void DrawVisibleGeometry (bool useDynamicBatching, bool useGPUInstancing) {
		var sortingSettings = new SortingSettings(camera) {
			criteria = SortingCriteria.CommonOpaque
		};
		var drawingSettings = new DrawingSettings(
			unlitShaderTagId, sortingSettings
		) {
			enableDynamicBatching = useDynamicBatching,
			enableInstancing = useGPUInstancing
		};
		drawingSettings.SetShaderPassName(1, litShaderTagId);

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