Shader "Environment/Terrain/Terrain Shader"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _testTex ("Test Texture", 2D) = "white" {}
        _testScale("Test Texture Scale", float) = 1
        baseTextures("Textures Array", 2DArray) = "" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0
        #pragma require 2darray
        #include "UnityCG.cginc"
        
        sampler2D _MainTex;
        sampler2D _testTex;
        float _testScale;

        const static int maxColorCount = 8;
        const static float epsilon = 1E-4;
        int baseColorCount;
        float4 baseColors[maxColorCount];
        float baseHeights[maxColorCount];
        float baseBlends[maxColorCount];
        UNITY_DECLARE_TEX2DARRAY(baseTextures);
        
        float minHeight;
        float maxHeight;

        struct Input
        {
            float3 worldPos;
            float3 worldNormal;
        };

        

        float inverseLerp(float a, float b, float val){
            return saturate((val - a)/(b - a));
        }
        
        float3 triplanar(float3 worldPos, float scale, float3 blendAxes, int textureIndex) {
            float3 scaledWorldPos = worldPos / scale;
            float3 xProjection = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(scaledWorldPos.y, scaledWorldPos.z, textureIndex)) * blendAxes.x;
            float3 yProjection = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(scaledWorldPos.x, scaledWorldPos.z, textureIndex)) * blendAxes.y;
            float3 zProjection = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(scaledWorldPos.x, scaledWorldPos.y, textureIndex)) * blendAxes.z;
            return xProjection + yProjection + zProjection;
        }
        void surf (Input i, inout SurfaceOutputStandard o)
        {
            //o.Albedo = col;
            float percentHeight = smoothstep(minHeight, maxHeight, i.worldPos.y);
            
            float3 blendAxes = abs(i.worldNormal);
            blendAxes /= blendAxes.x + blendAxes.y + blendAxes.z;
            
            for(int j = baseColorCount - 1; j >= 0; j--){                
                float blendStrength = smoothstep(-baseBlends[j] / 2 - epsilon, baseBlends[j] / 2, percentHeight - baseHeights[j]);
                //float4 texColor = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(i.worldPos.xz / _testScale, j));
                float3 texColor = triplanar(i.worldPos, _testScale, blendAxes, j);
                o.Albedo = o.Albedo * (1 - blendStrength) + texColor * blendStrength;   
                o.Smoothness = 0;            
            }
            //float blendStrength = smoothstep(-baseBlends[0] / 2, baseBlends[0] / 2, percentHeight - baseHeights[0]);
            //float blendStrength = step(0, percentHeight - baseHeights[0]);
            //o.Albedo = float4(1,0.5,0,1);
            //o.Albedo = blendStrength;
            // Albedo comes from a texture tinted by color
            //o.Albedo = percentHeight;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
