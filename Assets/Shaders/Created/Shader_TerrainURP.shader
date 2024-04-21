Shader "Environment/Terrain/Terrain Shader URP"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0, 1)) = 0.5
        _Metallic ("Metallic", Range(0, 1)) = 0.5
        _BaseTextures("Textures Array", 2DArray) = "" {}

        baseColorCount("Base Color Count", Float) = 0
        
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline"}
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 200
        Cull Off

        Pass{
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma require 2darray
            #pragma multi_compile_fog
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

            TEXTURE2D_ARRAY(_BaseTextures);
            SAMPLER(sampler_BaseTextures);

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
                pbrInput.normalWS = normalize(input.normalWS);
                pbrInput.viewDirectionWS = input.viewDirectionWS;
                pbrInput.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.vertexSH, pbrInput.normalWS);
                return pbrInput;
            }
            SurfaceData InitializeSurfaceData(Varyings input){
                SurfaceData surfData = (SurfaceData)0;
                half4 baseMap = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                surfData.albedo = baseMap;
                surfData.smoothness = _Glossiness;
                surfData.metallic = _Metallic;
                surfData.alpha = baseMap.a;
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

            half3 CustomGI(BRDFData brdfData, BRDFData brdfDataClearCoat, float clearCoatMask,
            half3 bakedGI, half occlusion,
            half3 normalWS, half3 viewDirectionWS)
            {
                half3 reflectVector = reflect(-viewDirectionWS, normalWS);
                half NoV = saturate(dot(normalWS, viewDirectionWS));
                half fresnelTerm = Pow4(1.0 - NoV);

                half3 indirectDiffuse = bakedGI * 1;
                half3 indirectSpecular = GlossyEnvironmentReflection(reflectVector, brdfData.perceptualRoughness, occlusion);

                half3 color = EnvironmentBRDF(brdfData, indirectDiffuse, indirectSpecular, fresnelTerm);
                return color;

                #if defined(_CLEARCOAT) || defined(_CLEARCOATMAP)
                    half3 coatIndirectSpecular = GlossyEnvironmentReflection(reflectVector, brdfDataClearCoat.perceptualRoughness, occlusion);
                    half3 coatColor = EnvironmentBRDFClearCoat(brdfDataClearCoat, clearCoatMask, coatIndirectSpecular, fresnelTerm);
                    half coatFresnel = kDielectricSpec.x + kDielectricSpec.a * fresnelTerm;
                    return color * (1.0 - coatFresnel * clearCoatMask) + coatColor;
                #else
                    return color;
                #endif
            }

            half4 CustomPBRLit(InputData inputData, SurfaceData surfaceData){
                bool specularHighlightsOff = false;
                // #ifdef _SPECULARHIGHLIGHTS_OFF
                //     specularHighlightsOff = true;
                // #endif

                BRDFData brdfData;

                // NOTE: can modify alpha
                InitializeBRDFData(surfaceData.albedo, surfaceData.metallic, surfaceData.specular, surfaceData.smoothness, surfaceData.alpha, brdfData);

                BRDFData brdfDataClearCoat = (BRDFData)0;

                //To ensure backward compatibility we have to avoid using shadowMask input, as it is not present in older shaders
                half4 shadowMask = half4(1,1,1,1);

                Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS, shadowMask);

                half3 color = CustomGI(brdfData, brdfDataClearCoat, surfaceData.clearCoatMask,
                inputData.bakedGI, surfaceData.occlusion,
                inputData.normalWS, inputData.viewDirectionWS);
                color += LightingPhysicallyBased(brdfData, brdfDataClearCoat,
                mainLight,
                inputData.normalWS, inputData.viewDirectionWS,
                surfaceData.clearCoatMask, specularHighlightsOff);
                return half4(color, 1);

                #ifdef _ADDITIONAL_LIGHTS
                    uint pixelLightCount = GetAdditionalLightsCount();
                    for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
                    {
                        Light light = GetAdditionalLight(lightIndex, inputData.positionWS, shadowMask);
                        #if defined(_SCREEN_SPACE_OCCLUSION)
                            light.color *= aoFactor.directAmbientOcclusion;
                        #endif
                        color += LightingPhysicallyBased(brdfData, brdfDataClearCoat,
                        light,
                        inputData.normalWS, inputData.viewDirectionWS,
                        surfaceData.clearCoatMask, specularHighlightsOff);
                    }
                #endif
                color += surfaceData.emission;

                return half4(color, 1);
                return half4(color, surfaceData.alpha);
            }

            Varyings vert(Attributes vertexInput){
                Varyings output = (Varyings)0;
                output.positionCS = TransformObjectToHClip(vertexInput.positionOS);
                output.uv = vertexInput.uv;
                output.normalWS = TransformObjectToWorldNormal(vertexInput.normal);
                output.positionWS = TransformObjectToWorld(vertexInput.positionOS);
                output.fogCoord = InitializeInputDataFog(float4(output.positionWS, 1), ComputeFogFactor(output.positionCS.z));
                output.shadowCoord = TransformWorldToShadowCoord(output.positionWS);
                output.viewDirectionWS = GetWorldSpaceNormalizeViewDir(output.positionWS);
                return output;
            }

            float4 frag(Varyings input): SV_Target{
                //SurfaceData surfData = (SurfaceData)0;
                SurfaceData surfData = InitializeSurfaceData(input);
                InputData pbrInput = InitializePBRInput(input);

                surf(pbrInput, surfData);

                // Light light = GetMainLight(input.shadowCoord);

                // //half NdotL = saturate(dot(input.normalWS, light.direction));
                // half3 radiance = (light.distanceAttenuation * light.shadowAttenuation);
                
                // half3 lightColor = LightingLambert(light.color, light.direction, input.normalWS);
                // lightColor += float3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w);

                // //half3 lightColor = VertexLighting(input.positionWS, input.normalWS);
                
                // surfData.albedo *= lightColor;
                // //surfData.albedo *= pow(light.distanceAttenuation * light.shadowAttenuation, 0.65);
                // half4 color = half4(MixFog(surfData.albedo, input.fogCoord), 1);

                half4 color = UniversalFragmentPBR(pbrInput, surfData);
                color.rgb = MixFog(surfData.albedo, input.fogCoord);
                color.a = 1;

                return color;
            }
            
            ENDHLSL
        }
    }
}
