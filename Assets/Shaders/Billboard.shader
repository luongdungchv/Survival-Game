// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Billboard"
{
    Properties
    {
        _MainTex("Texture Image", 2D) = "white" {}
        _ScaleX("Scale X", Float) = 1.0
        _ScaleY("Scale Y", Float) = 1.0
    }
    SubShader
    {
        Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM

            #pragma vertex vert  
            #pragma fragment frag
            #include "UnityCG.cginc"

            // User-specified uniforms            
            uniform sampler2D _MainTex;
            float4 _MainTex_ST;
            uniform float _ScaleX;
            uniform float _ScaleY;

            struct vertexInput
            {
                float4 vertex : POSITION;
                float2 tex : TEXCOORD0;
                float3 normal: NORMAL;
            };
            struct vertexOutput
            {
                float4 pos : SV_POSITION;
                float2 tex : TEXCOORD0;
                float3 normal: NORMAL;
            };

            vertexOutput vert(vertexInput input)
            {
                vertexOutput output;

                // output.pos = mul(UNITY_MATRIX_P,
                // mul(UNITY_MATRIX_MV, float4(0.0, 0.0, 0.0, 1.0))
                // + float4(input.vertex.x, input.vertex.y, 0.0, 0.0)
                // * float4(_ScaleX, _ScaleY, 1.0, 1.0));
                
                float4 worldOrigin = mul(UNITY_MATRIX_M, float4(0,0,0,1));
                float4 viewOrigin = float4(UnityObjectToViewPos(float3(0,0,0)), 1);
                
                float4 worldPos = mul(UNITY_MATRIX_M, input.vertex);
                //float4 flippedWorldPos = float4(-1,1,-1,1) * (worldPos - worldOrigin) + worldOrigin;
                float4 flippedWorldPos = worldPos;
                float4 viewPos = flippedWorldPos - worldOrigin + viewOrigin;
                float4 clipPos = mul(UNITY_MATRIX_P, viewPos);
                
                output.pos = clipPos;
                
                output.tex = TRANSFORM_TEX(input.tex, _MainTex);
                output.normal = input.normal;

                return output;
            }

            float4 frag(vertexOutput i) : COLOR
            {
                float4 col = tex2D(_MainTex, float2(i.tex.xy));
                float3 lightDir = ObjSpaceLightDir(i.pos);
                float intensity = round(dot(i.normal, lightDir) / 2 + 0.5);
                return col;
            }

            ENDCG
        }
    }
}