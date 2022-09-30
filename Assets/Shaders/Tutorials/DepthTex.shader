Shader "Unlit/DepthTex"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [Header(Wave)]
        _WaveDistance ("Distance from player", float) = 10
        _WaveTrail ("Length of the trail", Range(0,5)) = 1
        _WaveColor ("Color", Color) = (1,0,0,1)
        _WaveHeight("Wave Height", float) = 1
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
            #pragma target 3.0
            // make fog work

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
            sampler2D _CameraDepthTexture;
            float4 _WaveColor;
            float _WaveDistance;
            float _WaveTrail;
            float _WaveHeight;
            
            float GetWave(float depth){
                float waveFront = 1 - smoothstep(_WaveDistance, _WaveDistance + _WaveTrail, depth);
                float waveTrail = smoothstep(_WaveDistance - _WaveTrail, _WaveDistance, depth);
                float wave = waveFront * waveTrail;
                return wave;
            }
            float GetDepth(float2 uv){
                float depth = tex2D(_CameraDepthTexture, uv).r;
                //linear depth between camera and far clipping plane
                depth = Linear01Depth(depth);
                //depth as distance from camera in units
                depth = depth * _ProjectionParams.z;
                return depth;
            }

            v2f vert (appdata v)
            {
                v2f o;
                
                
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float depth = GetDepth(i.uv);
                //get source color
                fixed4 source = tex2D(_MainTex, i.uv);
                //skip wave and return source color if we're at the skybox
                if(depth >= _ProjectionParams.z)
                return source;

                //calculate wave
                float wave = GetWave(depth);
                i.vertex.y += wave * _WaveHeight;
                //mix wave into source color
                fixed4 col = lerp(source, _WaveColor, wave);

                return col;
            }
            ENDCG
        }
    }
}
