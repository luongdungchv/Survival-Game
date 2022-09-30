Shader "Unlit/TestTriplanar"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (0,0,0,1)
    }
    SubShader
    {
        Tags {"RenderType"="Opaque" "Queue"="Geometry"}
        LOD 100

        Pass{
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "../Headers/WorleyNoise.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;

            struct appdata{
                float4 vertex: POSITION;
                float2 uv: TEXCOORD0;
                float3 normal: NORMAL;
            };
            struct v2f{
                float4 vertex: SV_POSITION;
                float3 worldPos: TEXCOORD;
                float3 normal: NORMAL;
            };
            v2f vert(appdata i){
                v2f o;
                o.vertex = UnityObjectToClipPos(i.vertex);
                o.worldPos = mul(unity_ObjectToWorld, i.vertex).xyz;
                o.normal = normalize(mul(i.normal, (float3x3)unity_WorldToObject));
                o.normal = abs(o.normal);
                return o;
            }
            fixed4 frag(v2f i): SV_TARGET{
                float2 face = TRANSFORM_TEX(i.worldPos.xy, _MainTex) * 40;
                float2 side = TRANSFORM_TEX(i.worldPos.yz, _MainTex) * 40;
                float2 top = TRANSFORM_TEX(i.worldPos.xz, _MainTex) * 40;

                float3 w = i.normal;
                w = w / (w.x + w.y + w.z);

                // fixed4 colFace = tex2D(_MainTex, face) * w.z;
                // fixed4 colSide = tex2D(_MainTex, side) * w.x;
                // fixed4 colTop = tex2D(_MainTex, top) * w.y;
                
                fixed4 colFace = worleyNoise(face) * w.z;
                fixed4 colSide = worleyNoise(side) * w.x;
                fixed4 colTop = worleyNoise(top) * w.y;

                fixed4 combinedColor = (colFace + colSide + colTop);
                return combinedColor;
                //return fixed4(i.normal.xyz, 1);
            }
            ENDCG
        }
    }
}
