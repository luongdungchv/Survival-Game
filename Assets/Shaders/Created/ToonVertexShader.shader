Shader "Unlit/ToonVertexShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader{
        Tags {"RenderType"="Opaque"}
        LOD 100
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            struct appdata{
                float4 vertex: POSITION;
                float2 uv: TEXCOORD0;
                float3 normal: NORMAL;
            };
            struct v2f{
                float4 vertex: SV_POSITION;
                float2 uv: TEXCOORD;
                float3 normal: NORMAL;
            };
            v2f vert(appdata i){
                v2f o;
                o.vertex = UnityObjectToClipPos(i.vertex);
                o.uv = i.uv;
                //o.normal = normalize(mul(i.normal, (float3x3)unity_WorldToObject));
                o.normal = i.normal;
                return o;
            }
            fixed4 frag(v2f i): SV_TARGET{
                float3 lightDir = ObjSpaceLightDir(i.vertex);
                float intensity = round(dot(i.normal, lightDir) / 2 + 0.5);
                float light = lerp(0.5, 1, intensity);
                
                float lightMag = (_LightColor0.x + _LightColor0.y + _LightColor0.z) / 3;
                float lightMagAdd = lerp(0, 0.4, 1 - lightMag);

                fixed4 col = tex2D(_MainTex, i.uv) * light * (lightMag + lightMagAdd);
                return col;
                //return 0;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
