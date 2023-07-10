using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[ExecuteInEditMode, RequireComponent(typeof(Camera))]
public class CameraToFile : MonoBehaviour
{
    private enum TextureType { Opaque, Depth }
    private enum DepthFormat { R8, R16, R24, R32 }
    private enum OpaqueFormat
    {
        R32G32B32A32,
        R16G16B16A16,
        R32G32B32,
        R8G8B8A8
    }

    [SerializeField] private TextureType textureType = TextureType.Depth;

    [ShowIf("textureType", TextureType.Depth)]
    [SerializeField] private DepthFormat depthPrecision = DepthFormat.R24;
    [ShowIf("textureType", TextureType.Opaque)]
    [SerializeField] private OpaqueFormat opaquePrecision = OpaqueFormat.R32G32B32A32;
    [SerializeField] private string _filePath = "Assets/Textures/DepthTexture.bytes"; 
    private ComputeShader _computeShader;

    private Camera _camera;
    private RenderTexture _renderTexture = null;

    void Start()
    {
        _camera = GetComponent<Camera>();
        _camera.depthTextureMode = DepthTextureMode.Depth;
        _computeShader = Resources.Load<ComputeShader>("malatindez/CameraToFileComputeShader.compute");
    }

    [Button("Update Precision Format for Render Texture")]
    void UpdatePrecisionFormatForRenderTexture()
    {
        if (textureType == TextureType.Depth)
        {
            switch (depthPrecision)
            {
                case DepthFormat.R8:
                    _renderTexture = new RenderTexture(_camera.pixelWidth, _camera.pixelHeight, 8, RenderTextureFormat.Depth);
                    break;
                case DepthFormat.R16:
                    _renderTexture = new RenderTexture(_camera.pixelWidth, _camera.pixelHeight, 16, RenderTextureFormat.Depth);
                    break;
                case DepthFormat.R24:
                    _renderTexture = new RenderTexture(_camera.pixelWidth, _camera.pixelHeight, 24, RenderTextureFormat.Depth);
                    break;
                case DepthFormat.R32:
                    _renderTexture = new RenderTexture(_camera.pixelWidth, _camera.pixelHeight, 32, RenderTextureFormat.Depth);
                    break;
                default:
                    throw new System.ArgumentOutOfRangeException();
            }
        }
        else
        {
            switch (opaquePrecision)
            {
                case OpaqueFormat.R32G32B32A32:
                    _renderTexture = new RenderTexture(_camera.pixelWidth, _camera.pixelHeight, 0, RenderTextureFormat.ARGBFloat);
                    break;
                case OpaqueFormat.R16G16B16A16:
                    _renderTexture = new RenderTexture(_camera.pixelWidth, _camera.pixelHeight, 0, RenderTextureFormat.ARGBHalf);
                    break;
                case OpaqueFormat.R32G32B32:
                    _renderTexture = new RenderTexture(_camera.pixelWidth, _camera.pixelHeight, 0, RenderTextureFormat.ARGBFloat); // Unity does not support a RenderTextureFormat for R32G32B32.
                    break;
                case OpaqueFormat.R8G8B8A8:
                    _renderTexture = new RenderTexture(_camera.pixelWidth, _camera.pixelHeight, 0, RenderTextureFormat.ARGB32);
                    break;
                default:
                    throw new System.ArgumentOutOfRangeException();
            }
        }
        
        // Set the active RenderTexture to the one we created so our screenshot gets rendered to it.
        RenderTexture.active = _renderTexture;

        // Depending on the chosen texture type, perform the necessary rendering operations.
        if (textureType == TextureType.Depth)
        {
            _camera.SetTargetBuffers(_renderTexture.colorBuffer, _renderTexture.depthBuffer);
        }
        else
        {
            _camera.targetTexture = _renderTexture;
            _camera.Render();
        }

        ComputeBuffer _buffer = new ComputeBuffer(_camera.pixelWidth * _camera.pixelHeight, sizeof(float));

        // Set the active texture and the ComputeBuffer to the shader
        // You will need to create a Shader that copies the depth data to a ComputeBuffer
        _computeShader.SetTexture(0, "_MainTex", _renderTexture);
        _computeShader.SetBuffer(0, "_Buffer", _buffer);
        _computeShader.SetInt("_Width", _camera.pixelWidth);
        _computeShader.SetInt("_Height", _camera.pixelHeight);

        // Render the current frame to byte buffer
        _computeShader.Dispatch(0, _camera.pixelWidth / 8, _camera.pixelHeight / 8, 1);
        
        // Now that we've rendered, retrieve the data from the ComputeBuffer
        float[] data = new float[_camera.pixelWidth * _camera.pixelHeight];
        _buffer.GetData(data);

        // Convert the data to bytes and save it to a file
        byte[] bytes = new byte[data.Length * sizeof(float)];
        System.Buffer.BlockCopy(data, 0, bytes, 0, bytes.Length);

        File.WriteAllBytes(Application.dataPath + _filePath, bytes);

        // Clean up
        RenderTexture.active = null;
        _camera.targetTexture = null;
        _buffer.Release();
    }


    static Texture2D LoadFromFile(string path, int width, int height)
    {
        byte[] fileData = System.IO.File.ReadAllBytes(path);
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.LoadRawTextureData(fileData);
        tex.Apply();
        return tex;
    }
}
