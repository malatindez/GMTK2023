using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

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
    private static readonly int RayAmountID = Shader.PropertyToID("RayAmount");
    private static readonly int EchoAmountID = Shader.PropertyToID("EchoAmount");
    private static readonly int RayDataID = Shader.PropertyToID("RayData");
    private static readonly int EchoDataID = Shader.PropertyToID("EchoData");

    // CHANGE THIS DATA IN SHADER AS WELL
    public static readonly int MaximumRaysPerFrame = 32;

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
        maximumAmountOfStepsPerRay = Math.Min(maximumAmountOfStepsPerRay, (int)(worldViewDistance * pixelStep * diagonalPixelAmount));

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

#endif

        int numRays = Mathf.CeilToInt(viewAngle * numRaysPerDegree);
        _visibilityConeShader.SetVector(MapDepthTextureSizeID, new Vector2(_mapDepthTexture.width, _mapDepthTexture.height));
        _visibilityConeShader.SetInts(RayOriginPixelsID, rayOriginPixels);
        _visibilityConeShader.SetVector(RayDirectionID, rayDirection);

        _visibilityConeShader.SetFloat(HeightID, height);
        _visibilityConeShader.SetFloat(HeightDecreaseID, heightDecrease);

        _visibilityConeShader.SetFloat(HalfViewAngleID, halfViewAngle);
        _visibilityConeShader.SetFloat(AngleStepID, angleStep);

        _visibilityConeShader.SetInt(NumStepsID, maximumAmountOfStepsPerRay);
        _visibilityConeShader.SetInt("RayIdOffset", rayIdOffset);
        
        int threadGroups = Mathf.CeilToInt(numRays / 64.0f);
        _visibilityConeShader.Dispatch(0, threadGroups, 1, 1);

        rayData.Add (rayOriginPixels[0]);
        rayData.Add (rayOriginPixels[1]);
        rayData.Add (rayDirection.x);
        rayData.Add (rayDirection.y);

        rayData.Add(viewAngle);
        rayData.Add(maximumAmountOfStepsPerRay);
        rayData.Add(numRaysPerDegree);
        rayData.Add(height); 

        rayData.Add(highlightCenter.x); 
        rayData.Add(highlightCenter.y); 
        rayData.Add(highlightCenter.z); 
        rayData.Add(highlightAngle);
        rayIdOffset += numRays;
    }
    public RenderTexture MainViewMask { get; private set; }

    #endregion Methods

    #region Fields
    [SerializeField] private ComputeShader _clearShaderR8G8B8A8Conditional;
    [SerializeField] private ComputeShader _visibilityConeShader;
    [SerializeField] private ComputeShader _visibilityMaskShader;
    [SerializeField] private ComputeShader _RToRGBATransferShader;
    [SerializeField] private ComputeShader _RGBAToRGBATransferShader;
    [SerializeField] public RenderTexture _mapDepthTexture;
    private RenderTexture _furthestVisibleDistances;

    [SerializeField] private Camera _mapCamera = null;
    [SerializeField] private Camera _mainCamera = null;

    private int _maskHeight = 8192;
    private int _maskWidth = 8192;

    [SerializeField] private int _maximumTotalAmountOfRays = 1024 * 1024;
    private int _rayTextureSize;
    [SerializeField] private float _minAlpha = 0.5f;

    private struct RayData
    {
        // Ints are casted to floats to increase performance
        Vector2Int rayOriginPixels;
        Vector2 rayDirection;
        float viewAngle;
        int numRaysPerDegree;
        float highlightAngle;
        Vector3 highlightCenter;
        float height;
        int maximumAmountOfStepsPerRay;
    };
    // 12 floats
    private static readonly int RayDataSize = 12;
    private List<float> rayData = new List<float>();
    int rayIdOffset = 0;
    
    private ComputeBuffer _visibilityMaskRayData;
    #endregion Fields

    #region Debug
    public Texture2D RenderR8TextureToTexture2D(RenderTexture renderTexture)
    {
        Texture2D tempTexture = new(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
        tempTexture.hideFlags = HideFlags.HideAndDontSave;

        ComputeBuffer buffer = new(renderTexture.width * renderTexture.height * 4, sizeof(float));

        _RToRGBATransferShader.SetTexture(0, "Input", renderTexture);
        _RToRGBATransferShader.SetBuffer(0, ResultID, buffer);
        _RToRGBATransferShader.SetInt(WidthID, renderTexture.width);
        _RToRGBATransferShader.Dispatch(0, renderTexture.width / 8, renderTexture.height / 8, 1);

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
        Texture2D tempTexture = new(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
        tempTexture.hideFlags = HideFlags.HideAndDontSave;

        ComputeBuffer buffer = new(renderTexture.width * renderTexture.height * 4, sizeof(float));

        _RGBAToRGBATransferShader.SetTexture(0, "Input", renderTexture);
        _RGBAToRGBATransferShader.SetBuffer(0, ResultID, buffer);
        _RGBAToRGBATransferShader.SetInt(WidthID, renderTexture.width);
        _RGBAToRGBATransferShader.Dispatch(0, renderTexture.width / 8, renderTexture.height / 8, 1);

        Color[] colors = new Color[renderTexture.width * renderTexture.height];
        buffer.GetData(colors);
        tempTexture.SetPixels(colors);
        tempTexture.Apply();

        buffer.Release();


        return tempTexture;
    }

    private Texture2D _debugMainViewMask;
    private Texture2D _debugFurthestVisibleDistancesView;
    [SerializeField] private bool _debugRenderMasks = true;

    #endregion Debug

    private void ClearMasks()
    {
        _clearShaderR8G8B8A8Conditional.SetTexture(0, ResultID, MainViewMask);
        _clearShaderR8G8B8A8Conditional.SetVector(ClearColorID, new Vector4(0.0f, 0.0f, 0.0f, 1.0f));
        _clearShaderR8G8B8A8Conditional.SetInt("ClearFlags", 0x2 | 0x4 | 0x8);
        _clearShaderR8G8B8A8Conditional.SetInt("Width", MainViewMask.width);
        _clearShaderR8G8B8A8Conditional.SetInt("Height", MainViewMask.height);
        int threadGroupsX = Mathf.CeilToInt(_maskWidth / 16.0f);
        int threadGroupsY = Mathf.CeilToInt(_maskHeight / 16.0f);
        _clearShaderR8G8B8A8Conditional.Dispatch(0, threadGroupsX, threadGroupsY, 1);
    }

    private void FixedUpdate()
    {
    }
    private void LateUpdate()
    {
#if UNITY_EDITOR
        _visibilityMaskShader.SetBuffer(0, RayDataID, _visibilityMaskRayData);
        _visibilityMaskShader.SetTexture(0, FurthestVisibleDistancesID, _furthestVisibleDistances);
        _visibilityMaskShader.SetTexture(0, DepthMapID, _mapDepthTexture);
        _visibilityMaskShader.SetTexture(0, MainViewMaskID, MainViewMask);
        _visibilityMaskShader.SetInt(VisibilityMaskWidthID, MainViewMask.width);
        _visibilityMaskShader.SetInt(VisibilityMaskHeightID, MainViewMask.height);
        _visibilityMaskShader.SetInt(RayTextureSizeID, _rayTextureSize);
#endif

        Matrix4x4 orthoViewMatrix = _mapCamera.worldToCameraMatrix;
        Matrix4x4 orthoProjectionMatrix = _mapCamera.projectionMatrix;

        Matrix4x4 orthoViewProjectionMatrix = orthoProjectionMatrix * orthoViewMatrix;

        _visibilityMaskShader.SetFloat(MinAlphaID, _minAlpha);
        _visibilityMaskShader.SetMatrix(InvOrthoMatrixID, orthoViewProjectionMatrix.inverse);
        _visibilityMaskShader.SetVector(CameraPositionID, _mainCamera.transform.position);
        if (rayData.Count != 0)
        {
            _visibilityMaskRayData.SetData(rayData);
        }
        _visibilityMaskShader.SetInt(RayAmountID, rayData.Count / RayDataSize);

        _visibilityMaskShader.Dispatch(0, MainViewMask.width / 8, MainViewMask.height / 8, 1);


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
        rayData.Clear();
        rayIdOffset = 0;
    }

    private void Start()
    {
        _visibilityMaskRayData = new ComputeBuffer(RayDataSize * MaximumRaysPerFrame, sizeof(float) * RayDataSize);

        _maskHeight = _mapDepthTexture.height;
        _maskWidth = _mapDepthTexture.width;
        
        MainViewMask = new RenderTexture(_maskWidth, _maskHeight, 0)
        {
            enableRandomWrite = true,
            depthStencilFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.None,
            graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_SFloat
        };

        _rayTextureSize = (int)Math.Sqrt(Math.Pow(2, Math.Ceiling(Math.Log(_maximumTotalAmountOfRays) / Math.Log(2))));
        _furthestVisibleDistances = new RenderTexture(_rayTextureSize, _rayTextureSize, 0)
        {
            enableRandomWrite = true,
            depthStencilFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.None,
            graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R16_SFloat
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
        _clearShaderR8G8B8A8Conditional.SetVector(ClearColorID, new Vector4(0.0f,0.0f,0.0f,1.0f));
        _clearShaderR8G8B8A8Conditional.SetInt("ClearFlags", 0x1 | 0x2 | 0x4 | 0x8);
        ClearMasks();
        _clearShaderR8G8B8A8Conditional.SetInt("ClearFlags", 0x2 | 0x4 | 0x8);
        _clearShaderR8G8B8A8Conditional.SetInt("Width", MainViewMask.width);
        _clearShaderR8G8B8A8Conditional.SetInt("Height", MainViewMask.height);

        _visibilityMaskShader.SetBuffer(0, RayDataID, _visibilityMaskRayData);
        _visibilityMaskShader.SetTexture(0, FurthestVisibleDistancesID, _furthestVisibleDistances);
        _visibilityMaskShader.SetTexture(0, DepthMapID, _mapDepthTexture);
        _visibilityMaskShader.SetTexture(0, MainViewMaskID, MainViewMask);
        _visibilityMaskShader.SetInt(VisibilityMaskWidthID, MainViewMask.width);
        _visibilityMaskShader.SetInt(VisibilityMaskHeightID, MainViewMask.height);
        _visibilityMaskShader.SetInt(RayTextureSizeID, _rayTextureSize);


    }
}