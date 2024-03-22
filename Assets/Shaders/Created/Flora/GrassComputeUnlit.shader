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
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Cull Off

        Pass
        {
            Tags { "IgnoreProjector"="True" "RenderType"="Grass" "DisableBatching"="True" "LightMode" = "ForwardBase"}
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "../../Headers/PerlinNoise.cginc"
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "AutoLight.cginc"

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

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 worldPos: TEXCOORD1;
                float3 worldNormal: NORMAL;
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

                return o;
            }

            half4 CalcBRDF (half3 diffColor, half3 specColor, half oneMinusReflectivity, half smoothness,
            float3 normal, float3 viewDir,
            UnityLight light, UnityIndirect gi)
            {
                float perceptualRoughness = SmoothnessToPerceptualRoughness (smoothness);
                float3 halfDir = Unity_SafeNormalize (float3(light.dir) + viewDir);

                // NdotV should not be negative for visible pixels, but it can happen due to perspective projection and normal mapping
                // In this case normal should be modified to become valid (i.e facing camera) and not cause weird artifacts.
                // but this operation adds few ALU and users may not want it. Alternative is to simply take the abs of NdotV (less correct but works too).
                // Following define allow to control this. Set it to 0 if ALU is critical on your platform.
                // This correction is interesting for GGX with SmithJoint visibility function because artifacts are more visible in this case due to highlight edge of rough surface
                // Edit: Disable this code by default for now as it is not compatible with two sided lighting used in SpeedTree.
                #define UNITY_HANDLE_CORRECTLY_NEGATIVE_NDOTV 0

                #if UNITY_HANDLE_CORRECTLY_NEGATIVE_NDOTV
                    // The amount we shift the normal toward the view vector is defined by the dot product.
                    half shiftAmount = dot(normal, viewDir);
                    normal = shiftAmount < 0.0f ? normal + viewDir * (-shiftAmount + 1e-5f) : normal;
                    // A re-normalization should be applied here but as the shift is small we don't do it to save ALU.
                    //normal = normalize(normal);

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

                // Diffuse term
                half diffuseTerm = DisneyDiffuse(nv, nl, lh, perceptualRoughness) * nl;
                half diffuseAmbientSky = DisneyDiffuse(nv, ambientSkyNL, lh, perceptualRoughness) * ambientSkyNL;
                half diffuseAmbientEquator = DisneyDiffuse(nv, ambientEquatorNL, lh, perceptualRoughness) * ambientEquatorNL;

                // Specular term
                // HACK: theoretically we should divide diffuseTerm by Pi and not multiply specularTerm!
                // BUT 1) that will make shader look significantly darker than Legacy ones
                // and 2) on engine side "Non-important" lights have to be divided by Pi too in cases when they are injected into ambient SH
                float roughness = PerceptualRoughnessToRoughness(perceptualRoughness);
                #if UNITY_BRDF_GGX
                    // GGX with roughtness to 0 would mean no specular at all, using max(roughness, 0.002) here to match HDrenderloop roughtness remapping.
                    roughness = max(roughness, 0.002);
                    float V = SmithJointGGXVisibilityTerm (nl, nv, roughness);
                    float D = GGXTerm (nh, roughness);
                #else
                    // Legacy
                    half V = SmithBeckmannVisibilityTerm (nl, nv, roughness);
                    half D = NDFBlinnPhongNormalizedTerm (nh, PerceptualRoughnessToSpecPower(perceptualRoughness));
                #endif

                float specularTerm = V*D * UNITY_PI; // Torrance-Sparrow model, Fresnel is applied later

                #   ifdef UNITY_COLORSPACE_GAMMA
                specularTerm = sqrt(max(1e-4h, specularTerm));
                #   endif

                // specularTerm * nl can be NaN on Metal in some cases, use max() to make sure it's a sane value
                specularTerm = max(0, specularTerm * nl);
                #if defined(_SPECULARHIGHLIGHTS_OFF)
                    specularTerm = 0.0;
                #endif

                // surfaceReduction = Int D(NdotH) * NdotH * Id(NdotL>0) dH = 1/(roughness^2+1)
                half surfaceReduction;
                #   ifdef UNITY_COLORSPACE_GAMMA
                surfaceReduction = 1.0-0.28*roughness*perceptualRoughness;      // 1-0.28*x^3 as approximation for (1/(x^4+1))^(1/2.2) on the domain [0;1]
                #   else
                surfaceReduction = 1.0 / (roughness*roughness + 1.0);           // fade \in [0.5;1]
                #   endif

                // To provide true Lambert lighting, we need to be able to kill specular completely.
                specularTerm *= any(specColor) ? 1.0 : 0.0;

                half grazingTerm = saturate(smoothness + (1-oneMinusReflectivity));
                half3 color =   diffColor 
                * (gi.diffuse + light.color * diffuseTerm + unity_AmbientSky * diffuseAmbientSky * unity_AmbientEquator * diffuseAmbientEquator)
                // * (gi.diffuse + unity_AmbientSky * diffuseAmbientSky)
                // * (gi.diffuse + unity_AmbientEquator * diffuseAmbientEquator)
                + specularTerm * light.color * FresnelTerm (specColor, lh)
                + surfaceReduction * gi.specular * FresnelLerp (specColor, grazingTerm, nv);
                
                //return half4((gi.diffuse + light.color * diffuseTerm), 1);      
                //return unity_AmbientSky; 
                return float4((light.color * diffuseTerm + unity_AmbientSky * diffuseAmbientSky * unity_AmbientEquator * diffuseAmbientEquator), 1);
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
                c.rgb += UNITY_BRDF_GI (s.Albedo, specColor, oneMinusReflectivity, s.Smoothness, s.Normal, viewDir, s.Occlusion, gi);
                c.a = outputAlpha;
                return c;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float4 topCol = _TopColor;
                float4 botCol = _BotColor;

                SurfaceOutputStandard o = (SurfaceOutputStandard)0;
                o.Albedo = lerp(topCol, botCol, i.uv.y);
                o.Emission = 0.2;
                o.Metallic = _Metallic;
                o.Smoothness = lerp(0.1, _Glossiness, _SmoothnessState);
                o.Occlusion = 1;
                o.Normal = i.worldNormal;

                float3 worldPos = i.worldPos.xyz;
                fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
                float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
                fixed4 c = 0;

                UnityGI gi;
                UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
                gi.indirect.diffuse = 0;
                gi.indirect.specular = 0;
                gi.light.color = _LightColor0.rgb;
                gi.light.dir = lightDir;

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
                
                giInput.probeHDR[0] = unity_SpecCube0_HDR;
                giInput.probeHDR[1] = unity_SpecCube1_HDR;

                #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
                    giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
                #endif
                #ifdef UNITY_SPECCUBE_BOX_PROJECTION
                    giInput.boxMax[0] = unity_SpecCube0_BoxMax;
                    giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
                    giInput.boxMax[1] = unity_SpecCube1_BoxMax;
                    giInput.boxMin[1] = unity_SpecCube1_BoxMin;
                    giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
                #endif

                LightingStandard_GI(o, giInput, gi);
                c += LightingStandardCustom(o, worldViewDir, gi);
                // apply fog
                return c;
            }
            ENDCG
        }
    }
}
