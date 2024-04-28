Shader "Environment/Terrain/Terrain Shader URP"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0, 1)) = 0.5
        _Metallic ("Metallic", Range(0, 1)) = 0.5
        _BaseTextures("Textures Array", 2DArray) = "" {}
        
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "LightMode"="UniversalForward" "RenderPipeline"="UniversalPipeline"}
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 200
        Cull Off

        Pass{
            Tags { "LightMode" = "UniversalForwardOnly" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma require 2darray
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma target 4.5
            
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes{
                float4 positionOS: POSITION;
                float2 uv: TEXCOORD0;
                float4 normal: NORMAL;  
            };
            struct Varyings{
                float4 positionCS: SV_POSITION;
                float2 uv: TEXCOORD0;
                float3 normalWS: TEXCOORD1;
                float3 positionWS: TEXCOORD5;
                float4 shadowCoord: TEXCOORD2;
                float fogCoord: TEXCOORD3;
                half3 viewDirectionWS: TEXCOORD4; 
                DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 8);
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            // TEXTURE2D_ARRAY(_BaseTextures);
            // SAMPLER(sampler_BaseTextures);
            Texture2DArray _BaseTextures;
            SamplerState sampler_BaseTextures;

            const static int maxColorCount = 8;
            const static float epsilon = 1E-4;

            uniform float4 _Color;
            uniform float _Metallic;
            uniform float _Glossiness;
            uniform int baseColorCount;
            uniform float4 baseColors[maxColorCount];
            uniform float baseHeights[maxColorCount];
            uniform float baseBlends[maxColorCount];
            uniform float minHeight;
            uniform float maxHeight;
            uniform float _testScale;
            // CBUFFER_START(UnityPerMaterial)        
                
            // CBUFFER_END

            InputData InitializePBRInput(Varyings input){
                InputData pbrInput = (InputData)0;
                pbrInput.positionWS = input.positionWS;
                pbrInput.positionCS = input.positionCS;
                pbrInput.fogCoord = input.fogCoord;
                pbrInput.shadowCoord = input.shadowCoord;
                pbrInput.normalWS = NormalizeNormalPerPixel(input.normalWS);
                pbrInput.viewDirectionWS = input.viewDirectionWS;
                pbrInput.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.vertexSH, pbrInput.normalWS);
                return pbrInput;
            }
            SurfaceData InitializeSurfaceData(Varyings input){
                SurfaceData surfData = (SurfaceData)0;
                // half4 baseMap = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                // surfData.albedo = baseMap;
                surfData.smoothness = _Glossiness;
                surfData.metallic = _Metallic;
                surfData.alpha = 1;
                surfData.occlusion = 1;
                return surfData;
            }

            float inverseLerp(float a, float b, float val){
                return saturate((val - a)/(b - a));
            }
            
            float3 triplanar(float3 worldPos, float scale, float3 blendAxes, int textureIndex) {
                float3 scaledWorldPos = worldPos / scale;
                float3 xProjection = SAMPLE_TEXTURE2D_ARRAY(_BaseTextures, sampler_BaseTextures, float2(scaledWorldPos.y, scaledWorldPos.z), textureIndex) * blendAxes.x;
                float3 yProjection = SAMPLE_TEXTURE2D_ARRAY(_BaseTextures, sampler_BaseTextures, float2(scaledWorldPos.x, scaledWorldPos.z), textureIndex) * blendAxes.y;
                float3 zProjection = SAMPLE_TEXTURE2D_ARRAY(_BaseTextures, sampler_BaseTextures, float2(scaledWorldPos.x, scaledWorldPos.y), textureIndex) * blendAxes.z;
                return xProjection + yProjection + zProjection;
            }
            void surf (InputData i, inout SurfaceData o)
            {
                //o.Albedo = col;
                float percentHeight = smoothstep(minHeight, maxHeight, i.positionWS.y);
                
                float3 blendAxes = abs(i.normalWS);
                blendAxes /= blendAxes.x + blendAxes.y + blendAxes.z;

                for(int j = baseColorCount - 1; j >= 0; j--){                
                    float blendStrength = smoothstep(-baseBlends[j] / 2 - epsilon, baseBlends[j] / 2, percentHeight - baseHeights[j]);
                    float3 texColor = triplanar(i.positionWS, _testScale, blendAxes, j);
                    o.albedo = o.albedo * (1 - blendStrength) + texColor * blendStrength;   
                }
                
            }

            Varyings vert(Attributes vertexInput){
                Varyings output = (Varyings)0;
                output.positionCS = TransformObjectToHClip(vertexInput.positionOS);
                output.uv = vertexInput.uv;
                output.normalWS = TransformObjectToWorldNormal(vertexInput.normal);
                output.fogCoord = ComputeFogFactor(output.positionCS.z);
                output.positionWS = TransformObjectToWorld(vertexInput.positionOS);
                output.shadowCoord = TransformWorldToShadowCoord(output.positionWS);
                output.viewDirectionWS = GetWorldSpaceNormalizeViewDir(output.positionWS);
                return output;
            }

            float4 frag(Varyings input): SV_Target{
                //return _BaseTextures.Sample(sampler_BaseTextures, float3(35, 35, 0));
                SurfaceData surfData = InitializeSurfaceData(input);
                InputData pbrInput = InitializePBRInput(input);
                //return half4(surfData.albedo, 1);
                surf(pbrInput, surfData);
                half4 color = UniversalFragmentPBR(pbrInput, surfData);
                color = half4(MixFog(color.rgb, input.fogCoord), color.a);
                color *= _Color;
                color.a = 1;
                clip(color.a - 0.01);
                return color;
            }
            
            ENDHLSL
        }
    }
}
