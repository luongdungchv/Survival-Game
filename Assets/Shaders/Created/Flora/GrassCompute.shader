Shader "Environment/Flora/Grass Compute"
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

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard vertex:vert

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 5.0
        
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
            UNITY_VERTEX_INPUT_INSTANCE_ID
            uint id : SV_VertexID;
            uint inst : SV_InstanceID;
        };
        struct Props{
            float3 pos;
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
                float2 offsetX = worldPos.xy / _Scale + float2(_Time.y, 0);
                float perlinVal = perlinNoise(offsetX) - 0.5;
                float4 newPos = float4(worldPos, data.vertex.z) + float4(perlinVal * data.texcoord.y, 0, 0, 0);
                data.vertex = newPos;
                o.colId = props[data.inst].colorIndex;
            #endif
        } 
        
        void surf (Input i, inout SurfaceOutputStandard o)
        {
            #ifdef SHADER_API_D3D11
                int colId = i.colId;
                float4 topCol = colId == 0 ? _TopColor : _TopColor1;
                float4 botCol = colId == 0 ? _BotColor : _BotColor1;
                
                fixed4 c = tex2D (_MainTex, i.uv_MainTex) * _Color;
                
                o.Albedo = lerp(topCol, botCol, i.uv_MainTex.y);
                //o.Albedo = colId;
                
                o.Metallic = _Metallic;
                o.Smoothness = lerp(0.1, _Glossiness, _SmoothnessState);
                //o.Smoothness = _Glossiness;
                
                o.Alpha = c.a;
            #endif
        }
        ENDCG
    }
    FallBack "Diffuse" 
}
