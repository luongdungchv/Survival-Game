Shader "FX/OutlineLight"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _OutlineColor("Outline Color", Color) = (1,1,1,1)
        _OutlineSize("Outline Size", float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Lambert

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        fixed4 _Color, _OutlineColor;
        float _OutlineSize;

        struct Input
        {
            float2 uv_MainTex;
        };
        
        UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutput o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            
            o.Alpha = c.a;
        }
        ENDCG

        Pass
        {
            Cull Front
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            uniform half4 _OutlineColor;
            uniform float _OutlineSize;

            struct vertexInput {
                float4 position: POSITION;
                float4 texcoord: TEXCOORD0;
            };

            struct vertexOutput {
                float4 position: SV_POSITION;
                float4 texcoord: TEXCOORD0;
            };

            float4 Outline(float4 vertPos, float width)
            {
                float4x4 scaleMat;
                scaleMat[0][0] = 1.0 + width;
                scaleMat[0][1] = 0.0;
                scaleMat[0][2] = 0.0;
                scaleMat[0][3] = 0.0;
                scaleMat[1][0] = 0.0;
                scaleMat[1][1] = 1.0 + width;
                scaleMat[1][2] = 0.0;
                scaleMat[1][3] = 0.0;
                scaleMat[2][0] = 0.0;
                scaleMat[2][1] = 0.0;
                scaleMat[2][2] = 1.0 + width;
                scaleMat[2][3] = 0.0;
                scaleMat[3][0] = 0.0;
                scaleMat[3][1] = 0.0;
                scaleMat[3][2] = 0.0;
                scaleMat[3][3] = 1.0;	

                return mul(scaleMat, vertPos);
            }

            vertexOutput vert(vertexInput v) 
            {
                vertexOutput o;
                o.position = UnityObjectToClipPos(Outline(v.position,_OutlineSize));
                return o;
            }

            half4 frag(vertexOutput i): COLOR 
            {
                return _OutlineColor;
            }

            ENDCG
        }        

        
    }
}
