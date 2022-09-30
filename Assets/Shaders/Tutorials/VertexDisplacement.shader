Shader "Unlit/VertexDisplacement"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Amplitude("Amplitude", float) = 1
        _Frequency("Frequency", float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        GrabPass{"_SceneColor"}
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal: NORMAL;
                float3 tangent: TANGENT;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 normal: NORMAL;
                float3 tangent: TANGENT;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Amplitude;
            float _Frequency;

            v2f vert (appdata v)
            {
                v2f o;
                float3 point1 = v.vertex + v.tangent * 0.01;
                point1.y += _Amplitude * sin(point1.x * _Frequency + _Time.y);
                
                float3 bitangent = normalize(cross(v.tangent, v.normal));
                float3 point2 = v.vertex + bitangent * 0.01;
                point2.y += _Amplitude * sin(point2.x * _Frequency + _Time.y);   
                
                
                v.vertex.y += _Amplitude * sin(v.vertex.x * _Frequency + _Time.y);
                float3 normal = normalize(cross(v.vertex - point1,point2- v.vertex ));
                

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = normal;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 lightDir = ObjSpaceLightDir(i.vertex);
                float intensity = dot(i.normal, lightDir) / 2 + 0.5;
                fixed4 col = tex2D(_MainTex, i.uv) * intensity;
                return col;
            }
            ENDCG
        }
    }
}
