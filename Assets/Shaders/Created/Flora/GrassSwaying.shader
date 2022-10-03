Shader "Environment/Flora/Grass Swaying"
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
        #pragma target 3.0 
        
        #include "../../Headers/PerlinNoise.cginc"
        #include "UnityCG.cginc"

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
            float2 uv2;
            float3 localPos;
        };
        struct appdata{
            float4 vertex: POSITION;
            float2 uv: TEXCOORD0;
            float3 normal: NORMAL;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        float _Scale;
        float4 _TopColor;
        float4 _BotColor;
        float _BlendFactor;
        float _SmoothnessState;
        
        void vert(inout appdata_full data, out Input o){
            UNITY_INITIALIZE_OUTPUT(Input, o);
            float3 worldPos = mul(unity_ObjectToWorld, data.vertex).xyz;
            
            float2 offsetX = worldPos.xy / _Scale + float2(_Time.y, 0);
            float perlinVal = perlinNoise(offsetX) - 0.5;
            float4 newPos = data.vertex + float4(perlinVal * data.texcoord1.y, 0, 0, 0);
            data.vertex = newPos;
        }
        
        void surf (Input i, inout SurfaceOutputStandard o)
        {
            float3 ray = i.worldPos - _WorldSpaceCameraPos;
            float distToCam = length(ray);
            
            fixed4 c = tex2D (_MainTex, i.uv_MainTex) * _Color;
            float2 offsetX = i.worldPos.xz / _Scale + float2(_Time.y, 0);
            float4 col = perlinNoise(offsetX);
            
            
            
            o.Albedo = lerp(_BotColor, _TopColor, i.uv_MainTex.y);
            
            o.Metallic = _Metallic;
            o.Smoothness = lerp(0.1, _Glossiness, _SmoothnessState);
            
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse" 
}
