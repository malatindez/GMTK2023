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
    private static readonly int RayTextureSizeID = Shader.PropertyToID("RayTextureSize");
    private static readonly int DepthMapID = Shader.PropertyToID("DepthMap");
    private static readonly int ResultID = Shader.PropertyToID("Result");
    private static readonly int VisibilityMaskWidthID = Shader.PropertyToID("VisibilityMaskWidth");
    private static readonly int VisibilityMaskHeightID = Shader.PropertyToID("VisibilityMaskHeight");
    private static readonly int MinAlphaID = Shader.PropertyToID("MinAlpha");
    private static readonly int WidthID = Shader.PropertyToID("Width");
    private static readonly int HeightID = Shader.PropertyToID("Height");
    private static readonly int ClearColorID = Shader.PropertyToID("ClearColor");
    private static readonly int MainViewMaskID = Shader.PropertyToID("MainViewMask");
    private static readonly int HalfViewAngleID = Shader.PropertyToID("HalfViewAngle");
    private static readonly int AngleStepID = Shader.PropertyToID("AngleStep");
    private static readonly int HighlightAngleID = Shader.PropertyToID("HighlightAngle");
    private static readonly int InvOrthoMatrixID = Shader.PropertyToID("InvOrthoMatrix");
    private static readonly int CameraPositionID = Shader.PropertyToID("CameraPosition");
    private static readonly int HighlightCenterID = Shader.PropertyToID("HighlightCenter");
    private static readonly int MaximumVisibleDistancePixelsID = Shader.PropertyToID("MaximumVisibleDistancePixels");
    private static readonly int RayOriginHeightID = Shader.PropertyToID("RayOriginHeight");

    public void UpdateVisibilityMask(
        Vector3 worldRayDirection,
        Vector3 worldRayOrigin,
        Vector3 highlightCenter,
        float viewAngle,
        float worldViewDistance,
        int numRaysPerDegree,
        int maximumAmountOfStepsPerRay,
        float highlightRadius
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
            Vector3 heightStepSize = rayOrigin - _mapCamera.WorldToViewportPoint(worldRayOrigin + Vector3.down);
            // normalize height decrease
            heightDecrease *= pixelStep / 2;
        }

        // if the amount of pixels on the diagonal is less than the amount of rays
        // we need to decrease amount of rays because it wouldn't make sense to have more steps than amount of we can see

        int diagonalPixelAmount = (int)new Vector2(_mapDepthTexture.width, _mapDepthTexture.height).magnitude;
//        maximumAmountOfStepsPerRay = Math.Min(maximumAmountOfStepsPerRay, (int)(worldViewDistance * pixelStep * diagonalPixelAmount));

        float halfViewAngle = Mathf.Deg2Rad * viewAngle / 2;
        float angleStep = (Mathf.Deg2Rad * viewAngle) / Mathf.Ceil(viewAngle * numRaysPerDegree);
        
        float highlightAngle = Vector3.Dot(
            (worldRayOrigin - _mainCamera.transform.position).normalized,
            (worldRayOrigin + _mainCamera.transform.up * highlightRadius - _mainCamera.transform.position).normalized
        );
        

#if UNITY_EDITOR
        _visibilityConeShader.SetTexture(0, FurthestVisibleDistancesID, _furthestVisibleDistances);
        _visibilityConeShader.SetTexture(0, MapDepthTextureID, _mapDepthTexture);
        _visibilityConeShader.SetInt(RayTextureSizeID, _rayTextureSize);


        _visibilityMaskShader.SetTexture(0, FurthestVisibleDistancesID, _furthestVisibleDistances);
        _visibilityMaskShader.SetTexture(0, DepthMapID, _mapDepthTexture);
        _visibilityMaskShader.SetTexture(0, ResultID, MainViewMask);
        _visibilityMaskShader.SetInt(VisibilityMaskWidthID, MainViewMask.width);
        _visibilityMaskShader.SetInt(VisibilityMaskHeightID, MainViewMask.height);
        _visibilityMaskShader.SetInt(RayTextureSizeID, _rayTextureSize);
#endif

        Matrix4x4 orthoViewMatrix = _mapCamera.worldToCameraMatrix;
        Matrix4x4 orthoProjectionMatrix = _mapCamera.projectionMatrix;

        Matrix4x4 orthoViewProjectionMatrix = orthoProjectionMatrix * orthoViewMatrix;

        int numRays = Mathf.CeilToInt(viewAngle * numRaysPerDegree);
        _visibilityConeShader.SetVector(MapDepthTextureSizeID, new Vector2(_mapDepthTexture.width, _mapDepthTexture.height));
        _visibilityConeShader.SetInts(RayOriginPixelsID, rayOriginPixels);
        _visibilityConeShader.SetVector(RayDirectionID, rayDirection);

        _visibilityConeShader.SetFloat(HeightID, height);
        _visibilityConeShader.SetFloat(HeightDecreaseID, heightDecrease);

        _visibilityConeShader.SetFloat(HalfViewAngleID, halfViewAngle);
        _visibilityConeShader.SetFloat(AngleStepID, angleStep);

        _visibilityConeShader.SetInt(NumStepsID, maximumAmountOfStepsPerRay);
        
        int threadGroups = Mathf.CeilToInt(numRays / 64.0f);
        _visibilityConeShader.Dispatch(0, threadGroups, 1, 1);

        _visibilityMaskShader.SetInts(RayOriginPixelsID, rayOriginPixels);
        _visibilityMaskShader.SetVector(RayDirectionID, rayDirection);
        _visibilityMaskShader.SetFloat(ViewAngleID, viewAngle);
        _visibilityMaskShader.SetInt(NumRaysPerDegreeID, numRaysPerDegree);
        _visibilityMaskShader.SetFloat(MinAlphaID, _minAlpha);
        _visibilityMaskShader.SetFloat(HighlightAngleID, highlightAngle);
        _visibilityMaskShader.SetMatrix(InvOrthoMatrixID, orthoViewProjectionMatrix.inverse);
        _visibilityMaskShader.SetVector(CameraPositionID, _mainCamera.transform.position);
        _visibilityMaskShader.SetVector(HighlightCenterID, highlightCenter);
        _visibilityMaskShader.SetFloat(RayOriginHeightID, height);
        _visibilityMaskShader.SetInt(MaximumVisibleDistancePixelsID, maximumAmountOfStepsPerRay);
        _visibilityMaskShader.Dispatch(0, MainViewMask.width / 8, MainViewMask.height / 8, 1);
    }
    public RenderTexture MainViewMask { get; private set; }

    #endregion Methods

    #region Fields
    [SerializeField] private ComputeShader _clearShaderR8G8B8A8Conditional;
    [SerializeField] private ComputeShader _visibilityConeShader;
    [SerializeField] private ComputeShader _visibilityMaskShader;
    [SerializeField] private ComputeShader _R8ToR8G8B8A8TransferShader;
    [SerializeField] public RenderTexture _mapDepthTexture;
    private RenderTexture _furthestVisibleDistances;

    [SerializeField] private Camera _mapCamera = null;
    [SerializeField] private Camera _mainCamera = null;

    private int _maskHeight = 8192;
    private int _maskWidth = 8192;

    [SerializeField] private int _maximumTotalAmountOfRays = 512 * 512;
    private int _rayTextureSize;
    [SerializeField] private float _minAlpha = 0.5f;

    #endregion Fields

    #region Debug
    public Texture2D RenderR8TextureToTexture2D(RenderTexture renderTexture)
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


    public Texture2D RenderR32FTextureToTexture2D(RenderTexture renderTexture)
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

    public Texture2D RenderRGBATextureToTexture2D(RenderTexture renderTexture)
    {
        Texture2D tempTexture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
        tempTexture.hideFlags = HideFlags.HideAndDontSave;

        RenderTexture currentActiveRT = RenderTexture.active;

        RenderTexture.active = renderTexture;

        tempTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);

        tempTexture.Apply();

        RenderTexture.active = currentActiveRT;

        return tempTexture;
    }

    private Texture2D _debugMainViewMask;
    private Texture2D _debugFurthestVisibleDistancesView;
    [SerializeField] private bool _debugRenderMasks = true;

    #endregion Debug

    private void ClearMasks()
    {
        _clearShaderR8G8B8A8Conditional.SetTexture(0, ResultID, MainViewMask);
        int threadGroupsX = Mathf.CeilToInt(_maskWidth / 16.0f);
        int threadGroupsY = Mathf.CeilToInt(_maskHeight / 16.0f);

        _clearShaderR8G8B8A8Conditional.Dispatch(0, threadGroupsX, threadGroupsY, 1);
    }

    private void FixedUpdate()
    {
        ClearMasks();
    }
    private void LateUpdate()
    {
        if (_debugRenderMasks)
        {
            Destroy(_debugMainViewMask);
            Destroy(_debugFurthestVisibleDistancesView);
            _debugMainViewMask = RenderRGBATextureToTexture2D(MainViewMask);
            _debugFurthestVisibleDistancesView = RenderR32FTextureToTexture2D(_furthestVisibleDistances);
            _debugRenderMasks = false;
            File.WriteAllBytes("MainViewMask.png", _debugMainViewMask.EncodeToPNG());
            File.WriteAllBytes("FurthestVisibleDistances.png", _debugFurthestVisibleDistancesView.EncodeToPNG());

        }
    }

    private void Start()
    {
        _maskHeight = _mapDepthTexture.height;
        _maskWidth = _mapDepthTexture.width;
        
        MainViewMask = new RenderTexture(_maskWidth, _maskHeight, 0)
        {
            enableRandomWrite = true,
            depthStencilFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.None,
            graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_UNorm
        };

        _rayTextureSize = (int)Math.Sqrt(Math.Pow(2, Math.Ceiling(Math.Log(_maximumTotalAmountOfRays) / Math.Log(2))));
        _furthestVisibleDistances = new RenderTexture(_rayTextureSize, _rayTextureSize, 0)
        {
            enableRandomWrite = true,
            depthStencilFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.None,
            graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R32_SFloat
        };
        MainViewMask.Create();
        _furthestVisibleDistances.Create();

        _visibilityConeShader.SetTexture(0, FurthestVisibleDistancesID, _furthestVisibleDistances);
        _visibilityConeShader.SetTexture(0, MapDepthTextureID, _mapDepthTexture);
        _visibilityConeShader.SetInt(RayTextureSizeID, _rayTextureSize);



        // R -> FogOfWar
        // G -> VisibilityMask
        // B -> HighlightMask
        // A -> EcholocationMask
        _clearShaderR8G8B8A8Conditional.SetTexture(0, ResultID, MainViewMask);
        _clearShaderR8G8B8A8Conditional.SetVector(ClearColorID, new Vector4(0,0,0,0));
        _clearShaderR8G8B8A8Conditional.SetBool("R", true);
        _clearShaderR8G8B8A8Conditional.SetBool("G", true);
        _clearShaderR8G8B8A8Conditional.SetBool("B", true);
        _clearShaderR8G8B8A8Conditional.SetBool("A", true);
        ClearMasks();
        _clearShaderR8G8B8A8Conditional.SetBool("R", false);
        _clearShaderR8G8B8A8Conditional.SetBool("A", false);

        _visibilityMaskShader.SetTexture(0, FurthestVisibleDistancesID, _furthestVisibleDistances);
        _visibilityMaskShader.SetTexture(0, DepthMapID, _mapDepthTexture);
        _visibilityMaskShader.SetTexture(0, MainViewMaskID, MainViewMask);
        _visibilityMaskShader.SetInt(VisibilityMaskWidthID, MainViewMask.width);
        _visibilityMaskShader.SetInt(VisibilityMaskHeightID, MainViewMask.height);
        _visibilityMaskShader.SetInt(RayTextureSizeID, _rayTextureSize);


    }
}