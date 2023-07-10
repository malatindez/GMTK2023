using System;
using System.IO;
using UnityEngine;

public class VisibilityManager : MonoBehaviour
{
    // TODO:
    // Update the visibility cone so it depends on the point's height and
    // computes an angle between the point and the camera to check if there's an intersection.
    // TODO:
    // multiple depth maps with different resolution to approximate
    #region Methods

    public void GetCameraBounds()
    {
        float camHeight = _mapCamera.orthographicSize * 2;
        float camWidth = camHeight * _mapCamera.aspect;

        Vector3 camPos = _mapCamera.transform.position;

        Vector3 bottomLeft = camPos - new Vector3(camWidth / 2, camHeight / 2, 0);
        Vector3 topRight = camPos + new Vector3(camWidth / 2, camHeight / 2, 0);

#if UNITY_EDITOR
        Debug.Log("Bottom Left: " + bottomLeft);
        Debug.Log("Top Right: " + topRight);
#endif
    }
    void Awake()
    {
#if UNITY_EDITOR
        QualitySettings.vSyncCount = 0;  // VSync must be disabled
        Application.targetFrameRate = 90;
#endif
    }

    private static readonly int FurthestVisibleDistancesID = Shader.PropertyToID("FurthestVisibleDistances");
    private static readonly int MapDepthTextureID = Shader.PropertyToID("MapDepthTexture");
    private static readonly int MapDepthTextureSizeID = Shader.PropertyToID("MapDepthTextureSize");
    private static readonly int RayOriginPixelsID = Shader.PropertyToID("RayOriginPixels");
    private static readonly int HeightDecreaseID = Shader.PropertyToID("HeightDecrease");
    private static readonly int RayDirectionID = Shader.PropertyToID("RayDirection");
    private static readonly int ViewAngleID = Shader.PropertyToID("ViewAngle");
    private static readonly int NumStepsID = Shader.PropertyToID("NumSteps");
    private static readonly int NumRaysPerDegreeID = Shader.PropertyToID("NumRaysPerDegree");
    private static readonly int MinDepthID = Shader.PropertyToID("MinDepth");
    private static readonly int RayTextureSizeID = Shader.PropertyToID("RayTextureSize");
    private static readonly int DepthMapID = Shader.PropertyToID("DepthMap");
    private static readonly int ResultID = Shader.PropertyToID("Result");
    private static readonly int VisibilityMaskWidthID = Shader.PropertyToID("VisibilityMaskWidth");
    private static readonly int VisibilityMaskHeightID = Shader.PropertyToID("VisibilityMaskHeight");
    private static readonly int MinAlphaID = Shader.PropertyToID("MinAlpha");
    private static readonly int WidthID = Shader.PropertyToID("Width");
    private static readonly int HeightID = Shader.PropertyToID("Height");
    private static readonly int ClearColorID = Shader.PropertyToID("ClearColor");
    private static readonly int VisibilityMaskID = Shader.PropertyToID("VisibilityMask");
    private static readonly int FogOfWarMaskID = Shader.PropertyToID("FogOfWarMask");

    public void UpdateVisibilityMask(
        Vector3 worldRayDirection,
        Vector3 worldRayOrigin,
        float viewAngle,
        float worldViewDistance,
        int numRaysPerDegree,
        int maximumAmountOfStepsPerRay
        )
    {
        if (maximumAmountOfStepsPerRay <= 1 || numRaysPerDegree <= 0 || viewAngle < 0 || viewAngle * numRaysPerDegree >= _maximumTotalAmountOfRays)
        {
#if UNITY_EDITOR
            Debug.LogWarning("Invalid parameters for visibility mask calculation");
#endif
            return;
        }
        Vector2 rayOriginUV;
        int[] rayOriginPixels;
        float height;
        Vector2 rayDirection;
        float heightDecrease;
        float pixelStep;

        {
            Vector3 rayOrigin = _mapCamera.WorldToViewportPoint(worldRayOrigin);
            Vector3 unprojectedRayDirection = _mapCamera.WorldToViewportPoint(worldRayOrigin + worldRayDirection) - rayOrigin;

            rayOriginUV = new Vector2(rayOrigin.x, rayOrigin.y);

            // convert to depth viewport coordinates
            height = 1 - (rayOrigin.z - _mapCamera.nearClipPlane) / (_mapCamera.farClipPlane - _mapCamera.nearClipPlane);
            heightDecrease = (unprojectedRayDirection.z - _mapCamera.nearClipPlane) / (_mapCamera.farClipPlane - _mapCamera.nearClipPlane);

            rayDirection = new Vector2(
                unprojectedRayDirection.x,
                unprojectedRayDirection.y
            );
            unprojectedRayDirection.x = rayDirection.x;
            unprojectedRayDirection.y = rayDirection.y;
            rayDirection.Normalize();

            Vector3 pixelStepSize = rayOrigin - _mapCamera.WorldToViewportPoint(worldRayOrigin + Vector3.right);
            pixelStep = pixelStepSize.magnitude;
            rayOriginPixels = new int[] { (int)(rayOrigin.x * _mapDepthTexture.width), (int)(rayOrigin.y * _mapDepthTexture.height) };
            // normalize height decrease
            heightDecrease *= pixelStep;
        }

        // if the amount of pixels on the diagonal is less than the amount of rays
        // we need to decrease amount of rays because it wouldn't make sense to have more steps than amount of we can see

        int diagonalPixelAmount = (int)new Vector2(_mapDepthTexture.width, _mapDepthTexture.height).magnitude;
        maximumAmountOfStepsPerRay = Math.Min(maximumAmountOfStepsPerRay, (int)(worldViewDistance * pixelStep * diagonalPixelAmount));

        ClearFurthestVisibleDistances();
        int numRays = Mathf.CeilToInt(viewAngle * numRaysPerDegree);
        _visibilityConeShader.SetVector(MapDepthTextureSizeID, new Vector2(_mapDepthTexture.width, _mapDepthTexture.height));
        _visibilityConeShader.SetInts(RayOriginPixelsID, rayOriginPixels);
        _visibilityConeShader.SetFloat(HeightID, height);
        _visibilityConeShader.SetFloat(HeightDecreaseID, heightDecrease);
        _visibilityConeShader.SetVector(RayDirectionID, rayDirection);
        _visibilityConeShader.SetFloat(ViewAngleID, viewAngle);
        _visibilityConeShader.SetInt(NumStepsID, maximumAmountOfStepsPerRay);
        _visibilityConeShader.SetInt(NumRaysPerDegreeID, numRaysPerDegree);
        _visibilityConeShader.SetFloat(MinDepthID, _minDepth);
        int threadGroups = Mathf.CeilToInt(numRays / 64.0f);
        _visibilityConeShader.Dispatch(0, threadGroups, 1, 1);

        _visibilityMaskShader.SetInts(RayOriginPixelsID, rayOriginPixels);
        _visibilityMaskShader.SetVector(RayDirectionID, rayDirection);
        _visibilityMaskShader.SetFloat(ViewAngleID, viewAngle);
        _visibilityMaskShader.SetInt(NumRaysPerDegreeID, numRaysPerDegree);
        _visibilityMaskShader.SetFloat(MinAlphaID, _minAlpha);
        _visibilityMaskShader.Dispatch(0, VisibilityMask.width / 8, VisibilityMask.height / 8, 1);
    }
    public RenderTexture VisibilityMask { get; private set; }
    public RenderTexture FogOfWarMask { get; private set; }

    #endregion Methods

    #region Fields

    [SerializeField] private ComputeShader _clearShaderR8;
    [SerializeField] private ComputeShader _clearShaderR8G8B8A8;
    [SerializeField] private ComputeShader _fogOfWarMaskShader;
    private RenderTexture _furthestVisibleDistances;
    [SerializeField] private Camera _mapCamera = null;
    [SerializeField] public RenderTexture _mapDepthTexture;
    private int _maskHeight = 8192;
    private int _maskWidth = 8192;
    [SerializeField] private int _maximumTotalAmountOfRays = 1024 * 1024;
    [SerializeField] private float _minDepth = 0.5f;
    private int _rayTextureSize;
    [SerializeField] private ComputeShader _visibilityConeShader;
    [SerializeField] private ComputeShader _visibilityMaskShader;
    [SerializeField] private ComputeShader _R8ToR8G8B8A8TransferShader;
    [SerializeField] private float _minAlpha = 0.5f;

    #endregion Fields

    #region Debug
    public Texture2D RenderTextureToTexture2DR8(RenderTexture renderTexture)
    {
        Texture2D tempTexture = new(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
        tempTexture.hideFlags = HideFlags.HideAndDontSave;

        ComputeBuffer buffer = new(renderTexture.width * renderTexture.height * 4, sizeof(float));

        _R8ToR8G8B8A8TransferShader.SetTexture(0, "Input", renderTexture);
        _R8ToR8G8B8A8TransferShader.SetBuffer(0, ResultID, buffer);
        _R8ToR8G8B8A8TransferShader.SetInt(WidthID, renderTexture.width);
        _R8ToR8G8B8A8TransferShader.Dispatch(0, renderTexture.width / 8, renderTexture.height / 8, 1);

        Color[] colors = new Color[renderTexture.width * renderTexture.height];
        buffer.GetData(colors);
        tempTexture.SetPixels(colors);
        tempTexture.Apply();

        buffer.Release();


        return tempTexture;
    }


    public Texture2D RenderTextureToTexture2DR32F(RenderTexture renderTexture)
    {
        Texture2D texture = new(renderTexture.width, renderTexture.height, TextureFormat.RFloat, false);
        texture.hideFlags = HideFlags.HideAndDontSave;

        RenderTexture.active = renderTexture;
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture.Apply();
        RenderTexture.active = null;
        Texture2D rgbTex = new(texture.width, texture.height, TextureFormat.RGB24, false);
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                float rFloatValue = texture.GetPixel(x, y).r;
                rgbTex.SetPixel(x, y, new Color(rFloatValue / 10, rFloatValue / 10, rFloatValue / 10));
            }
        }
        rgbTex.Apply();
        return rgbTex;
    }



    private Texture2D _debugFogOfWarMaskView;
    private Texture2D _debugFurthestVisibleDistancesView;
    [SerializeField] private bool _debugRenderMasks = true;
    private Texture2D _debugVisibilityMaskView;

    #endregion Debug
    private void ClearFurthestVisibleDistances()
    {
        _clearShaderR8.SetTexture(0, ResultID, _furthestVisibleDistances);
        int threadGroupsX = Mathf.CeilToInt(_maskWidth / 16.0f);
        int threadGroupsY = Mathf.CeilToInt(_maskHeight / 16.0f);

        _clearShaderR8.Dispatch(0, threadGroupsX, threadGroupsY, 1);
    }

    private void ClearFogOfWarMask()
    {
        _clearShaderR8.SetTexture(0, ResultID, FogOfWarMask);
        int threadGroupsX = Mathf.CeilToInt(_maskWidth / 16.0f);
        int threadGroupsY = Mathf.CeilToInt(_maskHeight / 16.0f);

        _clearShaderR8.Dispatch(0, threadGroupsX, threadGroupsY, 1);
    }

    private void ClearVisibilityMask()
    {
        _clearShaderR8.SetTexture(0, ResultID, VisibilityMask);
        int threadGroupsX = Mathf.CeilToInt(_maskWidth / 16.0f);
        int threadGroupsY = Mathf.CeilToInt(_maskHeight / 16.0f);

        _clearShaderR8.Dispatch(0, threadGroupsX, threadGroupsY, 1);
    }

    private void FixedUpdate()
    {
        ClearVisibilityMask();
    }
    private void LateUpdate()
    {
        _fogOfWarMaskShader.SetTexture(0, VisibilityMaskID, VisibilityMask);
        _fogOfWarMaskShader.SetTexture(0, FogOfWarMaskID, FogOfWarMask);
        _fogOfWarMaskShader.Dispatch(0, _maskWidth / 8, _maskHeight / 8, 1);
        if (_debugRenderMasks)
        {
            Destroy(_debugFogOfWarMaskView);
            Destroy(_debugVisibilityMaskView);
            Destroy(_debugFurthestVisibleDistancesView);
            _debugFogOfWarMaskView = RenderTextureToTexture2DR8(FogOfWarMask);
            _debugVisibilityMaskView = RenderTextureToTexture2DR8(VisibilityMask);
            _debugFurthestVisibleDistancesView = RenderTextureToTexture2DR32F(_furthestVisibleDistances);
            _debugRenderMasks = false;
            File.WriteAllBytes("FogOfWarMask.png", _debugFogOfWarMaskView.EncodeToPNG());
            File.WriteAllBytes("VisibilityMask.png", _debugVisibilityMaskView.EncodeToPNG());
            File.WriteAllBytes("FurthestVisibleDistances.png", _debugFurthestVisibleDistancesView.EncodeToPNG());

        }
    }

    private void Start()
    {
        _maskHeight = _mapDepthTexture.height;
        _maskWidth = _mapDepthTexture.width;

        _clearShaderR8.SetInt(WidthID, _maskWidth);
        _clearShaderR8.SetInt(HeightID, _maskHeight);
        _clearShaderR8.SetInt(ClearColorID, 0);

        _clearShaderR8.SetInt(WidthID, _maskWidth);
        _clearShaderR8.SetInt(HeightID, _maskHeight);
        _clearShaderR8.SetInt(ClearColorID, 0);

        _clearShaderR8.SetInt(WidthID, _maskWidth);
        _clearShaderR8.SetInt(HeightID, _maskHeight);
        _clearShaderR8.SetInt(ClearColorID, 0);

        VisibilityMask = new RenderTexture(_maskWidth, _maskHeight, 0)
        {
            enableRandomWrite = true,
            depthStencilFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.None,
            graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8_UNorm
        };
        FogOfWarMask = new RenderTexture(_maskWidth, _maskHeight, 0)
        {
            enableRandomWrite = true,
            depthStencilFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.None,
            graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R16_UNorm
        };

        _rayTextureSize = (int)Math.Sqrt(Math.Pow(2, Math.Ceiling(Math.Log(_maximumTotalAmountOfRays) / Math.Log(2))));
        _furthestVisibleDistances = new RenderTexture(_rayTextureSize, _rayTextureSize, 0)
        {
            enableRandomWrite = true,
            depthStencilFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.None,
            graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R32_SFloat
        };
        VisibilityMask.Create();
        FogOfWarMask.Create();
        _furthestVisibleDistances.Create();

        _visibilityConeShader.SetTexture(0, FurthestVisibleDistancesID, _furthestVisibleDistances);
        _visibilityConeShader.SetTexture(0, MapDepthTextureID, _mapDepthTexture);
        _visibilityConeShader.SetInt(RayTextureSizeID, _rayTextureSize);


        _visibilityMaskShader.SetTexture(0, FurthestVisibleDistancesID, _furthestVisibleDistances);
        _visibilityMaskShader.SetTexture(0, DepthMapID, _mapDepthTexture);
        _visibilityMaskShader.SetTexture(0, ResultID, VisibilityMask);
        _visibilityMaskShader.SetInt(VisibilityMaskWidthID, VisibilityMask.width);
        _visibilityMaskShader.SetInt(VisibilityMaskHeightID, VisibilityMask.height);
        _visibilityMaskShader.SetInt(RayTextureSizeID, _rayTextureSize);

        ClearFogOfWarMask();
        ClearVisibilityMask();
    }
}