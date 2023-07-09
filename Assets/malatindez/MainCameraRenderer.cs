using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MainCameraRenderer : ScriptableRendererFeature
{
    public static RenderTexture environmentTexture { get; set; }
    public static RenderTexture environmentDepthTexture { get; set; }
    public static RenderTexture worldTexture { get; set; }
    public static RenderTexture visibilityFogOfWarTexture { get; set; }
    public static RenderTexture visibilityTexture { get; set; }
    public static Matrix4x4 orthoProj { get; set; } = Matrix4x4.identity;
    public static Matrix4x4 orthoView { get; set; } = Matrix4x4.identity;
    public static Matrix4x4 invMainProj { get; set; } = Matrix4x4.identity;
    public static Matrix4x4 invMainView { get; set; } = Matrix4x4.identity;
    public static Matrix4x4 MainProj { get; set; } = Matrix4x4.identity;
    public static Matrix4x4 MainView { get; set; } = Matrix4x4.identity;
    public static float PerspectiveAspectRatio { get; set; } = 1.0f;
    public static float OrthoAspectRatio { get; set; } = 1.0f;

    private class CustomRenderPass : ScriptableRenderPass
    {
        public Material material = null;
        private RenderTargetHandle tempTargetHandle;

        public CustomRenderPass()
        {
            material = new Material(Shader.Find("Unlit/MainCameraRenderer"));
            if (material == null)
            {
                Debug.LogErrorFormat("Missing Material. Main Camera render pass will not execute. Check for missing reference in the renderer resources.");
                return;
            }
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            cmd.GetTemporaryRT(tempTargetHandle.id, cameraTextureDescriptor);
            ConfigureTarget(tempTargetHandle.Identifier());
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("VisibilityPass");

            RenderTargetIdentifier source = renderingData.cameraData.renderer.cameraColorTarget;

            material.SetMatrix("_OrthoProj", orthoProj);
            material.SetMatrix("_OrthoView", orthoView);
            material.SetMatrix("_InvMainProj", invMainProj);
            material.SetMatrix("_InvMainView", invMainView);
            material.SetMatrix("_MainProj", MainProj);
            material.SetMatrix("_MainView", MainView);
            material.SetFloat("_PerspectiveAspectRatio", PerspectiveAspectRatio);
            material.SetFloat("_OrthoAspectRatio", OrthoAspectRatio);
            material.SetTexture("_EnvironmentTex", environmentTexture);
            material.SetTexture("_EnvironmentDepthTex", environmentDepthTexture);
            material.SetTexture("_WorldTex", worldTexture);
            material.SetTexture("_VisibilityFogOfWarTex", visibilityFogOfWarTexture);
            material.SetTexture("_VisibilityTex", visibilityTexture);

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
