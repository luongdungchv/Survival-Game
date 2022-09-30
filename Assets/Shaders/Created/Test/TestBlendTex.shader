Shader "Test/TestBlend"
{
    Properties{
        _Color("Tint", Color) = (0, 0, 0, 1) 
        _SecondColor("Second Color", Color) = (0,0,0,1)
        _MainTex ("Texture", 2D) = "white" {}
        _SecondTex ("Second Texture", 2D) = "white" {}
        _Blend ("Blend", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Transparent"}

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _SecondTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            fixed4 _SecondColor;
            sampler2D _Blend;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                //o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                //o.uv = v.uv;
                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            

            fixed4 frag (v2f i) : SV_TARGET
            {
                // sample the texture
                fixed4 col1 = tex2D(_MainTex, i.uv);
                fixed4 col2 = tex2D(_SecondTex, i.uv);
                fixed4 blendCol = tex2D(_Blend, i.uv);
                //col *= _Color;
                //col.w = 1;
                
                return lerp(_Color, _SecondColor, blendCol);
            }
            ENDCG
        }
    }
}
