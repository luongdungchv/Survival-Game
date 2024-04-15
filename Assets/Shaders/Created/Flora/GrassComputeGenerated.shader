// Upgrade NOTE: replaced 'defined FOG_COMBINED_WITH_WORLD_POS' with 'defined (FOG_COMBINED_WITH_WORLD_POS)'

Shader "Environment/Flora/Grass Compute 2"
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
        Tags { "RenderType"="Opaque" }
        LOD 200
        Cull Off
        
        // ------------------------------------------------------------
        // Surface shader code generated out of a CGPROGRAM block:
        

        // ---- forward rendering base pass:
        Pass {
            Name "FORWARD"
            Tags { "IgnoreProjector"="True" "RenderType"="Grass" "DisableBatching"="True" "LightMode" = "UniversalForward"}

            HLSLPROGRAM
            // compile directives
            #pragma vertex vert_surf
            #pragma fragment frag_surf
            #pragma target 5.0
            #pragma multi_compile_instancing
            #pragma multi_compile_fog
            #pragma multi_compile_fwdbase nodynlightmap nolightmap
            #include "HLSLSupport.cginc"
            #define UNITY_INSTANCED_LOD_FADE
            #define UNITY_INSTANCED_SH
            #define UNITY_INSTANCED_LIGHTMAPSTS
            #include "UnityShaderVariables.cginc"
            #include "UnityShaderUtilities.cginc"
            #if !defined(INSTANCING_ON)
                #include "UnityCG.cginc"
                #undef UNITY_SHOULD_SAMPLE_SH
                #define UNITY_SHOULD_SAMPLE_SH (!defined(UNITY_PASS_FORWARDADD) && !defined(UNITY_PASS_PREPASSBASE) && !defined(UNITY_PASS_SHADOWCASTER) && !defined(UNITY_PASS_META))
                #include "Lighting.cginc"
                #include "UnityPBSLighting.cginc"
                #include "AutoLight.cginc"

                #define INTERNAL_DATA
                #define WorldReflectionVector(data,normal) data.worldRefl
                #define WorldNormalVector(data,normal) normal

                #line 26 ""
                #ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
                #endif

                #include "../../Headers/PerlinNoise.cginc"
                #include "UnityCG.cginc"

                sampler2D _MainTex;

                struct Input
                {
                    float2 uv_MainTex;
                    float3 worldPos;
                    float2 uv2;
                    float3 localPos;
                    int colId;
                };
                struct appdata{
                    float4 vertex: POSITION;
                    float2 texcoord: TEXCOORD0;
                    float3 normal: NORMAL;
                    float3 color: COLOR0;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                    uint id : SV_VertexID;
                    uint inst : SV_InstanceID;
                };
                struct Props{
                    float3 pos, normal;
                    float4x4 trs;
                    int colorIndex;  
                };

                half _Glossiness; 
                half _Metallic;
                fixed4 _Color; 
                float _Scale;
                float4 _TopColor, _BotColor, _TopColor1, _BotColor1;
                float _BlendFactor;
                float _SmoothnessState;
                
                
                #ifdef SHADER_API_D3D11
                    StructuredBuffer<Props> props; 
                #endif
                void vert(inout appdata data, out Input o){
                    UNITY_SETUP_INSTANCE_ID(data)
                    UNITY_INITIALIZE_OUTPUT(Input,o);
                    #ifdef SHADER_API_D3D11
                        float3 worldPos = mul(props[data.inst].trs, data.vertex).xyz;        
                        float2 offsetX = worldPos.xy / _Scale + float2(_Time.y / 1.5, 0);
                        float2 offsetY = worldPos.xy / _Scale + float2(0, _Time.y / 1.5);
                        float perlinVal = perlinNoise(offsetX) - 0.5;
                        float perlinVal2 = perlinNoise(offsetY) - 0.5;
                        float4 newPos = float4(worldPos, data.vertex.z) + float4(perlinVal * data.color.x, perlinVal2 * data.color.x, 0, 0);
                        data.vertex = newPos;
                        data.normal = props[data.inst].normal;
                        o.colId = props[data.inst].colorIndex;
                    #endif
                } 
                
                void surf (Input i, inout SurfaceOutputStandard o)
                {
                    #ifdef SHADER_API_D3D11
                        int colId = i.colId;
                        float4 topCol = _TopColor;
                        float4 botCol = _BotColor;
                        
                        fixed4 c = tex2D (_MainTex, i.uv_MainTex) * _Color;
                        
                        o.Albedo = lerp(topCol, botCol, i.uv_MainTex.y);
                        o.Emission = 0.2;
                        o.Metallic = _Metallic;
                        o.Smoothness = lerp(0.1, _Glossiness, _SmoothnessState);
                        
                        o.Alpha = c.a;
                    #endif
                }
                

                #ifndef LIGHTMAP_ON
                    #ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
                        #define FOG_COMBINED_WITH_WORLD_POS
                        struct v2f_surf {
                            UNITY_POSITION(pos);
                            float3 worldNormal : TEXCOORD0;
                            float4 worldPos : TEXCOORD1;
                            float3 custompack0 : TEXCOORD2; // localPos
                            float2 uv : TEXCOORD3; // colId
                            #if UNITY_SHOULD_SAMPLE_SH
                                half3 sh : TEXCOORD4; // SH
                            #endif
                            UNITY_LIGHTING_COORDS(5,6)
                            UNITY_VERTEX_INPUT_INSTANCE_ID
                            UNITY_VERTEX_OUTPUT_STEREO
                        };
                    #endif
                    #ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
                        struct v2f_surf {
                            UNITY_POSITION(pos);
                            float3 worldNormal : TEXCOORD0;
                            float3 worldPos : TEXCOORD1;
                            float3 custompack0 : TEXCOORD2; // localPos
                            float2 uv : TEXCOORD3; // colId
                            #if UNITY_SHOULD_SAMPLE_SH
                                half3 sh : TEXCOORD4; // SH
                            #endif
                            UNITY_FOG_COORDS(5)
                            UNITY_SHADOW_COORDS(6)
                            UNITY_VERTEX_INPUT_INSTANCE_ID
                            UNITY_VERTEX_OUTPUT_STEREO
                        };
                    #endif
                #endif
                #ifdef LIGHTMAP_ON
                    #ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
                        #define FOG_COMBINED_WITH_WORLD_POS
                        struct v2f_surf {
                            UNITY_POSITION(pos);
                            float3 worldNormal : TEXCOORD0;
                            float4 worldPos : TEXCOORD1;
                            float3 custompack0 : TEXCOORD2; // localPos
                            float2 uv : TEXCOORD3; // colId
                            float4 lmap : TEXCOORD4;
                            UNITY_LIGHTING_COORDS(5,6)
                            UNITY_VERTEX_INPUT_INSTANCE_ID
                            UNITY_VERTEX_OUTPUT_STEREO
                        };
                    #endif
                    #ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
                        struct v2f_surf {
                            UNITY_POSITION(pos);
                            float3 worldNormal : TEXCOORD0;
                            float3 worldPos : TEXCOORD1;
                            float3 custompack0 : TEXCOORD2; // localPos
                            float2 uv : TEXCOORD3; // colId
                            float4 lmap : TEXCOORD4;
                            UNITY_FOG_COORDS(5)
                            UNITY_SHADOW_COORDS(6)
                            UNITY_VERTEX_INPUT_INSTANCE_ID
                            UNITY_VERTEX_OUTPUT_STEREO
                        };
                    #endif
                #endif

                float4 ComputeScreenPosCustom (float4 p)
                {
                    float4 o = p * 0.5;
                    return float4(o.x + o.w, o.y*_ProjectionParams.x + o.w, p.zw);    
                }

                // vertex shader
                v2f_surf vert_surf (appdata v) {
                    UNITY_SETUP_INSTANCE_ID(v);
                    v2f_surf o;
                    UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
                    UNITY_TRANSFER_INSTANCE_ID(v,o);
                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                    Input customInputData;
                    vert (v, customInputData);
                    o.custompack0.xyz = customInputData.localPos;
                    o.pos = UnityObjectToClipPos(v.vertex);
                    o.uv = v.texcoord;
                    float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                    float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                    #if defined(LIGHTMAP_ON) && defined(DIRLIGHTMAP_COMBINED)
                        fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
                        fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
                        fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
                    #endif
                    #if defined(LIGHTMAP_ON) && defined(DIRLIGHTMAP_COMBINED) && !defined(UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS)
                        o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
                        o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
                        o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
                    #endif
                    o.worldPos.xyz = worldPos;
                    o.worldNormal = worldNormal;
                    #ifdef LIGHTMAP_ON
                        o.lmap.xy = half2(0.0, 0.0) * unity_LightmapST.xy + unity_LightmapST.zw;
                    #endif

                    #ifndef LIGHTMAP_ON
                        #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
                            o.sh = 0;
                            #ifdef VERTEXLIGHT_ON
                                o.sh += Shade4PointLights (
                                unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
                                unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
                                unity_4LightAtten0, worldPos, worldNormal);
                            #endif
                            o.sh = ShadeSHPerVertex (worldNormal, o.sh);
                        #endif
                    #endif // !LIGHTMAP_ON

                    UNITY_TRANSFER_LIGHTING(o,half2(0.0, 0.0)); // pass shadow and, possibly, light cookie coordinates to pixel shader
                    #ifdef FOG_COMBINED_WITH_TSPACE
                        UNITY_TRANSFER_FOG_COMBINED_WITH_TSPACE(o,o.pos); // pass fog coordinates to pixel shader
                    #elif defined (FOG_COMBINED_WITH_WORLD_POS)
                        UNITY_TRANSFER_FOG_COMBINED_WITH_WORLD_POS(o,o.pos); // pass fog coordinates to pixel shader
                    #else
                        UNITY_TRANSFER_FOG(o,o.pos); // pass fog coordinates to pixel shader
                    #endif
                    o._ShadowCoord = ComputeScreenPosCustom(o.pos);
                    return o;
                }
                half4 CalcBRDF (half3 diffColor, half3 specColor, half oneMinusReflectivity, half smoothness,
                float3 normal, float3 viewDir,
                UnityLight light, UnityIndirect gi)
                {
                    float perceptualRoughness = SmoothnessToPerceptualRoughness (smoothness);
                    float3 halfDir = Unity_SafeNormalize (float3(light.dir) + viewDir);

                    #define UNITY_HANDLE_CORRECTLY_NEGATIVE_NDOTV 0

                    #if UNITY_HANDLE_CORRECTLY_NEGATIVE_NDOTV
                        half shiftAmount = dot(normal, viewDir);
                        normal = shiftAmount < 0.0f ? normal + viewDir * (-shiftAmount + 1e-5f) : normal;
                        float nv = saturate(dot(normal, viewDir)); // TODO: this saturate should no be necessary here
                    #else
                        half nv = abs(dot(normal, viewDir));    // This abs allow to limit artifact
                    #endif

                    float nl = saturate(dot(normal, light.dir));

                    float ambientSkyNL = saturate(dot(normal, float3(0, 1, 0)));
                    float ambientEquatorNL = saturate(dot(normal, float3(1, 0, 0)));

                    float nh = saturate(dot(normal, halfDir));

                    half lv = saturate(dot(light.dir, viewDir));
                    half lh = saturate(dot(light.dir, halfDir));

                    half diffuseTerm = DisneyDiffuse(nv, nl, lh, perceptualRoughness) * nl;
                    half diffuseAmbientSky = DisneyDiffuse(nv, ambientSkyNL, lh, perceptualRoughness) * ambientSkyNL;
                    half diffuseAmbientEquator = DisneyDiffuse(nv, ambientEquatorNL, lh, perceptualRoughness) * ambientEquatorNL;

                    float roughness = PerceptualRoughnessToRoughness(perceptualRoughness);
                    #if UNITY_BRDF_GGX
                        roughness = max(roughness, 0.002);
                        float V = SmithJointGGXVisibilityTerm (nl, nv, roughness);
                        float D = GGXTerm (nh, roughness);
                    #else
                        half V = SmithBeckmannVisibilityTerm (nl, nv, roughness);
                        half D = NDFBlinnPhongNormalizedTerm (nh, PerceptualRoughnessToSpecPower(perceptualRoughness));
                    #endif

                    float specularTerm = V*D * UNITY_PI; // Torrance-Sparrow model, Fresnel is applied later

                    #   ifdef UNITY_COLORSPACE_GAMMA
                    specularTerm = sqrt(max(1e-4h, specularTerm));
                    #   endif

                    specularTerm = max(0, specularTerm * nl);
                    #if defined(_SPECULARHIGHLIGHTS_OFF)
                        specularTerm = 0.0;
                    #endif

                    half surfaceReduction;
                    #   ifdef UNITY_COLORSPACE_GAMMA
                    surfaceReduction = 1.0-0.28*roughness*perceptualRoughness;      // 1-0.28*x^3 as approximation for (1/(x^4+1))^(1/2.2) on the domain [0;1]
                    #   else
                    surfaceReduction = 1.0 / (roughness*roughness + 1.0);           // fade \in [0.5;1]
                    #   endif

                    specularTerm *= any(specColor) ? 1.0 : 0.0;

                    half grazingTerm = saturate(smoothness + (1-oneMinusReflectivity));
                    half3 color =   diffColor 
                    * (gi.diffuse + _LightColor0.rgb * diffuseTerm)
                    + specularTerm * _LightColor0.rgb * FresnelTerm (specColor, lh)
                    + surfaceReduction * gi.specular * FresnelLerp (specColor, grazingTerm, nv);
                    ;
                    
                    return half4(color, 1);
                }


                inline half4 LightingStandardCustom (SurfaceOutputStandard s, half3 viewDir, UnityGI gi)
                {
                    s.Normal = normalize(s.Normal);

                    half oneMinusReflectivity;
                    half3 specColor = 1;
                    s.Albedo = DiffuseAndSpecularFromMetallic (s.Albedo, s.Metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);

                    // shader relies on pre-multiply alpha-blend (_SrcBlend = One, _DstBlend = OneMinusSrcAlpha)
                    // this is necessary to handle transparency in physically correct way - only diffuse component gets affected by alpha
                    half outputAlpha = 1;
                    s.Albedo = PreMultiplyAlpha (s.Albedo, s.Alpha, oneMinusReflectivity, /*out*/ outputAlpha);

                    
                    half4 c = CalcBRDF (s.Albedo, specColor, oneMinusReflectivity, s.Smoothness, s.Normal, viewDir, gi.light, gi.indirect);
                    return c;
                    c.rgb += UNITY_BRDF_GI (s.Albedo, specColor, oneMinusReflectivity, s.Smoothness, s.Normal, viewDir, s.Occlusion, gi);
                    c.a = outputAlpha;
                    return c;
                }

                // fragment shader
                fixed4 frag_surf (v2f_surf IN) : SV_Target {
                    UNITY_SETUP_INSTANCE_ID(IN);
                    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
                    // prepare and unpack data
                    Input surfIN;
                    #ifdef FOG_COMBINED_WITH_TSPACE
                        UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
                    #elif defined (FOG_COMBINED_WITH_WORLD_POS)
                        UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
                    #else
                        UNITY_EXTRACT_FOG(IN);
                    #endif
                    UNITY_INITIALIZE_OUTPUT(Input,surfIN);
                    surfIN.uv_MainTex = IN.uv;
                    surfIN.worldPos.x = 1.0;
                    surfIN.uv2.x = 1.0;
                    surfIN.localPos.x = 1.0;
                    surfIN.localPos = IN.custompack0.xyz;
                    float3 worldPos = IN.worldPos.xyz;
                    #ifndef USING_DIRECTIONAL_LIGHT
                        fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
                    #else
                        fixed3 lightDir = _WorldSpaceLightPos0.xyz;
                    #endif
                    float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                    #ifdef UNITY_COMPILER_HLSL
                        SurfaceOutputStandard o = (SurfaceOutputStandard)0;
                    #else
                        SurfaceOutputStandard o;
                    #endif
                    o.Albedo = 0.0;
                    o.Emission = 0.0;
                    o.Alpha = 0.0;
                    o.Occlusion = 1.0;
                    fixed3 normalWorldVertex = fixed3(0,0,1);
                    o.Normal = IN.worldNormal;
                    normalWorldVertex = IN.worldNormal;

                    // call surface function
                    surf (surfIN, o);

                    // compute lighting & shadowing factor
                    UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
                    fixed4 c = 0;

                    // Setup lighting environment
                    UnityGI gi;
                    UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
                    gi.indirect.diffuse = 0;
                    gi.indirect.specular = 0;
                    gi.light.color = _LightColor0.rgb;
                    gi.light.dir = lightDir;
                    // Call GI (lightmaps/SH/reflections) lighting function
                    UnityGIInput giInput;
                    UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
                    giInput.light = gi.light;
                    giInput.worldPos = worldPos;
                    giInput.worldViewDir = worldViewDir;
                    giInput.atten = atten;
                    #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
                        giInput.lightmapUV = IN.lmap;
                    #else
                        giInput.lightmapUV = 0.0;
                    #endif
                    #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
                        giInput.ambient = IN.sh;
                    #else
                        giInput.ambient.rgb = 0.0;
                    #endif
                    // giInput.probeHDR[0] = unity_SpecCube0_HDR;
                    // giInput.probeHDR[1] = unity_SpecCube1_HDR;
                    // #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
                    //     giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
                    // #endif
                    // #ifdef UNITY_SPECCUBE_BOX_PROJECTION
                    //     giInput.boxMax[0] = unity_SpecCube0_BoxMax;
                    //     giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
                    //     giInput.boxMax[1] = unity_SpecCube1_BoxMax;
                    //     giInput.boxMin[1] = unity_SpecCube1_BoxMin;
                    //     giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
                    // #endif
                    LightingStandard_GI(o, giInput, gi);

                    // realtime lighting: call lighting function
                    c += LightingStandard (o, worldViewDir, gi);
                    // fixed4 shadow = SHADOW_ATTENUATION(IN);
                    // c *= shadow;
                    return c;
                }


            #endif

            // -------- variant for: INSTANCING_ON 



            ENDHLSL

        }
        Pass{
            Tags {"LightMode"="ShadowCaster" }
            Cull Off   
            
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "../../Headers/PerlinNoise.cginc"
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "AutoLight.cginc"

            struct Props{
                float3 pos, normal;
                float4x4 trs;
                int colorIndex;  
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            half _Glossiness; 
            half _Metallic;
            fixed4 _Color; 
            float _Scale;
            float4 _TopColor, _BotColor, _TopColor1, _BotColor1;
            float _BlendFactor;
            float _SmoothnessState;

            StructuredBuffer<Props> props;
            void vert (inout float4 vertex:POSITION,inout float2 uv:TEXCOORD0, inout float4 color:COLOR, uint i:SV_InstanceID)
            {              
                float3 worldPos = mul(props[i].trs, vertex).xyz;
                float2 offsetX = worldPos.xy / _Scale + float2(_Time.y / 1.5, 0);
                float2 offsetY = worldPos.xy / _Scale + float2(0, _Time.y / 1.5);
                float perlinVal = perlinNoise(offsetX) - 0.5;
                float perlinVal2 = perlinNoise(offsetY) - 0.5;
                float4 newPos = float4(worldPos, vertex.z) + float4(perlinVal * color.x, perlinVal2 * color.x, 0, 0);
                vertex = UnityObjectToClipPos(newPos);
                vertex = UnityApplyLinearShadowBias(vertex);
            }
            float4 frag (float4 vertex:POSITION, float2 uv:TEXCOORD0) : SV_Target
            {
                #ifndef SHADOWS_DEPTH
                    if (any(unity_LightShadowBias)) {
                        // not the camera depth texture
                        clip(-1); // replace with whatever
                    }
                #endif
                return 0;
            }
            ENDHLSL
        }

        

        #LINE 111

    }
    //FallBack "Diffuse" 
}
