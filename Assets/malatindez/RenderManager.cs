using UnityEngine;

[RequireComponent(typeof(Camera))]
public class RenderManager : MonoBehaviour
{
    // TODO: use depth map from environment renderer in the main renderer, remove environment from main renderer.
    // This will improve render performance by TWO times.

    [SerializeField] private Camera _mainRenderer;
    [SerializeField] private Camera _environmentRenderer;
    [SerializeField] private Camera _environmentDepthRenderer;
    [SerializeField] private Camera _mapRenderer;
    [SerializeField] private RenderTexture _mainRenderTexture;
    [SerializeField] private RenderTexture _environmentRenderTexture;
    [SerializeField] private RenderTexture _environmentRenderDepthTexture;
    [SerializeField] private RenderTexture _mapDepthTexture;
    [SerializeField] private VisibilityManager _visibilityManager;
    [SerializeField] private ComputeShader _UVPrepassShader;
    private RenderTexture _UVPrepassTexture = null;
    private Camera _displayCamera;
    // Start is called before the first frame update
    private void Start()
    {
        _displayCamera = GetComponent<Camera>();
        _UVPrepassTexture = new RenderTexture(16, 16, UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16_SFloat, UnityEngine.Experimental.Rendering.GraphicsFormat.None)
        {
            enableRandomWrite = true
        };
    }
    private static bool UpdateTextureIfResolutionChanged(RenderTexture texture)
    {
        if (texture.width != Screen.width || texture.height != Screen.height)
        {
            if (texture == null)
            {
                texture = new RenderTexture(Screen.width, Screen.height, 24);
            }
            else
            {
                texture.Release();
                texture.width = Screen.width;
                texture.height = Screen.height;
            }

            _ = texture.Create();
            return true;
        }
        return false;
    }

    private static void UpdateTextureIfResolutionChanged(ref RenderTexture texture, Camera targetCamera)
    {
        if(UpdateTextureIfResolutionChanged(texture))
            targetCamera.targetTexture = texture;
    }

    private static void UpdateDepthTextureIfResolutionChanged(ref RenderTexture texture, Camera targetCamera)
    {
        if (texture.width != Screen.width || texture.height != Screen.height)
        {
            if (texture == null)
            {
                texture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.RFloat);
            }
            else
            {
                texture.Release();
                texture.width = Screen.width;
                texture.height = Screen.height;
            }
            texture.Create();
            targetCamera.targetTexture = texture;
        }
    }

    private void OnPreRender()
    {
    }
    
    private void LateUpdate()
    {
        MainCameraRenderer.environmentTexture = _environmentRenderTexture;
        MainCameraRenderer.environmentDepthTexture = _environmentRenderDepthTexture;
        MainCameraRenderer.worldTexture = _mainRenderTexture;
        MainCameraRenderer.visibilityFogOfWarTexture = _visibilityManager.FogOfWarMask;
        MainCameraRenderer.visibilityTexture = _visibilityManager.VisibilityMask;

        MainCameraRenderer.PerspectiveAspectRatio = _environmentDepthRenderer.aspect;
        MainCameraRenderer.OrthoAspectRatio = _mapRenderer.aspect;

        Matrix4x4 viewMatrix = _environmentDepthRenderer.worldToCameraMatrix;
        Matrix4x4 projectionMatrix = _environmentDepthRenderer.projectionMatrix;

        Matrix4x4 gpuProjectionMatrix = GL.GetGPUProjectionMatrix(_environmentDepthRenderer.projectionMatrix, true);
        Matrix4x4 gpuProjectionMatrixF = GL.GetGPUProjectionMatrix(_environmentDepthRenderer.projectionMatrix, false);

        Matrix4x4 viewProjectionMatrix = gpuProjectionMatrixF * viewMatrix;
        Matrix4x4 inverseViewProjectionMatrix = viewProjectionMatrix.inverse;

        Matrix4x4 orthoViewMatrix = _mapRenderer.worldToCameraMatrix;
        Matrix4x4 orthoProjectionMatrix = _mapRenderer.projectionMatrix;


        Matrix4x4 orthoViewProjectionMatrix = orthoProjectionMatrix * orthoViewMatrix;

        _UVPrepassShader.SetMatrix("_InvViewProj", inverseViewProjectionMatrix);

        _UVPrepassShader.SetTexture(0, "_EnvironmentTex", _environmentRenderTexture);
        _UVPrepassShader.SetTexture(0, "_EnvironmentDepthTex", _environmentRenderDepthTexture);

        _UVPrepassShader.SetMatrix("_InvPerspectiveView", viewMatrix.inverse);
        _UVPrepassShader.SetMatrix("_InvPerspectiveProj", gpuProjectionMatrixF.inverse);
        _UVPrepassShader.SetMatrix("_InvPerspectiveViewProj", inverseViewProjectionMatrix);
        _UVPrepassShader.SetMatrix("_OrthoViewProj", orthoViewProjectionMatrix);

        _UVPrepassShader.SetFloat("_PerspectiveAspectRatio", MainCameraRenderer.PerspectiveAspectRatio);
        _UVPrepassShader.SetFloat("_OrthoAspectRatio", MainCameraRenderer.OrthoAspectRatio);
        
        _UVPrepassShader.SetFloat("_zNear", _environmentDepthRenderer.nearClipPlane);
        _UVPrepassShader.SetFloat("_zFar", _environmentDepthRenderer.farClipPlane);

        _UVPrepassShader.SetVector("_CameraPosition", _environmentDepthRenderer.transform.position);

        _UVPrepassShader.SetVector("_ImageDimensions", new Vector2(_UVPrepassTexture.width, _UVPrepassTexture.height));
        
        _UVPrepassShader.SetTexture(0, "Result", _UVPrepassTexture);

        _UVPrepassShader.Dispatch(0, _UVPrepassTexture.width / 8, _UVPrepassTexture.height / 8, 1);

        MainCameraRenderer.UVPrepassTexture = _UVPrepassTexture;

        UpdateTextureIfResolutionChanged(ref _mainRenderTexture, _mainRenderer);
        UpdateTextureIfResolutionChanged(ref _environmentRenderTexture, _environmentRenderer);
        UpdateDepthTextureIfResolutionChanged(ref _environmentRenderDepthTexture, _environmentDepthRenderer);

        if(UpdateTextureIfResolutionChanged(_UVPrepassTexture))
        {
            _UVPrepassTexture.enableRandomWrite = true;
        }
        _UVPrepassTexture.Create();
    }
}
