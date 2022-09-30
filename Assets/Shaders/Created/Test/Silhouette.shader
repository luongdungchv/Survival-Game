Shader "Test/Silhouette"
{
    Properties
    {
        _Color ("Silhouette Color", Color) = (0,0,0,1)
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        
        LOD 100
        
        //Coverd by other objects
        Pass
        {
            ZWrite Off
            ZTest Greater
            
            CGPROGRAM
            // Upgrade NOTE: excluded shader from DX11; has structs without semantics (struct v2f members worldPos)
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal: NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos: POSITION1;
                float3 normal: NORMAL;
            };

            float4 _Color;          

            v2f vert (appdata v)
            {
                v2f o;
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.normal = v.normal;
                return o;
            }

            fixed4 frag (v2f v) : SV_Target
            {
                float3 camPos = _WorldSpaceCameraPos.xyz;
                float3 dir = camPos - v.worldPos;
                float3 worldNormal = normalize(mul(v.normal, (float3x3)unity_WorldToObject));
                float intensity = dot(normalize(dir), worldNormal);
                intensity = saturate(intensity);
                float4 outline = (1 - pow(intensity, 1)) * _Color;
                outline.a = (1 - pow(intensity, 1));
                
                return (1 - pow(intensity, 1)) * _Color;
            }
            ENDCG
        }
        
        //Uncovered pass
        Pass
        {
            //ZTest Always
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
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
        
        
    }
}
