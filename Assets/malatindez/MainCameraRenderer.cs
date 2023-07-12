using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MainCameraRenderer : ScriptableRendererFeature
{
    public static RenderTexture environmentTexture { get; set; }
    public static RenderTexture environmentDepthTexture { get; set; }
    public static RenderTexture worldTexture { get; set; }
    public static RenderTexture mainViewMaskTexture { get; set; }
    public static Matrix4x4 InvPerspectiveViewProj { get; set; } = Matrix4x4.identity;
    public static Matrix4x4 OrthoViewProj { get; set; } = Matrix4x4.identity;
    
    private class CustomRenderPass : ScriptableRenderPass
    {
        public Material material = null;
        private RenderTargetHandle tempTargetHandle;
        private readonly int _InvViewProjID = Shader.PropertyToID("_InvViewProj");
        private readonly int _OrthoViewProjID = Shader.PropertyToID("_OrthoViewProj");
        private readonly int _ImageDimensionsID = Shader.PropertyToID("_ImageDimensions");
        private readonly int _EnvironmentTexID = Shader.PropertyToID("_EnvironmentTex");
        private readonly int _EnvironmentDepthTexID = Shader.PropertyToID("_EnvironmentDepthTex");
        private readonly int _WorldTexID = Shader.PropertyToID("_WorldTex");
        private readonly int _MainViewMaskID = Shader.PropertyToID("_MainViewMask");

        public CustomRenderPass()
        {
            var t = Shader.Find("Unlit/MainCameraRenderer");
            if (t == null)
            {
                Debug.LogWarningFormat("Missing shader. Main Camera render pass will not execute. Check for missing reference in the renderer resources.");
                return;
            }
            material = new Material(t);
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            cmd.GetTemporaryRT(tempTargetHandle.id, cameraTextureDescriptor);
            ConfigureTarget(tempTargetHandle.Identifier());
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if(material == null)
            {
                return;
            }
            CommandBuffer cmd = CommandBufferPool.Get("VisibilityPass");

            RenderTargetIdentifier source = renderingData.cameraData.renderer.cameraColorTarget;

            material.SetMatrix(_InvViewProjID, InvPerspectiveViewProj);
            material.SetMatrix(_OrthoViewProjID, OrthoViewProj);
            material.SetVector(_ImageDimensionsID, new Vector2(environmentTexture.width, environmentTexture.height));

            material.SetTexture(_EnvironmentTexID, environmentTexture);
            material.SetTexture(_EnvironmentDepthTexID, environmentDepthTexture);
            material.SetTexture(_WorldTexID, worldTexture);
            material.SetTexture(_MainViewMaskID, mainViewMaskTexture);

            // Apply the shader pass
            cmd.Blit(source, tempTargetHandle.Identifier(), material);

            // Blit the temporary render texture back to the camera target
            cmd.Blit(tempTargetHandle.Identifier(), source);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(tempTargetHandle.id);
        }
    }

    private CustomRenderPass m_ScriptablePass;

    public override void Create()
    {
        m_ScriptablePass = new CustomRenderPass()
        {
            renderPassEvent = RenderPassEvent.AfterRendering
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }
}
