Shader "Environment/Water/WaterShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NormalMap("Normal Map", 2D) = ""{}
        _NormalMap1("Normal Map", 2D) = ""{}
        _SpecularExpo("Specular Exponent", float) = 1
        _Color ("Color", Color) = (0,0,0,0)
        _FlowSpeed("Flow Speed", float) = 1
        _SmoothnessState("Smoothness State", Range(0,1)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Transparent"}
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        
        GrabPass{
            "_BehindTex"
        }
        
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal: NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 normal: NORMAL;
                float3 worldPos: TEXCOORD1;
                float4 grabPos:TEXCOORD2;
            };

            sampler2D _MainTex;
            sampler2D _BehindTex;
            sampler2D _GrabTex;
            float4 _MainTex_ST;
            sampler2D _NormalMap;
            sampler2D _NormalMap1;
            float4 _NormalMap_ST;
            float _SpecularExpo;
            float4 _Color;
            float _FlowSpeed;
            float _SmoothnessState;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.grabPos = ComputeGrabScreenPos(o.vertex); 
                
                float2 worldPosFlat = TRANSFORM_TEX(v.uv, _NormalMap);
                
                float4 packedNormal = tex2Dlod(_NormalMap, float4(worldPosFlat,1, 1));
                //o.normal = v.normal;
                o.normal = v.normal;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                
                float2 offsetUV = i.uv + float2(_Time.y * _FlowSpeed, 0);
                float2 offsetUV2 = i.uv + float2(0, _Time.y * _FlowSpeed);
                
                float3 packedNormal = UnpackNormal(tex2D(_NormalMap, offsetUV));
                float3 packedNormal1 = UnpackNormal(tex2D(_NormalMap1, offsetUV2));
                float3 combinedNormal = (packedNormal1 + packedNormal) ;
                //combinedNormal = float3(combinedNormal.x, combinedNormal.z, combinedNormal.y);
                float4 distortMagnitude = float4(0, 0, 0, 0);
                
                float4 behindCol = tex2Dproj(_BehindTex, i.grabPos + distortMagnitude);
                
                //float4 behindCol = tex2Dproj(_BehindTex, ComputeGrabScreenPos(i.vertex));
                //packedNormal1 = lerp(float3(0.5, 0.5, 1), )
                
                float3 lightDir = ObjSpaceLightDir(i.vertex);
                float lightIntensity = dot(lightDir, i.normal) / 2 + 0.5;
                
                float3 viewDir = (i.worldPos - _WorldSpaceCameraPos);
                float3 worldNormal = (mul(combinedNormal, (float3x3)unity_WorldToObject));
                float3 viewReflect = normalize(reflect(viewDir, worldNormal));
                float3 lightPosReflect = -reflect(_WorldSpaceLightPos0, float3(0, 1, 0));
                
                float3 reflectSpecular = pow(saturate(dot(normalize(float3(1,0.3,1)), viewReflect)), 1);
                float specular = (saturate(dot(_WorldSpaceLightPos0, viewReflect)) );
                //float specular = (saturate(dot(light, viewReflect)))''
                //specular = (saturate(pow(specular, lerp(2, _SpecularExpo, _SmoothnessState)))) + 0.2;
                specular = (saturate(pow(specular, lerp(2, _SpecularExpo, _SmoothnessState))))  ;
                float4 specColor = lerp(0, _LightColor0, specular);
                float4 tintedColor = lerp(_Color * _LightColor0, specColor, specular);
                
                float2 screenUV = float2(i.grabPos.xy / i.grabPos.w);
                
                //return  behindCol * tintedColor;
                //return behindCol * _Color;
                //return float4(screenUV, 0, 1);
                //col = tex2Dproj(_BehindTex, i.grabPos + distortMagnitude);
                //return combinedNormal.z;
                //return float4(worldNormal, 1);
                return tintedColor;
            }
            ENDHLSL
        }
    }
    
}
