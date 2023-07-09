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

            float4x4 _OrthoView;
            float4x4 _OrthoProj;
            float4x4 _InvMainView;
            float4x4 _InvMainProj;
            
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
                float rawDepth = UNITY_SAMPLE_TEX2D(_EnvironmentDepthTex, i.uv);
    
                float4 viewPos = mul(_InvMainProj, float4(i.uv * 2.0 - 1.0, rawDepth, 1.0));
                viewPos /= viewPos.w;
                float3 worldPos = mul(_InvMainView, viewPos).xyz;

    
                // Convert world space coordinates to orthographic camera space
                float3 viewPosOrtho = mul(_OrthoView, float4(worldPos, 1.0)).xyz;
                float3 projPosOrtho = mul(_OrthoProj, float4(viewPosOrtho, 1.0)).xyz;
                float2 orthoUV = 0.5 * (projPosOrtho.xy / projPosOrtho.z) + 0.5;
    
                float fog = UNITY_SAMPLE_TEX2D(_VisibilityFogOfWarTex, orthoUV).r;
                float vis = UNITY_SAMPLE_TEX2D(_VisibilityTex, orthoUV).r;
    
                float4 col = UNITY_SAMPLE_TEX2D(_EnvironmentTex, i.uv);
                float4 col2 = UNITY_SAMPLE_TEX2D(_WorldTex, i.uv);
    return fixed4(orthoUV.x  / 1000, orthoUV.y /1000, 0, 1);
    return col;
                fixed4 finalColor = fixed4(i.uv.x, i.uv.y, 0, 1);
                return finalColor;
            }
            ENDCG
        }
    }
}
