using System;
using System.IO;
using UnityEngine;

public class VisibilityManager : MonoBehaviour
{
    #region Methods

    public void GetCameraBounds()
    {
        float camHeight = _mapCamera.orthographicSize * 2;
        float camWidth = camHeight * _mapCamera.aspect;

        Vector3 camPos = _mapCamera.transform.position;

        Vector3 bottomLeft = camPos - new Vector3(camWidth / 2, camHeight / 2, 0);
        Vector3 topRight = camPos + new Vector3(camWidth / 2, camHeight / 2, 0);

        Debug.Log("Bottom Left: " + bottomLeft);
        Debug.Log("Top Right: " + topRight);
    }

    public void UpdateVisibilityMask(
        Vector3 worldRayDirection,
        Vector3 worldRayOrigin,
        float viewAngle,
        float worldViewDistance,
        int numRaysPerDegree,
        int numSteps,
        bool updateFogOfWar = true
        )
    {
        if (numSteps <= 1 || numRaysPerDegree <= 0 || viewAngle < 0)
        {
            Debug.LogWarning("Invalid parameters for visibility mask calculation");
            return;
        }
        Vector3 unprojectedRayOrigin = _mapCamera.WorldToViewportPoint(worldRayOrigin);
        Vector3 unprojectedRayDirection = _mapCamera.WorldToViewportPoint(worldRayOrigin + worldRayDirection) - unprojectedRayOrigin;
        Vector2 rayOrigin = new Vector2(unprojectedRayOrigin.x, unprojectedRayOrigin.y);
        Vector2 rayDirection = new Vector2(unprojectedRayDirection.x, unprojectedRayDirection.y);
        rayDirection.Normalize();
        float viewDistance = worldViewDistance / _mapCamera.orthographicSize * 2.0f;
        ClearFurthestVisibleDistances();
        int numRays = Mathf.CeilToInt(viewAngle * numRaysPerDegree);
        _visibilityConeShader.SetTexture(0, "FurthestVisibleDistances", _furthestVisibleDistances);
        _visibilityConeShader.SetTexture(0, "MapDepthTexture", _mapDepthTexture);

        _visibilityConeShader.SetVector("MapDepthTextureSize", new Vector2(_mapDepthTexture.width, _mapDepthTexture.height));
        _visibilityConeShader.SetVector("RayOrigin", rayOrigin);
        _visibilityConeShader.SetVector("RayDirection", rayDirection);
        _visibilityConeShader.SetFloat("ViewAngle", viewAngle);
        _visibilityConeShader.SetFloat("ViewDistance", viewDistance);
        _visibilityConeShader.SetInt("NumRaysPerDegree", numRaysPerDegree);
        _visibilityConeShader.SetInt("NumSteps", numSteps);
        _visibilityConeShader.SetFloat("MinDepth", _minDepth);
        _visibilityConeShader.SetInt("RayTextureSize", _rayTextureSize);
        int threadGroups = Mathf.CeilToInt(numRays / 64.0f);
        _visibilityConeShader.Dispatch(0, threadGroups, 1, 1);

        _visibilityMaskShader.SetTexture(0, "FurthestVisibleDistances", _furthestVisibleDistances);
        _visibilityMaskShader.SetTexture(0, "DepthMap", _mapDepthTexture);
        _visibilityMaskShader.SetTexture(0, "Result", _visibilityMask);
        _visibilityMaskShader.SetVector("RayOrigin", rayOrigin);
        _visibilityMaskShader.SetVector("RayDirection", rayDirection);
        _visibilityMaskShader.SetFloat("ViewAngle", viewAngle);
        _visibilityMaskShader.SetFloat("ViewDistance", viewDistance);
        _visibilityMaskShader.SetInt("NumRaysPerDegree", numRaysPerDegree);
        _visibilityMaskShader.SetInt("RayTextureSize", _rayTextureSize);
        _visibilityMaskShader.SetInt("VisibilityMaskWidth", _visibilityMask.width);
        _visibilityMaskShader.SetInt("VisibilityMaskHeight", _visibilityMask.height);
        _visibilityMaskShader.SetFloat("MinAlpha", _minAlpha);
        _visibilityMaskShader.Dispatch(0, _visibilityMask.width / 8, _visibilityMask.height / 8, 1);
        // TODO: MOVE TO COMPUTE SHADER
        if (updateFogOfWar)
        {
            _fogOfWarMaskShader.SetTexture(0, "VisibilityMask", _visibilityMask);
            _fogOfWarMaskShader.SetTexture(0, "FogOfWarMask", _fogOfWarMask);
            _fogOfWarMaskShader.Dispatch(0, _maskWidth / 8, _maskHeight / 8, 1);
        }
        if (_debugRenderMasks)
        {
            _debugFogOfWarMaskView = RenderTextureToTexture2DR8(_fogOfWarMask);
            _debugVisibilityMaskView = RenderTextureToTexture2DR8(_visibilityMask);
            _debugFurthestVisibleDistancesView = RenderTextureToTexture2DR32F(_furthestVisibleDistances);
            _debugRenderMasks = false;
            File.WriteAllBytes("FogOfWarMask.png", _debugFogOfWarMaskView.EncodeToPNG());
            File.WriteAllBytes("VisibilityMask.png", _debugVisibilityMaskView.EncodeToPNG());
            File.WriteAllBytes("FurthestVisibleDistances.png", _debugFurthestVisibleDistancesView.EncodeToPNG());

        }
    }

    #endregion Methods

    #region Fields

    [SerializeField] private ComputeShader _clearShaderR8;
    [SerializeField] private ComputeShader _clearShaderR8G8B8A8;
    private RenderTexture _fogOfWarMask;
    [SerializeField] private ComputeShader _fogOfWarMaskShader;
    private RenderTexture _furthestVisibleDistances;
    [SerializeField] private Camera _mapCamera = null;
    [SerializeField] private RenderTexture _mapDepthTexture;
    private int _maskHeight = 8192;
    private int _maskWidth = 8192;
    [SerializeField] private int _maximumTotalAmountOfRays = 512 * 512;
    [SerializeField] private float _minDepth = 0.5f;
    private int _rayTextureSize;
    [SerializeField] private ComputeShader _visibilityConeShader;
    private RenderTexture _visibilityMask;
    [SerializeField] private ComputeShader _visibilityMaskShader;
    [SerializeField] private ComputeShader _R8ToR8G8B8A8TransferShader;
    [SerializeField] private float _minAlpha = 0.5f;

    #endregion Fields

    #region Debug
    public Texture2D RenderTextureToTexture2DR8(RenderTexture renderTexture)
    {
        Texture2D tempTexture = new(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);

        ComputeBuffer buffer = new(renderTexture.width * renderTexture.height * 4, sizeof(float));

        _R8ToR8G8B8A8TransferShader.SetTexture(0, "Input", renderTexture);
        _R8ToR8G8B8A8TransferShader.SetBuffer(0, "Result", buffer);
        _R8ToR8G8B8A8TransferShader.SetInt("Width", renderTexture.width);
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
        _clearShaderR8.SetTexture(0, "Result", _furthestVisibleDistances);
        _clearShaderR8.SetInt("Width", _maskWidth);
        _clearShaderR8.SetInt("Height", _maskHeight);
        _clearShaderR8.SetInt("ClearColor", 0);
        int threadGroupsX = Mathf.CeilToInt(_maskWidth / 16.0f);
        int threadGroupsY = Mathf.CeilToInt(_maskHeight / 16.0f);

        _clearShaderR8.Dispatch(0, threadGroupsX, threadGroupsY, 1);
    }

    private void ClearFogOfWarMask()
    {
        _clearShaderR8.SetTexture(0, "Result", _fogOfWarMask);
        _clearShaderR8.SetInt("Width", _maskWidth);
        _clearShaderR8.SetInt("Height", _maskHeight);
        _clearShaderR8.SetInt("ClearColor", 0);
        int threadGroupsX = Mathf.CeilToInt(_maskWidth / 16.0f);
        int threadGroupsY = Mathf.CeilToInt(_maskHeight / 16.0f);

        _clearShaderR8.Dispatch(0, threadGroupsX, threadGroupsY, 1);
    }

    private void ClearVisibilityMask()
    {
        _clearShaderR8.SetTexture(0, "Result", _visibilityMask);
        _clearShaderR8.SetInt("Width", _maskWidth);
        _clearShaderR8.SetInt("Height", _maskHeight);
        _clearShaderR8.SetInt("ClearColor", 0);
        int threadGroupsX = Mathf.CeilToInt(_maskWidth / 16.0f);
        int threadGroupsY = Mathf.CeilToInt(_maskHeight / 16.0f);

        _clearShaderR8.Dispatch(0, threadGroupsX, threadGroupsY, 1);
    }

    private void FixedUpdate()
    {
        ClearVisibilityMask();
    }

    private void Start()
    {
        _maskHeight = _mapDepthTexture.height;
        _maskWidth = _mapDepthTexture.width;
        _visibilityMask = new RenderTexture(_maskWidth, _maskHeight, 0)
        {
            enableRandomWrite = true,
            depthStencilFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.None,
            graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8_UNorm
        };
        _fogOfWarMask = new RenderTexture(_maskWidth, _maskHeight, 0)
        {
            enableRandomWrite = true,
            depthStencilFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.None,
            graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8_UNorm
        };

        _rayTextureSize = (int)Math.Sqrt(Math.Pow(2, Math.Ceiling(Math.Log(_maximumTotalAmountOfRays) / Math.Log(2))));
        _furthestVisibleDistances = new RenderTexture(_rayTextureSize, _rayTextureSize, 0)
        {
            enableRandomWrite = true,
            depthStencilFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.None,
            graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R32_SFloat
        };

        ClearFogOfWarMask();
        ClearVisibilityMask();
    }
}