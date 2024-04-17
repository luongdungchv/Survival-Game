Shader "Unlit/GrassBladeIndirect"
{
    Properties
    {
        _MainTex ("Main Tex", 2D) = "white" {}
        _RenderTex ("Render Tex", 2D) = "white" {}
        _PrimaryCol ("Primary Color", Color) = (1, 1, 1)
        _SecondaryCol ("Secondary Color", Color) = (1, 0, 1)
        _AOColor ("AO Color", Color) = (1, 0, 1)
        _TipColor ("Tip Color", Color) = (0, 0, 1)
        _Scale ("Scale", Range(0.0, 2.0)) = 0.0
        _MeshDeformationLimit ("Mesh Deformation Limit", Range(0.0, 5.0)) = 0.0
        _WindNoiseScale ("Wind Noise Scale", float) = 0.0
        _WindStrength ("Wind Strength", float) = 1.0
        _WindSpeed ("Wind Speed", Vector) = (0, 0, 0, 0)
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
 
        Pass
        {
 
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForwardOnly" }
 
            HLSLPROGRAM
 
            #pragma vertex vert
            #pragma fragment frag
 
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
 
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma target 4.5
 
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma shader_feature _ALPHATEST_ON
 
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
            #include "noise.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 
            struct VertexInput
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal: NORMAL;
            };
 
            struct VertexOutput
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float4 shadowCoord: TEXCOORD2;
                float4 interp8 : INTERP8;
            };
 
            StructuredBuffer<float4x4> trsBuffer;
            sampler2D _MainTex;
            sampler2D _RenderTex;
            float4 _MainTex_ST;
            float4 _RenderTex_ST;
            float4 _PrimaryCol, _SecondaryCol, _AOColor, _TipColor;
            float _Scale;
            float4 _LightDir;
            float _MeshDeformationLimit;
            float4 _WindSpeed;
            float _WindStrength;
            float _WindNoiseScale;
 
            VertexOutput vert (VertexInput v, uint instanceID : SV_InstanceID)
            {
                VertexOutput o;
 
 
                //applying transformation matrix
                float3 positionWorldSpace = mul(trsBuffer[instanceID], float4(v.vertex.xyz, 1));
 
                //move world UVs by time
                float4 worldPos = float4(positionWorldSpace, 1);
                float2 worldUV = worldPos.xz + _WindSpeed * _Time.y;
 
                //creating noise from world UVs
                float noise = 0;
                Unity_SimpleNoise_float(worldUV, _WindNoiseScale, noise);
                noise = pow(noise, 2);
 
                //to keep bottom part of mesh at its position
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                float smoothDeformation = smoothstep(0, _MeshDeformationLimit, o.uv.y);
                float distortion = smoothDeformation * noise;
 
                //apply distortion
                positionWorldSpace.x += distortion * _WindStrength;
                o.vertex = mul(UNITY_MATRIX_VP, float4(positionWorldSpace, 1));
 
                VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);
                o.shadowCoord = TransformWorldToShadowCoord(positionWorldSpace);
 
                VertexNormalInputs normal = GetVertexNormalInputs(v.normal);
                o.normal = normalize(mul(v.normal, (float3x3)UNITY_MATRIX_I_M));
               
                return o;
            }
 
            float4 frag (VertexOutput i) : SV_Target
            {
   
                float4 col = lerp(_PrimaryCol, _SecondaryCol, i.uv.y);
 
                //from https://github.com/GarrettGunnell/Grass/blob/main/Assets/Shaders/ModelGrass.shader
                float light = clamp(dot(_LightDir, normalize(float3(0, 1, 0))), 0 , 1);
                float4 ao = lerp(_AOColor, 1.0f, i.uv.y);
                float4 tip = lerp(0.0f, _TipColor, i.uv.y * i.uv.y * (1.0f + _Scale));
                float4 grassColor = (col + tip) * light * ao;
 
                 Light mainLight = GetMainLight(i.shadowCoord);
                 float strength = dot(mainLight.direction, i.normal);
                 float4 lightColor = float4(mainLight.color, 1)*(mainLight.distanceAttenuation * mainLight.shadowAttenuation);
             
                  half receiveshadow = MainLightRealtimeShadow(i.shadowCoord);
 
                return grassColor * receiveshadow;
            }
            ENDHLSL
        }
 
//Shadow caster pass    
        Pass {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ColorMask 0
            LOD 100
            Cull Off
 
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma target 4.5
 
            #include "UnityCG.cginc"
            #include "noise.hlsl"
 
            struct VertexInput
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
 
            struct VertexOutput
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };
 
            StructuredBuffer<float4x4> trsBuffer;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _MeshDeformationLimit;
            float4 _WindSpeed;
            float _WindStrength;
            float _WindNoiseScale;
 
            VertexOutput vert (VertexInput v, uint instanceID : SV_InstanceID)
            {
                VertexOutput o;
         
                //applying transformation matrix
                float3 positionWorldSpace = mul(trsBuffer[instanceID], float4(v.vertex.xyz, 1));
 
                //move world UVs by time
                float4 worldPos = float4(positionWorldSpace, 1);
                float2 worldUV = worldPos.xz + _WindSpeed * _Time.y;
 
                //creating noise from world UVs
                float noise = 0;
                Unity_SimpleNoise_float(worldUV, _WindNoiseScale, noise);
                noise = pow(noise, 2);
 
                //to keep bottom part of mesh at its position
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                float smoothDeformation = smoothstep(0, _MeshDeformationLimit, o.uv.y);
                float distortion = smoothDeformation * noise;
 
                //apply distortion
                positionWorldSpace.x += distortion * _WindStrength;
                o.vertex = mul(UNITY_MATRIX_VP, float4(positionWorldSpace, 1));
 
                return o;
            }
 
           fixed4 frag (VertexOutput i) : SV_Target
           {
                SHADOW_CASTER_FRAGMENT(i);
           }
           ENDHLSL
       }
    }
}