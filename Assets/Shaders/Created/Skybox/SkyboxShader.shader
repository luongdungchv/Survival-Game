﻿Shader "Environment/Skybox/Skybox Shader"
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
        _SunMoonState ("State", float) = 0.5 
        
        _SkyColor ("Sky Color", Color) = (0,0,0,0)  
        _SkyColor1 ("Sky Color1", Color) = (0,0,0,0)
        _GroundColor ("Ground Color", Color) = (0,0,0,0)
        _BlendFactor ("Blend Factor", float) = 0
        _StarScale("Star Scale", float) = 1
        _Power("Power", float) = 10
        _State("State", float) = 0
        _Value("_Value", float) = 0
        
        _StarTexture("Star Texture", 2D) = "white"{}
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "../../Headers/WorleyNoise.cginc"
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
                float3 normal: TEXCOORD4;
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
            float _State, _Value;
            float _SunMoonState;
            float _StarScale;
            float _Power;
            
            float PI = 3.141592654;
            
            sampler2D _StarTexture;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPosBase = mul(unity_ObjectToWorld, v.vertex).xyz;
                //o.worldPos = normalize(o.worldPosBase);
                o.worldPos = normalize(mul((float3x3)unity_ObjectToWorld, v.vertex.xyz));               
                o.normal = normalize(mul(v.normal, (float3x3)unity_WorldToObject));
                return o;
            }
            
            float4 calcSunAtten(float3 lightPos, float3 worldPos){
                float angle = lerp(-0.5236, 0.5236, _State);
                                
                float2 sunPos = float2(lightPos.y * cos(angle) - lightPos.z * sin(angle), lightPos.y * sin(angle) + lightPos.z * cos(angle));
                float3 delta = float3(lightPos.x, sunPos.x, sunPos.y) - worldPos;
                float dist = length(delta);
                float spot = 1 - smoothstep(0.0, _SunSize, dist);
                spot = step(0.0000001, spot);
                sunPos = normalize(sunPos);
                
                float3 newCoordX = float3(1,0,0);               
                float3 newCoordY = float3(0, -sunPos.y, sunPos.x);
                float3 newCoordZ = float3(0, sunPos.x, sunPos.y);
                
                float newPosX = dot(newCoordX, delta);
                float newPosY = dot(newCoordY, -delta);
                float newPosZ = dot(newCoordZ, -delta);
                
                float3 newPos = float3(newPosX, newPosY, newPosZ);           
                newPos.z = 0;              
                float4 col = tex2D(_MainTex, newPos.xy / _SunSize / 2 + float2(0.5, 0.5)) * spot;

                float3 lightCol = pow(_LightColor0.xyz, 0.1);
                if(_Value < 1) col = step(0.05, col.r) * float4(lightCol, 1);
                
                return col;
            }

            float4 triplanar(float3 positionWS, float3 normalWS){
                float3 weights = normalWS;
                weights = abs(weights);
                weights = weights / (weights.x + weights.y + weights.z);

                float2 uv_front = positionWS.xy;
                float2 uv_side = positionWS.zy;
                float2 uv_top = positionWS.zx;

                fixed4 col_front = tex2D(_StarTexture, uv_front / positionWS.z);
                fixed4 col_side = tex2D(_StarTexture, uv_side / positionWS.x);
                fixed4 col_top = tex2D(_StarTexture, uv_top / positionWS.y);

                float cos45 = 0.58;
                col_top *= step(cos45, abs(positionWS.y)) * (abs(uv_top.x) < 1 && abs(uv_top.y) < cos45);
                col_side *= step(cos45, abs(positionWS.x));
                col_front *= step(cos45, abs(positionWS.z)) * (abs(uv_front.x) < cos45 && abs(uv_front.y) < cos45);

                //return float4(positionWS, 1);
                //return positionWS.z;
                //return positionWS.y;
                //return acos(positionWS.y);
                //return col_top;

                // col_front *= weights.z;
				// col_side *= weights.x;
				// col_top *= weights.y;
                // return col_side;

                fixed4 col = col_front + col_side + col_top;
                return col;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float PI = 3.14159265;
                
                float part1 = atan2(i.worldPos.x, i.worldPos.z) / ( PI * 2 );
                float part2 = asin(i.worldPos.y) * 2 / PI;
                float2 uv0 = float2(part1, part2);
                float2 starUV = float2(atan2(i.worldPos.x, i.worldPos.z) / ( PI * 1.2 ), asin(i.worldPos.y) * 2 / PI);
                
                
                float2 cloudUV = uv0 * _CloudScatter + float2(_Time.y * _CloudSpeed, 0);
                float2 cloudUV1 = uv0 * _CloudScatter - float2(_Time.y * _CloudSpeed2, 0);
                float4 cloudDensity1 = tex2D(_CloudDensityMap, cloudUV) * 0.5;
                float4 cloudDensity2 = tex2D(_CloudDensityMap, cloudUV1) * 0.5;
                float4 cloudDensity = cloudDensity1 + cloudDensity2;
                cloudDensity = pow(cloudDensity, _CloudDensity) * _CloudOpacity;
                cloudDensity *= 1 - smoothstep(0.8,1, i.uv.y);
                cloudDensity = step(0.58, length(cloudDensity));
                
                float4 cloudCol = pow(tex2D(_CloudTex, uv0 * _CloudTexScatter), _CloudTexDensity);
                cloudCol = lerp(0.7, 1, cloudCol);
                cloudCol = lerp(0, cloudCol, cloudDensity);
                float t = smoothstep(-_BlendFactor + 0.33, _BlendFactor + 0.33, i.uv.y);
                
                float4 lightPos = -_WorldSpaceLightPos0;
                float4 col = lerp(_GroundColor, _SkyColor, t);
                cloudCol *= (col + 0.3);
                //return float4(pow(_LightColor0.xyz, 0.1) * calcSunAtten(_WorldSpaceLightPos0.xyz, i.worldPos), 0);
                
               
                //return calcSunAtten(_WorldSpaceLightPos0.xyz, i.worldPos);     
                
                ///float starCol = tex2D(_StarTexture, uv0 * _StarScale);
                //float worleyVal = saturate(worleyNoise(i.uv * _StarScale));
                //float worleyVal = saturate(tex2D(_StarTexture, uv0));
                //float starrySky = pow(1 - worleyVal, _Power); 
                
                
                                
                //float4 starrySky = tex2D(_StarTexture, starUV) * 2;
                //return starrySky * 2;
                //return starrySky;
                //return i.worldPos.x;
                float4 starrySky = triplanar(i.worldPos, i.worldPos);
                starrySky = pow(starrySky, 0.7);
                //return starrySky;
                starrySky = lerp(0, starrySky, _SunMoonState);


                col += starrySky;

                float4 sunMoon = 1 * calcSunAtten(_WorldSpaceLightPos0.xyz, i.worldPos);
                float sunMoonMask = step(0.05, sunMoon.r);
                col = (1 - sunMoonMask) * col;
                col += sunMoon; 
                
                
                col -= lerp(0, col, cloudDensity);
                col += cloudCol;
                
                
                //return saturate(worleyNoise(i.uv * _StarScale));
                //return saturate(tex2D(_StarTexture, uv0));
                //return worleyVal;
                return col;
                
            }
            ENDHLSL
        }
    }
}
