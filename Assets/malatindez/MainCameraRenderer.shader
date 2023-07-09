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
            #pragma glsl_precision_high

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
            float4x4 _MainView;
            float4x4 _MainProj;
            float _PerspectiveAspectRatio;
            float _OrthoAspectRatio;
            
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

float2 adjustAspectRatio(float2 uv, float fromAspect, float toAspect)
{
    float ratio = fromAspect / toAspect;
    uv.x = uv.x * ratio + (1.0 - ratio) * 0.5;
    uv.y = uv.y * ratio + (1.0 - ratio) * 0.5;
    return uv;
}

            fixed4 frag(v2f i) : SV_Target
            {
                // Reconstructing the eye space position from depth buffer value
                float depth = UNITY_SAMPLE_TEX2D(_EnvironmentDepthTex, i.uv).r;
                float4 clipPos = float4(i.uv * 2.0 - 1.0, depth * 2 - 1, 1.0);
    float4 viewPos = mul(_InvMainProj, clipPos);
                viewPos /= viewPos.w; // undo the perspective division
    float3 worldPos = mul(_InvMainView, viewPos).xyz;
    //return fixed4(worldPos.xyz / 10, 1);

                // Compute ortho UV of the pixel from world position and ortho matrices
                float4 orthoPos = mul(_OrthoProj, mul(_OrthoView, float4(worldPos, 1)));
                float2 orthoUV = (orthoPos.xy / orthoPos.w) * 0.5 + 0.5;
    
                float fog = UNITY_SAMPLE_TEX2D(_VisibilityFogOfWarTex, orthoUV).r;
                float vis = UNITY_SAMPLE_TEX2D(_VisibilityTex, orthoUV).r;
    
                float4 col = UNITY_SAMPLE_TEX2D(_EnvironmentTex, i.uv);
                float4 col2 = UNITY_SAMPLE_TEX2D(_WorldTex, i.uv);
//                return float4(fog,vis,0,1);
    return float4(worldPos / 10, 1);
                fixed4 finalColor = fixed4(i.uv.x, i.uv.y, 0, 1);
                return finalColor;
            }
            ENDCG
        }
    }
}
