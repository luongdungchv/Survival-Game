Shader "Custom/URP Lit"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0, 1)) = 0.5
        _Metallic ("Metallic", Range(0, 1)) = 0.5
        
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

           #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local_fragment _EMISSION
            #pragma shader_feature_local_fragment _METALLICSPECGLOSSMAP
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local_fragment _OCCLUSIONMAP
            #pragma shader_feature_local _PARALLAXMAP
            #pragma shader_feature_local _ _DETAIL_MULX2 _DETAIL_SCALED
            #pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF
            #pragma shader_feature_local_fragment _SPECULAR_SETUP
            #pragma shader_feature_local _RECEIVE_SHADOWS_OFF

            // -------------------------------------
            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile_fog

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON
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
            CBUFFER_START(UnityPerMaterial)
                uniform float4 _Color;
                uniform float _Metallic;
                uniform float _Glossiness;
            CBUFFER_END

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
                half4 baseMap = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                surfData.albedo = baseMap;
                surfData.smoothness = _Glossiness;
                surfData.metallic = _Metallic;
                surfData.alpha = baseMap.a;
                surfData.occlusion = 1;
                return surfData;
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
                SurfaceData surfData = InitializeSurfaceData(input);
                InputData pbrInput = InitializePBRInput(input);
                half4 color = UniversalFragmentPBR(pbrInput, surfData);
                color = half4(MixFog(color.rgb, input.fogCoord), color.a);
                color *= _Color;
                clip(color.a - 0.01);
                return color;
            }
            
            ENDHLSL
        }
    }
}
