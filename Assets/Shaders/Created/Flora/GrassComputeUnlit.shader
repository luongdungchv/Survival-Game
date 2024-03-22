Shader "Unlit/GrassComputeUnlit"
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

    CGINCLUDE
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
    ENDCG

    SubShader
    {
        Tags { "IgnoreProjector"="True" "RenderType"="Grass" "DisableBatching"="True" }
        LOD 100
        Cull Off

        Pass
        {
            Tags { "LightMode" = "ForwardBase" }
            CGPROGRAM
            #pragma multi_compile_fwdbase

            

            struct appdata{
                float4 vertex: POSITION;
                float2 texcoord: TEXCOORD0;
                float3 normal: NORMAL;
                float3 color: COLOR0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                uint id : SV_VertexID;
                uint inst : SV_InstanceID;
            };
            

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 worldPos: TEXCOORD1;
                float3 worldNormal: NORMAL;
                float4 _ShadowCoord : TEXCOORD2;
            };

            

            float4 ComputeScreenPosCustom (float4 p)
            {
                float4 o = p * 0.5;
                return float4(o.x + o.w, o.y*_ProjectionParams.x + o.w, p.zw);    
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
                return diffuseTerm;
                return half4(_LightColor0.rgb * diffuseTerm, 1);
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


            v2f vert (appdata data)
            {
                v2f o;
                
                float3 worldPos = mul(props[data.inst].trs, data.vertex).xyz;        
                float2 offsetX = worldPos.xy / _Scale + float2(_Time.y / 1.5, 0);
                float2 offsetY = worldPos.xy / _Scale + float2(0, _Time.y / 1.5);
                float perlinVal = perlinNoise(offsetX) - 0.5;
                float perlinVal2 = perlinNoise(offsetY) - 0.5;
                float4 newPos = float4(worldPos, data.vertex.z) + float4(perlinVal * data.color.x, perlinVal2 * data.color.x, 0, 0);
                o.vertex = UnityObjectToClipPos(newPos);
                o.worldPos = newPos;
                //data.vertex = float4(worldPos, 1);
                o.worldNormal = props[data.inst].normal;
                o.uv = data.texcoord;
                o._ShadowCoord = ComputeScreenPosCustom(o.vertex);

                return o;
            }

            
            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float4 topCol = _TopColor;
                float4 botCol = _BotColor;

                fixed4 c = lerp(topCol, botCol, i.uv.y);

                // float3 worldPos = i.worldPos.xyz;
                // fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
                // float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));

                // SurfaceOutputStandard o = (SurfaceOutputStandard)0;
                // o.Albedo = lerp(topCol, botCol, i.uv.y);
                // o.Emission = 0.2;
                // o.Metallic = _Metallic; 
                // o.Smoothness = lerp(0.1, _Glossiness, _SmoothnessState);
                // o.Occlusion = 1;

                // UNITY_LIGHT_ATTENUATION(atten, i, worldPos)
                // fixed4 c = 0;

                // UnityGI gi;
                // UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
                // gi.indirect.diffuse = 0;
                // gi.indirect.specular = 0;
                // gi.light.color = _LightColor0.rgb;
                // gi.light.dir = lightDir;
                // // Call GI (lightmaps/SH/reflections) lighting function
                // UnityGIInput giInput;
                // UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
                // giInput.light = gi.light;
                // giInput.worldPos = worldPos;
                // giInput.worldViewDir = worldViewDir;
                // giInput.atten = atten;

                // LightingStandard_GI(o, giInput, gi);
                // c += LightingStandardCustom (o, worldViewDir, gi);

                // float4 shadow = SHADOW_ATTENUATION(i);
                // c *= shadow;
                // apply fog
                return c;
            }
            ENDCG
        }
        Pass{
            Tags {"LightMode"="ShadowCaster" }
            Cull Off                  
            CGPROGRAM
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
            ENDCG
        }
    }
}
