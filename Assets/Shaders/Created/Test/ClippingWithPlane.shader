Shader "Lesson/ClippingWithPlane"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Plane("Clipping plane", Vector) = (0,1,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            // make fog work

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos: TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Plane;

            v2f vert (appdata v)
            {
                v2f o;               
                o.vertex = UnityObjectToClipPos(v.vertex);   
                //o.vertex = v.vertex; 
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;          
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            { 
                float3 worldPos = i.worldPos;
                float3 orig = (0,0,0);
                float dist = dot(worldPos - orig, _Plane.xyz);
                clip(dist);             
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}
