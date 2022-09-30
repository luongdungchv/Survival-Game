Shader "Unlit/TestPlanarMapping"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Tint", Color) = (0, 0, 0, 1) 
        
    }
    SubShader
    {
        Tags{"RenderType"="Opaque" "Queue"="Geometry"}
        LOD 100
        Pass{
            
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            fixed4 _Color;
            float4 _MainTex_ST;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            }; 
            struct v2f{
                float4 vertex: SV_POSITION;
                float2 uv: TEXCOORD0;
            };
            v2f vert(appdata i){
                v2f o;
                o.vertex = UnityObjectToClipPos(i.vertex);
                float4 worldPos =mul(unity_ObjectToWorld, i.vertex);
                o.uv = TRANSFORM_TEX(worldPos.xy, _MainTex);
                return o;
            }
            fixed4 frag(v2f i): SV_TARGET{
                fixed4 col = tex2D(_MainTex, i.uv);
                col *= _Color;
                return col;
            }

            ENDCG
        }
        
    }
}
