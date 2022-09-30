Shader "Custom/CustomLigting"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Custom fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        float4 _Color;

        struct Input
        {
            float2 uv_MainTex;
        };
        float4 LightingCustom(SurfaceOutput s, float3 lightDir, float atten){
            //return floor(dot(s.Normal, lightDir) + 1);
            float intensity = floor(dot(s.Normal, lightDir) + 1);
            float4 c;
            c.rgb = s.Albedo * _LightColor0.rgb;
            c.a = s.Alpha;

            return float4( s.Albedo * intensity * _LightColor0.rgb, s.Alpha);
            //return 0;
        }

        void surf (Input i, inout SurfaceOutput o)
        {
            fixed4 col = tex2D(_MainTex, i.uv_MainTex);
            o.Albedo = col.rgb;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
