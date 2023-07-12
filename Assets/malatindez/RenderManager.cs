using System.Collections;
using Unity.Mathematics;
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
    [SerializeField] private Vector2 ScreenDowngrade = new Vector2(8, 8);
    private Camera _displayCamera;
    // Start is called before the first frame update
    private void Start()
    {
        _displayCamera = GetComponent<Camera>();
    }
    private bool UpdateTextureIfResolutionChanged(RenderTexture texture)
    {
        if (texture.width != Screen.width || texture.height != Screen.height)
        {
            if (texture == null)
            {
                texture = new RenderTexture((int)(Screen.width / ScreenDowngrade.x), (int)(Screen.height / ScreenDowngrade.x), 24)
                {
                    depthStencilFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R16_UNorm,
                    graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_SFloat,
                    anisoLevel = 0,
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp,
                    autoGenerateMips = false,
                    useMipMap = false,
                };
            }
            else
            {
                texture.Release();
                texture.width = (int)(Screen.width / ScreenDowngrade.x);
                texture.height = (int)(Screen.height / ScreenDowngrade.y);
                texture.anisoLevel = 0;
                texture.filterMode = FilterMode.Point;
                texture.autoGenerateMips = false;
                texture.wrapMode = TextureWrapMode.Clamp;
                texture.useMipMap = false;
            }

            _ = texture.Create();
            return true;
        }
        return false;
    }

    private void UpdateTextureIfResolutionChanged(ref RenderTexture texture, Camera targetCamera)
    {
        if(UpdateTextureIfResolutionChanged(texture))
            targetCamera.targetTexture = texture;
    }

    private void UpdateDepthTextureIfResolutionChanged(ref RenderTexture texture, Camera targetCamera)
    {
        if (texture.width != Screen.width || texture.height != Screen.height)
        {
            if (texture == null)
            {
                texture = new RenderTexture((int)(Screen.width / ScreenDowngrade.x), (int)(Screen.height / ScreenDowngrade.y), 0, RenderTextureFormat.RFloat)
                {
                    graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_SFloat,
                    anisoLevel = 0,
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp,
                    autoGenerateMips = false,
                    useMipMap = false,
                };
            }
            else
            {
                texture.Release();
                texture.width = (int)(Screen.width / ScreenDowngrade.x);
                texture.height = (int)(Screen.height / ScreenDowngrade.y);
                texture.anisoLevel = 0;
                texture.filterMode = FilterMode.Point;
                texture.autoGenerateMips = false;
                texture.wrapMode = TextureWrapMode.Clamp;
                texture.useMipMap = false;
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
        MainCameraRenderer.mainViewMaskTexture = _visibilityManager.MainViewMask;

        Matrix4x4 viewMatrix = _environmentDepthRenderer.worldToCameraMatrix;

        Matrix4x4 gpuProjectionMatrixF = GL.GetGPUProjectionMatrix(_environmentDepthRenderer.projectionMatrix, false);

        Matrix4x4 viewProjectionMatrix = gpuProjectionMatrixF * viewMatrix;
        Matrix4x4 inverseViewProjectionMatrix = viewProjectionMatrix.inverse;

        Matrix4x4 orthoViewMatrix = _mapRenderer.worldToCameraMatrix;
        Matrix4x4 orthoProjectionMatrix = _mapRenderer.projectionMatrix;

        Matrix4x4 orthoViewProjectionMatrix = orthoProjectionMatrix * orthoViewMatrix;

        MainCameraRenderer.InvPerspectiveViewProj = inverseViewProjectionMatrix;
        MainCameraRenderer.OrthoViewProj = orthoViewProjectionMatrix;

        UpdateTextureIfResolutionChanged(ref _mainRenderTexture, _mainRenderer);
        UpdateTextureIfResolutionChanged(ref _environmentRenderTexture, _environmentRenderer);
        UpdateDepthTextureIfResolutionChanged(ref _environmentRenderDepthTexture, _environmentDepthRenderer);
    }
}
