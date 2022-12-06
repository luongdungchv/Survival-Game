Shader "Environment/Flora/TreeLeaves1"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Scale ("Wind Density", float) = 1
        _Speed ("Wind Speed", float) = 1
        _Strength ("Wind Strength", float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="TransparentCutout" "Queue"="Transparent"}
        LOD 200
        Cull Off

        CGPROGRAM
        #pragma surface surf Lambert addshadow 

        #pragma target 3.0
        

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float4 vertex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        float _Scale;
        float _Speed;
        float _Strength;

        UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_INSTANCING_BUFFER_END(Props)
        
        
        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;  
            clip(c.a - 0.5);
        }
        ENDCG
    }
    FallBack "Diffuse"
}
