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

    HLSLINCLUDE
    #pragma vertex vert
    #pragma fragment frag

    #include "../../Headers/PerlinNoise.cginc"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

    struct Props{
        float3 pos, normal;
        float4x4 trs;
        int colorIndex;  
    };

    sampler2D _MainTex;
    float4 _MainTex_ST;

    half _Glossiness; 
    half _Metallic;
    float4 _Color; 
    float _Scale;
    float4 _TopColor, _BotColor, _TopColor1, _BotColor1;
    float _BlendFactor;
    float _SmoothnessState;

    StructuredBuffer<Props> props; 
    ENDHLSL

    SubShader
    {
        Tags {"RenderType"="Grass" "DisableBatching"="True" }
        LOD 100
        Cull Off

        Pass
        {
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
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

            


            v2f vert (appdata data)
            {
                v2f o;
                
                float3 worldPos = mul(props[data.inst].trs, data.vertex).xyz;        
                float2 offsetX = worldPos.xy / _Scale + float2(_Time.y / 1.5, 0);
                float2 offsetY = worldPos.xy / _Scale + float2(0, _Time.y / 1.5);
                float perlinVal = perlinNoise(offsetX) - 0.5;
                float perlinVal2 = perlinNoise(offsetY) - 0.5;
                float4 newPos = float4(worldPos, data.vertex.z) + float4(perlinVal * data.color.x, perlinVal2 * data.color.x, 0, 0);
                o.vertex = TransformObjectToHClip(newPos);
                o.worldPos = newPos;
                //data.vertex = float4(worldPos, 1);
                o.worldNormal = props[data.inst].normal;
                o.uv = data.texcoord;
                o._ShadowCoord = ComputeScreenPosCustom(o.vertex);

                return o;
            }

            
            float4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float4 topCol = _TopColor;
                float4 botCol = _BotColor;

                float4 c = lerp(topCol, botCol, i.uv.y);

                return c;
            }
            ENDHLSL
        }
        // Pass{
        //     Tags {"LightMode"="ShadowCaster" }
        //     Cull Off                  
        //     HLSLPROGRAM
        //     void vert (inout float4 vertex:POSITION,inout float2 uv:TEXCOORD0, inout float4 color:COLOR, uint i:SV_InstanceID)
        //     {              
        //         float3 worldPos = mul(props[i].trs, vertex).xyz;
        //         float2 offsetX = worldPos.xy / _Scale + float2(_Time.y / 1.5, 0);
        //         float2 offsetY = worldPos.xy / _Scale + float2(0, _Time.y / 1.5);
        //         float perlinVal = perlinNoise(offsetX) - 0.5;
        //         float perlinVal2 = perlinNoise(offsetY) - 0.5;
        //         float4 newPos = float4(worldPos, vertex.z) + float4(perlinVal * color.x, perlinVal2 * color.x, 0, 0);
        //         vertex = TransformObjectToHClip(newPos);
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
