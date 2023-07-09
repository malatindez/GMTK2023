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
    private Camera _displayCamera;
    // Start is called before the first frame update
    private void Start()
    {
        _displayCamera = GetComponent<Camera>();
    }

    private static void UpdateTextureIfResolutionChanged(ref RenderTexture texture, Camera targetCamera)
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
            targetCamera.targetTexture = texture;
        }
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
        MainCameraRenderer.orthoProj = _mapRenderer.projectionMatrix;
        MainCameraRenderer.orthoView = _mapRenderer.worldToCameraMatrix;
        MainCameraRenderer.invMainProj = _environmentDepthRenderer.projectionMatrix.inverse;
        MainCameraRenderer.invMainView = _environmentDepthRenderer.worldToCameraMatrix.inverse;


        UpdateTextureIfResolutionChanged(ref _mainRenderTexture, _mainRenderer);
        UpdateTextureIfResolutionChanged(ref _environmentRenderTexture, _environmentRenderer);
        UpdateDepthTextureIfResolutionChanged(ref _environmentRenderDepthTexture, _environmentDepthRenderer);
    }
}
