Shader "FX/SlashShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Exponent("Exponent", float) = 1
        _NoiseExponent("Noise Exponent", float) = 1
        _WorleyScale("Worley Noise Scale", float) = 1
        _Speed("Noise Speed", float) = 1
        [HDR]_Color1("Color 1", Color) = (1,1,1,1)
        [HDR]_Color2("Color 2", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Transparent"}
        LOD 100
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work

            #include "UnityCG.cginc"
            #include "../../Headers/WorleyNoise.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0;
            };

            struct v2f
            {
                float3 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Exponent;
            float _NoiseExponent;
            float _WorleyScale;
            float4 _Color1;
            float4 _Color2;
            float _Speed;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = float3(TRANSFORM_TEX(v.uv.xy, _MainTex), v.uv.z);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                
                fixed4 col = tex2D(_MainTex, i.uv.xy);
                float4 baseNoise =pow(worleyNoise(i.uv.xy * _WorleyScale, _Time.y * _Speed), i.uv.z);
                float4 noise = saturate(1 - baseNoise);
                float4 noise2 = saturate(baseNoise);
                
                noise *= _Color1;
                noise2 *= _Color2;
                //noise += noise2;
                
                noise *= pow(col, _Exponent);
                return noise2 * pow(col, _Exponent);
            }
            ENDCG
        }
    }
}
