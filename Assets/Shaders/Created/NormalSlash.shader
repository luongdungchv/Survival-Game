Shader "Attack/NormalSlash"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Power ("Power", float) = 1
    }
    SubShader{
        Tags {"RenderType"="Opaque" "Queue"="Transparent"}
        Blend One OneMinusSrcAlpha
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
            float _Power;

            struct appdata{
                float4 vertex: POSITION;
                float2 uv: TEXCOORD0;
                float4 color: COLOR;
            };
            struct v2f{
                float4 vertex: SV_POSITION;
                float2 uv: TEXCOORD;
                float4 color: COLOR;

            };
            v2f vert(appdata i){
                v2f o;
                o.vertex = UnityObjectToClipPos(i.vertex);
                o.uv = i.uv;
                o.color = i.color;
                return o;
            }
            fixed4 frag(v2f i): SV_TARGET{
                float4 col = pow(tex2D(_MainTex, i.uv), _Power);
                
                return i.color * col * i.color.a;
                //return 0;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
