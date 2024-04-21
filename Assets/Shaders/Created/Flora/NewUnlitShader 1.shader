Shader "Environment/Flora/Grass Compute 2 Test"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        
        _Scale ("Noise Scale", float) = 1
        
        _TopColor ("Top Color", Color) = (1,1,1,1)
        _BotColor ("Bot Color", Color) = (1,1,1,1)
        
        _TopColor1 ("Top Color", Color) = (1,1,1,1)
        _BotColor1 ("Bot Color", Color) = (1,1,1,1)
        
        _BlendFactor("Blend Factor", float) = 0.5
        _SmoothnessState("Smoothness State", float) = 0
    }
    SubShader
    {
        LOD 100
        Cull Off
        ZWrite On
        Name "Grass"
        Tags {
            "RenderType"="Opaque"
            "RenderPipeline"="UniversalPipeline"
            "LightMode"="UniversalForwardOnly"
            "Queue"="Geometry"
        }
        Pass {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForwardOnly" }
            HLSLPROGRAM
            // compile directives
            #pragma vertex vert_surf
            #pragma fragment frag_surf
            
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma target 4.5
            
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
            #include "noise.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "../../Headers/PerlinNoise.cginc"

            struct appdata{
                float4 vertex: POSITION;
                float2 texcoord: TEXCOORD0;
                float3 normal: NORMAL;
                float3 color: COLOR0;
                uint id : SV_VertexID;
                uint inst : SV_InstanceID;
                float2 staticLightmapUV: TEXCOORD1;
            };
            struct Props{
                float3 pos, normal;
                float4x4 trs;
                int colorIndex;  
            };

            struct Input
            {
                float2 uv_MainTex;
                float3 worldPos;
                float2 uv2;
                float3 localPos;
                int colId;
                float4 shadowCoord;
            };

            struct SurfaceOutputStandard{
                float4 Albedo;
                float Emission;
                float Metallic;
                float Smoothness;
                float Alpha;
                float3 Normal;
            };

            // CBUFFER_START(UnityPerMaterial)
                
            // CBUFFER_END
            CBUFFER_START(UnityPerMaterial)
                uniform sampler2D _MainTex;
                uniform half _Glossiness; 
                uniform half _Metallic;
                uniform float4 _Color; 
                uniform float _Scale;
                uniform float4 _TopColor, _BotColor, _TopColor1, _BotColor1;
                uniform float _BlendFactor;
                uniform float _SmoothnessState;
                StructuredBuffer<Props> props; 
            CBUFFER_END
            struct v2f_surf {
                float4 pos: SV_POSITION;
                float3 worldNormal : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 custompack0 : TEXCOORD2; // localPos
                float2 uv : TEXCOORD3; // colId
                float4 lmap : TEXCOORD4;
                float4 shadowCoord: TEXCOORD6;
                float fogCoord: TEXCOORD5;
                DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 8);
            };

            void vert(inout appdata data, out InputData o){
                o = (InputData)0;

                float3 worldPos = mul(props[data.inst].trs, data.vertex).xyz;        
                float2 offsetX = worldPos.xy / _Scale + float2(_Time.y / 1.5, 0);
                float2 offsetY = worldPos.xy / _Scale + float2(0, _Time.y / 1.5);
                float perlinVal = perlinNoise(offsetX) - 0.5;
                float perlinVal2 = perlinNoise(offsetY) - 0.5;
                float4 newPos = float4(worldPos, data.vertex.z) + float4(perlinVal * data.color.x, perlinVal2 * data.color.x, 0, 0);
                data.vertex = newPos;
                data.normal = props[data.inst].normal;
                o.shadowCoord = TransformWorldToShadowCoord(data.vertex.xyz);
                
            } 
            void surf (Input i, inout SurfaceData o)
            {
                int colId = i.colId;
                float4 topCol = _TopColor;
                float4 botCol = _BotColor;
                
                float4 c = tex2D (_MainTex, i.uv_MainTex) * _Color;
                
                float4 shadowAtten = MainLightRealtimeShadow(i.shadowCoord);
                o.albedo = lerp(topCol, botCol, i.uv_MainTex.y).xyz;
                o.emission = 0.2;
                o.metallic = _Metallic;
                o.smoothness = lerp(0.1, _Glossiness, _SmoothnessState);         
                o.alpha = c.a;
            }

            float4 ComputeScreenPosCustom (float4 p)
            {
                float4 o = p * 0.5;
                return float4(o.x + o.w, o.y*_ProjectionParams.x + o.w, p.zw);    
            }

            v2f_surf vert_surf (appdata v){
                v2f_surf o;
                InputData customInputData = (InputData)0;
                vert (v, customInputData);
                o.pos = float4(TransformObjectToHClip(v.vertex.xyz) );
                o.uv = v.texcoord;
                float3 worldPos = v.vertex.xyz;
                float3 worldNormal = v.normal.xyz;
                o.worldPos.xyz = worldPos;
                o.worldNormal = worldNormal;
                o.shadowCoord = TransformWorldToShadowCoord(worldPos);
                o.fogCoord = InitializeInputDataFog(float4(o.worldPos.xyz, 1), ComputeFogFactor(o.pos.z));;
                OUTPUT_LIGHTMAP_UV(v.staticLightmapUV, unity_LightmapST, o.staticLightmapUV);
                OUTPUT_SH(o.worldNormal.xyz, o.vertexSH);
                return o;
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

            float4 frag_surf (v2f_surf IN) : SV_Target{
                // prepare and unpack data
                Input surfIN = (Input)0;

                surfIN.uv_MainTex = IN.uv;
                surfIN.worldPos.x = 1.0;
                surfIN.uv2.x = 1.0;
                surfIN.localPos.x = 1.0;
                surfIN.localPos = IN.custompack0.xyz;
                
                float3 worldPos = IN.worldPos.xyz;
                float3 worldViewDir = normalize(GetWorldSpaceViewDir(worldPos));

                SurfaceData o = (SurfaceData)0;
                
                o.albedo = 0.0;
                o.emission = 0.0;
                o.alpha = 0.0;
                o.occlusion = 1;
                float3 normalWorldVertex = float3(0,0,1);
                o.normalTS = float3(1, 0, 0);
                normalWorldVertex = IN.worldNormal;

                // call surface function
                surf (surfIN, o);

                InputData inputData = (InputData)0;
                inputData.positionWS = IN.worldPos;
                inputData.positionCS = IN.pos;
                inputData.fogCoord = IN.fogCoord;
                inputData.shadowCoord = IN.shadowCoord;
                inputData.normalWS = NormalizeNormalPerPixel(IN.worldNormal);
                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(inputData.positionWS);
                inputData.bakedGI = SAMPLE_GI(IN.staticLightmapUV, IN.vertexSH, inputData.normalWS);

                float2 lightmapYV;
                float4 c = 0;

                c = CustomPBRLit(inputData, o);
                c.rgb = MixFog(c.rgb, inputData.fogCoord);
                return c;
            }

            ENDHLSL
        }
        // Pass{
            //     Tags {"LightMode"="ShadowCaster" }
            //     Cull Off   
            
            
            //     HLSLPROGRAM
            //     #pragma vertex vert
            //     #pragma fragment frag

            //     #include "../../Headers/PerlinNoise.cginc"
            //     #include "UnityCG.cginc"
            //     #include "Lighting.cginc"
            //     #include "UnityPBSLighting.cginc"
            //     #include "AutoLight.cginc"

            //     struct Props{
                //         float3 pos, normal;
                //         float4x4 trs;
                //         int colorIndex;  
            //     };

            //     sampler2D _MainTex;
            //     float4 _MainTex_ST;

            //     half _Glossiness; 
            //     half _Metallic;
            //     fixed4 _Color; 
            //     float _Scale;
            //     float4 _TopColor, _BotColor, _TopColor1, _BotColor1;
            //     float _BlendFactor;
            //     float _SmoothnessState;

            //     StructuredBuffer<Props> props;
            //     void vert (inout float4 vertex:POSITION,inout float2 uv:TEXCOORD0, inout float4 color:COLOR, uint i:SV_InstanceID)
            //     {              
                //         float3 worldPos = mul(props[i].trs, vertex).xyz;
                //         float2 offsetX = worldPos.xy / _Scale + float2(_Time.y / 1.5, 0);
                //         float2 offsetY = worldPos.xy / _Scale + float2(0, _Time.y / 1.5);
                //         float perlinVal = perlinNoise(offsetX) - 0.5;
                //         float perlinVal2 = perlinNoise(offsetY) - 0.5;
                //         float4 newPos = float4(worldPos, vertex.z) + float4(perlinVal * color.x, perlinVal2 * color.x, 0, 0);
                //         vertex = UnityObjectToClipPos(newPos);
                //         vertex = UnityApplyLinearShadowBias(vertex);
            //     }
            //     float4 frag (float4 vertex:POSITION, float2 uv:TEXCOORD0) : SV_Target
            //     {
                //         #ifndef SHADOWS_DEPTH
                //             if (any(unity_LightShadowBias)) {
                    //                 // not the camera depth texture
                    //                 clip(-1); // replace with whatever
                //             }
                //         #endif
                //         return 0;
            //     }
            //     ENDHLSL
        // }

        
    }   
} 