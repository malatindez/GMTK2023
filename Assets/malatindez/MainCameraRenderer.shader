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
        _UVPrepassTexture ("UV Ortho Map Texture", 2D) = "white" {}
         
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
            UNITY_DECLARE_TEX2D(_UVPrepassTexture);

            
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
    float depth = UNITY_SAMPLE_TEX2D(_EnvironmentDepthTex, i.uv);
    float2 orthoUV = UNITY_SAMPLE_TEX2D(_UVPrepassTexture, i.uv).xy;
    if(orthoUV.x < 0 || orthoUV.x > 1 || orthoUV.y < 0 || orthoUV.y > 1)
    {
//        return float4(0, 0, 0, 1);
    }
    float fog = UNITY_SAMPLE_TEX2D(_VisibilityFogOfWarTex, orthoUV);
    float vis = UNITY_SAMPLE_TEX2D(_VisibilityTex, orthoUV);
    
    float4 col = UNITY_SAMPLE_TEX2D(_EnvironmentTex, i.uv);
    float4 col2 = UNITY_SAMPLE_TEX2D(_WorldTex, i.uv);
    
    return col2 * vis + float4(float3(length(col.xyz) / 3, length(col.xyz) / 3, length(col.xyz) / 3) * fog, 1) * 0.2;

}
            ENDCG
        }
    }
}
