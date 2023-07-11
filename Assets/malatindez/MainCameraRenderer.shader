Shader "Unlit/MainCameraRenderer" // DO NOT CHANGE. Main camera renderer feature depends on this string.
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _EnvironmentTex ("Texture", 2D) = "white" {}
        _EnvironmentDepthTex ("Texture", 2D) = "white" {}
        _WorldTex ("Texture", 2D) = "white" {}
        _VisibilityFogOfWarTex ("Fog of War Texture", 2D) = "white" {}
        _VisibilityTex ("Visibility Texture", 2D) = "white" {}         
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog effect available in scene view
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            UNITY_DECLARE_TEX2D(_EnvironmentTex);
            UNITY_DECLARE_TEX2D(_EnvironmentDepthTex);
            UNITY_DECLARE_TEX2D(_WorldTex);
            UNITY_DECLARE_TEX2D(_VisibilityFogOfWarTex);
            UNITY_DECLARE_TEX2D(_VisibilityTex);

            float4x4 _InvViewProj;
            float4x4 _OrthoViewProj;


            float2 _ImageDimensions;

            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
                {
                float4 clipPos : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(2)
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.clipPos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }


            fixed4 frag(v2f i) : SV_Target
            {
        
                float4 col = UNITY_SAMPLE_TEX2D(_EnvironmentTex, i.uv);
                float4 col2 = UNITY_SAMPLE_TEX2D(_WorldTex, i.uv);
                float depth = UNITY_SAMPLE_TEX2D(_EnvironmentDepthTex, i.uv);
                if (depth <= 0.001) // if we don't see anything
                {
                    return col;
                }

                // Compute normalized viewport position in the range [-1, 1]
                float2 uv = i.uv * 2 - 1;

                // Unproject the depth and viewport position to homogenized coordinates
                float4 homogenizedPos = float4(uv.x, uv.y, depth, 1.0);
        
                // Transform to world coordinates
                float4 worldPos = mul(_InvViewProj, homogenizedPos);

                // Divide by w to dehomogenize
                worldPos /= worldPos.w;
        
                float4 orthoPos = mul(_OrthoViewProj, worldPos);
                float2 orthoUV = orthoPos.xy / orthoPos.w;
                orthoUV = (orthoUV + 1) * 0.5;
//    return float4(orthoUV, 0, 0);
                float fog = UNITY_SAMPLE_TEX2D(_VisibilityFogOfWarTex, orthoUV);
                float vis = UNITY_SAMPLE_TEX2D(_VisibilityTex, orthoUV);
                if(vis >= 0.99f)
                {
                    return col2;
                }
                return col2 * vis * 0.5 + 
                    float4(float3(length(col.xyz) / 3, length(col.xyz) / 3, length(col.xyz) / 3) * fog, 1) * 0.3
                + max(0.0f, fog - 0.1f) * col2 * 0.2 ;

            }
            ENDCG
        }
    }
}
