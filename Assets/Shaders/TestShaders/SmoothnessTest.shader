Shader "Test/SmoothnessTest"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NormalMap ("Normal", 2D) = "white" {}
        _SpecularExpo("Specular Exponent", float) = 1
        _FlowSpeed("Flow Speed", float) = 1
        
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
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
            #include "../Headers/PerlinNoise.cginc"

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
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _NormalMap;
            float _SpecularExpo;
            float _FlowSpeed;
            float _Scale;
            float4 _TopColor;
            float4 _BotColor;
            float _BlendFactor;
            float _SmoothnessState;
            

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = v.normal;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }
            
            float distanceToElippse(float coord){
                return sqrt(1 - coord * coord) / 2;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                
                float2 offsetUV = i.uv + float2(_Time.y * _FlowSpeed, _Time.y * _FlowSpeed);
                float2 offsetUV2 = i.uv - float2(_Time.y * _FlowSpeed, _Time.y * _FlowSpeed);
                
                float3 packedNormal = UnpackNormal(tex2D(_NormalMap, i.uv));
                float3 packedNormal1 = UnpackNormal(tex2D(_NormalMap, offsetUV2));
                
                //packedNormal1 = lerp(float3(0.5, 0.5, 1), )
                
                float3 lightDir = ObjSpaceLightDir(i.vertex);
                float lightIntensity = dot(lightDir, i.normal) / 2 + 0.5;
                
                float3 lookingDir = mul((float3x3)unity_CameraToWorld, float3(0,0,1));
                float3 viewDir = (i.worldPos - _WorldSpaceCameraPos);
                float3 viewDirProjXY = normalize(float3(viewDir.xy, 0));
                float3 viewDirProjYZ = normalize(float3(0, viewDir.yz));
                float threshold = lerp(0.95, 0.98, 1 - smoothstep(0.9, 1, dot(viewDirProjXY, float3(0, -1, 0))));
                
                
                float3 worldNormal = mul(i.normal, (float3x3)unity_WorldToObject);
                float3 viewReflect = normalize(reflect(viewDir, worldNormal));
                
                float specular = saturate(dot(_WorldSpaceLightPos0, viewReflect));
                //specular = round(saturate(pow(specular, _SpecularExpo)));
                float inverseSpec = 1 - specular;
                float4 specColor = lerp(0, _LightColor0, specular);
                specular = smoothstep(0.96, 1, specular);
                //specular = step(threshold, specular);
                
                float2 offsetX = i.worldPos.xz / _Scale + float2(_Time.y, 0);
                float4 col = perlinNoise(offsetX);
                return lerp(_BotColor, _TopColor, i.uv.y) + specular;
                
                //return pow(normalCol.y, 0.435) - col.y;
                //return specular;
            }
            ENDHLSL
        }
    }
}
