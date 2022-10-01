Shader "Environment/Rock/Cavity"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _LightStrength ("LightStrength", Float) = 0.3 
        _DarkStrength ("DarkStrength", Float) = 0.3 
        _Spread ("Spread", Float) = 1.0
        [Toggle(USE_MULTIPLY)] _UseMultiply("Use Multiply", Float) = 0
        [Toggle(CURVATURE_ONLY)] _CurvatureOnly("Curvature Only", Float) = 0
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma shader_feature USE_MULTIPLY
            #pragma shader_feature CURVATURE_ONLY

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            sampler2D _CameraDepthNormalsTexture;

            float _LightStrength;
            float _DarkStrength;
            float _Spread;
            float _Bias;


            //based on:
            //https://blender.community/c/rightclickselect/J9bbbc/
            //https://i.imgur.com/dXx94sV.png

            fixed4 frag (v2f i) : SV_Target
            {
                float3 normals;
                float depth;

                DecodeDepthNormal(tex2D(_CameraDepthNormalsTexture,i.uv),depth,normals);
                float3 center = normals;

                DecodeDepthNormal(tex2D(_CameraDepthNormalsTexture,i.uv + float2(0,_MainTex_TexelSize.y*_Spread)),depth,normals);
                float3 up = normals;

                DecodeDepthNormal(tex2D(_CameraDepthNormalsTexture,i.uv + float2(0,-_MainTex_TexelSize.y*_Spread)),depth,normals);
                float3 down = normals;

                DecodeDepthNormal(tex2D(_CameraDepthNormalsTexture,i.uv + float2(-_MainTex_TexelSize.x*_Spread,0)),depth,normals);
                float3 left = normals;

                DecodeDepthNormal(tex2D(_CameraDepthNormalsTexture,i.uv + float2(_MainTex_TexelSize.x*_Spread,0)),depth,normals);
                float3 right = normals;
                
                float prewitt = (right.r-left.r) + (up.g-down.g); //add 0.5 to view it since it goes negative 

                //return prewitt.xxxx;

                float obj = abs(right.b-left.b) + abs(up.b-down.b); //edge detect 

                obj = obj > 0.01; //make all lines strong 

                //return obj.xxxx;

                float mask = saturate(0.7*(1.0-(prewitt+1.0))); //only the outline edges of objects

                //return mask.xxxx;

                float curvature = lerp(prewitt+0.5,0.5,mask);

                //return curvature.xxxx;

                fixed4 col = tex2D(_MainTex, i.uv);

                curvature -= 0.5; //range now goes from -0.5 to 0.5
                
                curvature = ((curvature > 0) * _LightStrength*curvature) + ((curvature<0) * _DarkStrength*curvature);
                
                #ifdef CURVATURE_ONLY
                    curvature += 0.5;
                    return curvature.xxxx;
                #endif

                #ifdef USE_MULTIPLY
                    return col * (1.0+curvature.xxxx); //multiply
                #else
                    return col + curvature.xxxx; //add
                #endif
            }
            ENDCG
        }
    }
}