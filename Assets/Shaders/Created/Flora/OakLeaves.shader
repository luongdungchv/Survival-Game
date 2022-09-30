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
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Lambert fullforwardshadows vertex:vert

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0
        
        #include "../../Headers/PerlinNoise.cginc"

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

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
        // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)
        
        void vert(inout appdata_full data){
            float3 worldPos = mul(unity_ObjectToWorld, float4(data.vertex.xyz, 1)).xyz;
            
            float2 offsetX = worldPos.xy / _Scale + float2(_Time.y * _Speed, 0);
            float perlinVal = perlinNoise(offsetX) - 0.5;
            float4 newPos = data.vertex + float4(0, perlinVal * data.texcoord1.y * _Strength, 0, 0);
            float displace = perlinVal * _Strength * data.texcoord1.y;
            float4 newPos2 = float4(worldPos, 1) + float4(displace, 0, displace, 0);
            //data.vertex = lerp(data.vertex, newPos, data.texcoord1.y);
            data.vertex = mul(unity_WorldToObject, newPos2);
            //data.vertex = mul(newPos2, unity_WorldToObject);
        }

        
        void surf (Input IN, inout SurfaceOutput o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            
            o.Alpha = c.a;
            clip(o.Alpha - 0.5);
        }
        ENDCG
    }
    FallBack "Diffuse"
}
