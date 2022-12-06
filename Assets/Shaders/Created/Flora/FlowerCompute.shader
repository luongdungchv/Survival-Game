Shader "Environment/Flora/Flower Compute"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _MainTex1 ("Albedo (RGB) 1", 2D) = "white" {}
        _MainTex2 ("Albedo (RGB) 2", 2D) = "white" {}

        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Scale ("Noise Scale", float) = 1
        _TopColor ("Top Color", Color) = (1,1,1,1)
        _BotColor ("Bot Color", Color) = (1,1,1,1)
        _BlendFactor("Blend Factor", float) = 0.5
        _SmoothnessState("Smoothness State", float) = 0
        _WindSpeed("Wind Speed", float) = 1
        _WindStrength("Wind Strength", float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        Cull Off

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard vertex:vert

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 5.0 
        
        #include "../../Headers/PerlinNoise.cginc"
        #include "UnityCG.cginc"

        sampler2D _MainTex;
        sampler2D _MainTex1;
        sampler2D _MainTex2;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
            float2 uv2;
            float3 localPos;
            int texIndex;
        };
        struct appdata{
            float4 vertex: POSITION;
            float2 texcoord: TEXCOORD0;
            float3 normal: NORMAL;
            float4 tangent: TANGENT;
            UNITY_VERTEX_INPUT_INSTANCE_ID
            uint id : SV_VertexID;
            uint inst : SV_InstanceID;
        };
        struct InstanceData{
            float3 pos;
            float4x4 trs;
            int texIndex;  
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        float _Scale;
        float4 _TopColor;
        float4 _BotColor;
        float _BlendFactor;
        float _SmoothnessState;
        float _WindSpeed;
        float _WindStrength;
        
        #ifdef SHADER_API_D3D11
            StructuredBuffer<InstanceData> instDatas; 
        #endif
        
        void vert(inout appdata data, out Input o){
            UNITY_INITIALIZE_OUTPUT(Input, o);
            UNITY_SETUP_INSTANCE_ID(data)
            #ifdef SHADER_API_D3D11
                float3 worldPos = mul(instDatas[data.inst].trs, data.vertex).xyz;
                float3 worldOrigin = instDatas[data.inst].pos;   
                
                float2 offsetX = worldOrigin.xy / 5 + float2(_Time.y * _WindSpeed, 0);
                float2 offSetY = worldOrigin.xy / 5 + float2(0, _Time.y * _WindSpeed );
                float perlinVal = perlinNoise(offsetX) - 0.5;
                float perlinVal2 = perlinNoise(offSetY) - 0.5;
                float4 newPos = float4(worldPos, 0) + float4(perlinVal * _WindStrength , 0, perlinVal2 * _WindStrength, 0);
                data.vertex = newPos;
                data.normal = float3(0,1,0);
                o.texIndex = instDatas[data.inst].texIndex;
            #endif
        }
        
        void surf (Input i, inout SurfaceOutputStandard o)
        {    
            float texId = i.texIndex;
            float4 col ;
            
            if(texId == 0) col = tex2D(_MainTex, i.uv_MainTex);
            else if(texId == 1) col = tex2D(_MainTex1, i.uv_MainTex);
            else if (texId == 2) col = tex2D(_MainTex2, i.uv_MainTex);
            
            o.Albedo = col;
            //o.Emission = 0.2;
            o.Smoothness = 0;
            o.Metallic = 0;
            
        }
        ENDCG
    }
}
