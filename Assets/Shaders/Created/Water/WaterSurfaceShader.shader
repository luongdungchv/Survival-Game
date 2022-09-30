Shader "Environment/Water/WaterSurfaceShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "" {}
        _Glossiness ("Smoothness", Range(0,2)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _NormalMap("Normal Map", 2D) = ""{}
        _SecondNormalMap("Second Normal map", 2D) = ""{}
        _FlowSpeed("Flow Speed", float) = 1
        _Scale("Scale", float) = 1
        [Slider]_NormalStrength("Normal strength", Range(0, 3)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200

        Blend SrcAlpha OneMinusSrcAlpha
        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows alpha:blend

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _NormalMap;
        sampler2D _SecondNormalMap;
        float4 _NormalMap_ST;
        float _FlowSpeed;
        float _Scale;
        float _NormalStrength;

        struct Input
        {
            float3 worldPos;
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        // put more per-instance properties here

        float3 Unity_NormalStrength(float3 In, float Strength)
        {
            return (In.rg * Strength, lerp(1, In.b, saturate(Strength)));
        }
        float3 Self_Unpack(float4 packedNormal, float scale = 1.0)
        {
            packedNormal.a *= packedNormal.r;
            float3 normal;
            normal.xy = packedNormal.ag * 2.0 - 1.0;
            normal.z = sqrt(1.0 - saturate(dot(normal.xy, normal.xy)));

            // must scale after reconstruction of normal.z which also
            // mirrors UnpackNormalRGB(). This does imply normal is not returned
            // as a unit length vector but doesn't need it since it will get normalized after TBN transformation.
            // If we ever need to blend contributions with built-in shaders for URP
            // then we should consider using UnpackDerivativeNormalAG() instead like
            // HDRP does since derivatives do not use renormalization and unlike tangent space
            // normals allow you to blend, accumulate and scale contributions correctly.
            normal.xy *= scale;
            return normal;
        }
        
        void surf (Input i, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, i.uv_MainTex) * _Color;
            
            // float2 offsetUV = i.worldPos.xz / _Scale + float2(0, _Time.y * _FlowSpeed);
            // float2 offsetUV2 = float2(-i.worldPos.x, i.worldPos.z) / _Scale - float2(0, _Time.y * _FlowSpeed);
            
            float2 offsetUV = i.uv_MainTex + float2(_Time.y * _FlowSpeed, _Time.y * _FlowSpeed);
            float2 offsetUV2 = i.uv_MainTex - float2(_Time.y * _FlowSpeed, _Time.y * _FlowSpeed);
            
            float4 normalVal1 = tex2D(_NormalMap, i.uv_MainTex);
            float4 normalVal2 = tex2D(_NormalMap, offsetUV2);
            // normalVal1.z = pow(normalVal1.z, 0.5);
            // normalVal1.y = pow(normalVal1.y, 0.5);
            // normalVal2.z = pow(normalVal2.z, 0.5);
            // normalVal2.y = pow(normalVal2.y, 0.5);
            
            float4 packedNormal1 = lerp(float4(0.5, 0.5, 1, 1), normalVal1, _NormalStrength);
            float4 packedNormal2 = lerp(float4(0.5, 0.5, 1, 1), normalVal2, _NormalStrength);
            //float4 packedNormal1 = float4(Unity_NormalStrength(tex2D(_NormalMap, offsetUV).rgb, _NormalStrength), 1);
            //float4 packedNormal2 = float4(Unity_NormalStrength(tex2D(_NormalMap, offsetUV2).rgb, _NormalStrength), 1);
            
            
            o.Normal = UnpackNormal(packedNormal1) + UnpackNormal(packedNormal2);
            //o.Normal = UnpackNormal(packedNormal2) + UnpackNormal(packedNormal1);
            float3 normal = UnpackNormal(packedNormal1);
            //normal.y = pow(normal.y, _Glossiness);
            //o.Normal = normal;
            o.Albedo = _Color;
            //o.Albedo =normalVal1;
            //o.Albedo = _Color;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = 1;
            o.Alpha = _Color.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
