Shader "Test/Skybox Shader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        
        _CloudDensityMap("Cloud Density Map", 2D) = "white"{}
        _CloudDensity("Cloud Density", float) = 1
        _CloudOpacity("Cloud Opacity", float) = 1
        _CloudScatter("Cloud Scatter", float) = 1
        _CloudSpeed("Cloud Speed", float) = 1
        _CloudSpeed2("Reverse Cloud Speed", float) = 1
        _CloudTex("Cloud Texture", 2D) = "white"{}
        _CloudTexDensity("Cloud Texture Density", float) = 1 
        _CloudTexScatter("Cloud Texture Scatter", float) = 1
        
        _SunSize ("Sun Size", Range(0,1)) = 0.2
        _SkyColor ("Sky Color", Color) = (0,0,0,0)  
        _SkyColor1 ("Sky Color1", Color) = (0,0,0,0)
        _GroundColor ("Ground Color", Color) = (0,0,0,0)
        _BlendFactor ("Blend Factor", float) = 0
        _SunMoonState ("State", float) = 0.5 
        _StarScale("Star Scale", float) = 1
        _Power("Power", float) = 10
        _State("State", float) = 0
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
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "../Headers/WorleyNoise.cginc"
            //#include "../Headers/PerlinNoise.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal: NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 uv0: TEXCOORD3;
                float4 vertex : SV_POSITION;
                float3 worldPos: TEXCOORD1;
                float3 worldPosBase: TEXCOORD2;
                float3 normal: NORMAL;
            };

            sampler2D _MainTex;
            
            sampler2D _CloudDensityMap;
            float _CloudDensity;
            float _CloudOpacity;
            float _CloudScatter;
            float _CloudSpeed;
            sampler2D _CloudTex;
            float _CloudTexDensity;
            float _CloudTexScatter;
            float _CloudSpeed2;
            
            float4 _MainTex_ST;
            fixed4 _SkyColor;
            fixed4 _GroundColor;
            fixed4 _SkyColor1;
            float _BlendFactor;
            
            float _SunSize;
            float _State;
            float _SunMoonState;
            float _StarScale;
            float _Power;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPosBase = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldPos = normalize(o.worldPosBase);
                float3 worldPos = normalize(mul(unity_ObjectToWorld, v.vertex).xyz);               
                o.normal = abs(v.normal);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }
            
            float calcSunAtten(float3 lightPos, float3 worldPos){
                float angle = lerp(-0.5236, 0.5236, _State);
                
                float2 sunPos = float2(lightPos.y * cos(angle) - lightPos.z * sin(angle), lightPos.y * sin(angle) + lightPos.z * cos(angle));
                float3 delta = float3(lightPos.x, sunPos.x, sunPos.y) - worldPos;
                float dist = length(delta);
                float spot = 1 - smoothstep(0.0, _SunSize, dist);
                return spot * spot ;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float PI = 3.14159265;
                
                float part1 = atan2(i.worldPos.x, i.worldPos.z) / ( PI * 2 );
                float part2 = asin(i.worldPos.y) * 2 / PI;
                float2 uv0 = float2(part1, part2);
                
                //return tex2D(_MainTex, uv0);
                
                float2 cloudUV = uv0 * _CloudScatter + float2(_Time.y * _CloudSpeed, 0);
                float2 cloudUV1 = uv0 * _CloudScatter - float2(_Time.y * _CloudSpeed2, 0);
                float4 cloudDensity1 = tex2D(_CloudDensityMap, cloudUV) * 0.5;
                float4 cloudDensity2 = tex2D(_CloudDensityMap, cloudUV1) * 0.5;
                float4 cloudDensity = cloudDensity1 + cloudDensity2;
                cloudDensity = pow(cloudDensity, _CloudDensity) * _CloudOpacity;
                cloudDensity *= 1 - smoothstep(0.8,1, i.uv.y);
                cloudDensity = step(0.58, length(cloudDensity));
                
                float4 cloudCol = pow(tex2D(_CloudTex, uv0 * _CloudTexScatter), _CloudTexDensity);
                cloudCol = lerp(0.8, 1, cloudCol);
                cloudCol = lerp(0, cloudCol, cloudDensity);
                float t = smoothstep(-_BlendFactor + 0.33, _BlendFactor + 0.33, i.uv.y);
                cloudCol *= _SkyColor;
                //return cloudCol;
                
                float4 lightPos = -_WorldSpaceLightPos0;
                float4 col = lerp(_GroundColor, _SkyColor, 0.5);
                cloudCol *= (col + 0.5);
                col += float4(pow(_LightColor0.xyz, 0.1) * calcSunAtten(_WorldSpaceLightPos0.xyz, i.worldPos), 0);
                //col += cloudDensity;
                //col = tex2D(_MainTex, uv0);               
                col -= lerp(0, col, cloudDensity);
                //col += lerp(0.63, cloudCol, cloudCol) ;
                col += cloudCol;
                
                float worleyVal = saturate(worleyNoise(uv0 * _StarScale));
                float starrySky = pow(1 - worleyVal, _Power); 
                starrySky = lerp(0, starrySky, _SunMoonState);
                return col + starrySky;
                //return worleyVal;
                
            }
            ENDCG
        }
    }
}
