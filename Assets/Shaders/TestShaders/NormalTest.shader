Shader "Unlit/NormalTest"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NormalMap ("Normal", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

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
            float4 _MainTex_ST;
            sampler2D _NormalMap;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float4 col = tex2D(_MainTex, i.uv);
                float4 normalCol = tex2D(_NormalMap, i.uv);
                // apply fog
                float z = sqrt(1 - saturate(dot(col.xy, col.xy)));
                
                float3 normal;
                normal.xy = col.ag * 2.0 - 1.0;
                normal.z = sqrt(1.0 - saturate(dot(normal.xy, normal.xy)));
                
                //return   normalCol.z - normalCol.y;
                //return col.a;
                //return step(col.g, col.b);
                //return normalCol.y - col.y;
                normalCol.z = pow(normalCol.z, 0.5);
                normalCol.y = pow(normalCol.y, 0.5);
                //return pow(normalCol.y, 0.435) - col.y;
                return normalCol;
            }
            ENDHLSL
        }
    }
}
