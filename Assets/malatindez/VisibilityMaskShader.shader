Shader "Unlit/VisibilityMaskShader"
{
    Properties
    {
        _FurthestVisibleDistances ("FurthestVisibleDistances", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1) 
    }
    SubShader
    {
        // No culling or depth
Cull Off

ZWrite Off

ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

#include "UnityCG.cginc"

struct appdata
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
};

struct v2f
{
    float2 uv : TEXCOORD0;
    float4 vertex : SV_POSITION;
    float4 worldPos : TEXCOORD1;
};

sampler2D _FurthestVisibleDistances;
float4 _Color;
float _ViewDistance;
            
float2 RayOrigin;
float2 RayDirection;
float ViewAngle;
float ViewDistance;
int NumRaysPerDegree;
int RayTextureSize;

v2f vert(appdata v)
{
    v2f o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.uv = v.uv;
    o.worldPos = mul(unity_ObjectToWorld, v.vertex);
    return o;
}

fixed frag(v2f i) : SV_Target
{
    float halfViewAngle = ViewAngle / 2.0f;
    int numRays = ceil(ViewAngle * NumRaysPerDegree);
    float angleStep = ViewAngle / (float) (numRays - 1);

    float3 worldPos = i.worldPos.xyz;

    float2 toPixel = worldPos.xz - RayOrigin;
                        
    float2 normalizedToPixel = normalize(toPixel.xy);
    float2 normalizedRayDirection = normalize(RayDirection);

    float dotProduct = dot(normalizedToPixel, normalizedRayDirection);
    float angle = acos(dotProduct);

    int rayId = int((angle + halfViewAngle) / angleStep);
    float distance = length(toPixel.xy);
                        
    float2 rayTextureCoord = float2(rayId % (uint) RayTextureSize, rayId / (uint) RayTextureSize) / (uint) RayTextureSize;
    float furthestVisibleDistance = tex2D(_FurthestVisibleDistances, rayTextureCoord).r;
                        
    if (distance > furthestVisibleDistance)
    {
        return 0.5f;
    }

    float col = lerp(0.0f, 0.5f, distance / _ViewDistance);
    return col;
}
            ENDCG
        }
    }
}
