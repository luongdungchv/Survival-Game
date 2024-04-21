// Upgrade NOTE: replaced 'defined FOG_COMBINED_WITH_WORLD_POS' with 'defined (FOG_COMBINED_WITH_WORLD_POS)'

Shader "Environment/Terrain/Terrain Shader"
{
  Properties
  {
    _MainTex ("Albedo (RGB)", 2D) = "white" {}
    _testTex ("Test Texture", 2D) = "white" {}
    _testScale("Test Texture Scale", float) = 1
    _BaseTextures("Textures Array", 2DArray) = "" {}
  }
  SubShader
  {
    Tags { "RenderType"="Opaque" }
    LOD 200

    
    // ------------------------------------------------------------
    // Surface shader code generated out of a HLSLPROGRAM block:
    

    // ---- forward rendering base pass:
    Pass {
      Name "FORWARD"
      Tags { "LightMode" = "UniversalForward" }

      HLSLPROGRAM
      // compile directives
      #pragma vertex vert_surf
      #pragma fragment frag_surf
      #pragma target 3.0
      #pragma require 2darray
      #pragma multi_compile_instancing
      #pragma multi_compile_fog
      #pragma multi_compile_fwdbase
      #include "HLSLSupport.cginc"
      #define UNITY_INSTANCED_LOD_FADE
      #define UNITY_INSTANCED_SH
      #define UNITY_INSTANCED_LIGHTMAPSTS
      #define UNITY_INSTANCED_RENDERER_BOUNDS
      #include "UnityShaderVariables.cginc"
      #include "UnityShaderUtilities.cginc"
      // -------- variant for: <when no other keywords are defined>
      
      #include "UnityCG.cginc"
      #include "Lighting.cginc"
      #include "AutoLight.cginc"

      #define INTERNAL_DATA
      #define WorldReflectionVector(data,normal) data.worldRefl
      #define WorldNormalVector(data,normal) normal

      #line 13 ""
      #ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
      #endif
      #include "UnityCG.cginc"
      
      sampler2D _MainTex;
      sampler2D _testTex;
      float _testScale;

      const static int maxColorCount = 8;
      const static float epsilon = 1E-4;
      int baseColorCount;
      float4 baseColors[maxColorCount];
      float baseHeights[maxColorCount];
      float baseBlends[maxColorCount];
      UNITY_DECLARE_TEX2DARRAY(_BaseTextures);
      
      float minHeight;
      float maxHeight;

      struct Input
      {
        float3 worldPos;
        float3 worldNormal;
      };

      

      float inverseLerp(float a, float b, float val){
        return saturate((val - a)/(b - a));
      }
      
      float3 triplanar(float3 worldPos, float scale, float3 blendAxes, int textureIndex) {
        float3 scaledWorldPos = worldPos / scale;
        float3 xProjection = UNITY_SAMPLE_TEX2DARRAY(_BaseTextures, float3(scaledWorldPos.y, scaledWorldPos.z, textureIndex)) * blendAxes.x;
        float3 yProjection = UNITY_SAMPLE_TEX2DARRAY(_BaseTextures, float3(scaledWorldPos.x, scaledWorldPos.z, textureIndex)) * blendAxes.y;
        float3 zProjection = UNITY_SAMPLE_TEX2DARRAY(_BaseTextures, float3(scaledWorldPos.x, scaledWorldPos.y, textureIndex)) * blendAxes.z;
        return xProjection + yProjection + zProjection;
      }
      void surf (Input i, inout SurfaceOutput o)
      {
        //o.Albedo = col;
        float percentHeight = smoothstep(minHeight, maxHeight, i.worldPos.y);
        
        float3 blendAxes = abs(i.worldNormal);
        blendAxes /= blendAxes.x + blendAxes.y + blendAxes.z;

        for(int j = baseColorCount - 1; j >= 0; j--){                
          float blendStrength = smoothstep(-baseBlends[j] / 2 - epsilon, baseBlends[j] / 2, percentHeight - baseHeights[j]);
          float3 texColor = triplanar(i.worldPos, _testScale, blendAxes, j);
          o.Albedo = o.Albedo * (1 - blendStrength) + texColor * blendStrength;   
        }
        //o.Albedo = UNITY_SAMPLE_TEX2DARRAY(_BaseTextures, float3(i.worldPos.x / 35, i.worldPos.z / 35, 0));
      }
      

      // vertex-to-fragment interpolation data
      // no lightmaps:
      #ifndef LIGHTMAP_ON
        // half-precision fragment shader registers:
        #ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
          #define FOG_COMBINED_WITH_WORLD_POS
          struct v2f_surf {
            UNITY_POSITION(pos);
            float3 worldNormal : TEXCOORD0;
            float4 worldPos : TEXCOORD1;
            #if UNITY_SHOULD_SAMPLE_SH
              half3 sh : TEXCOORD2; // SH
            #endif
            UNITY_LIGHTING_COORDS(3,4)
            #if SHADER_TARGET >= 30
              float4 lmap : TEXCOORD5;
            #endif
            UNITY_VERTEX_INPUT_INSTANCE_ID
            UNITY_VERTEX_OUTPUT_STEREO
          };
        #endif
        // high-precision fragment shader registers:
        #ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
          struct v2f_surf {
            UNITY_POSITION(pos);
            float3 worldNormal : TEXCOORD0;
            float3 worldPos : TEXCOORD1;
            #if UNITY_SHOULD_SAMPLE_SH
              half3 sh : TEXCOORD2; // SH
            #endif
            UNITY_FOG_COORDS(3)
            UNITY_SHADOW_COORDS(4)
            #if SHADER_TARGET >= 30
              float4 lmap : TEXCOORD5;
            #endif
            UNITY_VERTEX_INPUT_INSTANCE_ID
            UNITY_VERTEX_OUTPUT_STEREO
          };
        #endif
      #endif
      // with lightmaps:
      #ifdef LIGHTMAP_ON
        // half-precision fragment shader registers:
        #ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
          #define FOG_COMBINED_WITH_WORLD_POS
          struct v2f_surf {
            UNITY_POSITION(pos);
            float3 worldNormal : TEXCOORD0;
            float4 worldPos : TEXCOORD1;
            float4 lmap : TEXCOORD2;
            UNITY_LIGHTING_COORDS(3,4)
            UNITY_VERTEX_INPUT_INSTANCE_ID
            UNITY_VERTEX_OUTPUT_STEREO
          };
        #endif
        // high-precision fragment shader registers:
        #ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
          struct v2f_surf {
            UNITY_POSITION(pos);
            float3 worldNormal : TEXCOORD0;
            float3 worldPos : TEXCOORD1;
            float4 lmap : TEXCOORD2;
            UNITY_FOG_COORDS(3)
            UNITY_SHADOW_COORDS(4)
            #ifdef DIRLIGHTMAP_COMBINED
              float3 tSpace0 : TEXCOORD5;
              float3 tSpace1 : TEXCOORD6;
              float3 tSpace2 : TEXCOORD7;
            #endif
            UNITY_VERTEX_INPUT_INSTANCE_ID
            UNITY_VERTEX_OUTPUT_STEREO
          };
        #endif
      #endif

      // vertex shader
      v2f_surf vert_surf (appdata_full v) {
        UNITY_SETUP_INSTANCE_ID(v);
        v2f_surf o;
        UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
        UNITY_TRANSFER_INSTANCE_ID(v,o);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
        o.pos = UnityObjectToClipPos(v.vertex);
        float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
        float3 worldNormal = UnityObjectToWorldNormal(v.normal);
        #if defined(LIGHTMAP_ON) && defined(DIRLIGHTMAP_COMBINED)
          fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
          fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
          fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
        #endif
        #if defined(LIGHTMAP_ON) && defined(DIRLIGHTMAP_COMBINED) && !defined(UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS)
          o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
          o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
          o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
        #endif
        o.worldPos.xyz = worldPos;
        o.worldNormal = worldNormal;
        #ifdef DYNAMICLIGHTMAP_ON
          o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
        #endif
        #ifdef LIGHTMAP_ON
          o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
        #endif

        // SH/ambient and vertex lights
        #ifndef LIGHTMAP_ON
          #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
            o.sh = 0;
            // Approximated illumination from non-important point lights
            #ifdef VERTEXLIGHT_ON
              o.sh += Shade4PointLights (
              unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
              unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
              unity_4LightAtten0, worldPos, worldNormal);
            #endif
            o.sh = ShadeSHPerVertex (worldNormal, o.sh);
          #endif
        #endif // !LIGHTMAP_ON

        UNITY_TRANSFER_LIGHTING(o,v.texcoord1.xy); // pass shadow and, possibly, light cookie coordinates to pixel shader
        #ifdef FOG_COMBINED_WITH_TSPACE
          UNITY_TRANSFER_FOG_COMBINED_WITH_TSPACE(o,o.pos); // pass fog coordinates to pixel shader
        #elif defined (FOG_COMBINED_WITH_WORLD_POS)
          UNITY_TRANSFER_FOG_COMBINED_WITH_WORLD_POS(o,o.pos); // pass fog coordinates to pixel shader
        #else
          UNITY_TRANSFER_FOG(o,o.pos); // pass fog coordinates to pixel shader
        #endif
        return o;
      }

      // fragment shader
      fixed4 frag_surf (v2f_surf IN) : SV_Target {
        UNITY_SETUP_INSTANCE_ID(IN);
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
        // prepare and unpack data
        Input surfIN;
        #ifdef FOG_COMBINED_WITH_TSPACE
          UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
        #elif defined (FOG_COMBINED_WITH_WORLD_POS)
          UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
        #else
          UNITY_EXTRACT_FOG(IN);
        #endif
        UNITY_INITIALIZE_OUTPUT(Input,surfIN);
        surfIN.worldPos.x = 1.0;
        surfIN.worldNormal.x = 1.0;
        float3 worldPos = IN.worldPos.xyz;
        #ifndef USING_DIRECTIONAL_LIGHT
          fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
        #else
          fixed3 lightDir = _WorldSpaceLightPos0.xyz;
        #endif
        surfIN.worldNormal = IN.worldNormal;
        surfIN.worldPos = worldPos;
        #ifdef UNITY_COMPILER_HLSL
          SurfaceOutput o = (SurfaceOutput)0;
        #else
          SurfaceOutput o;
        #endif
        o.Albedo = 0.0;
        o.Emission = 0.0;
        o.Specular = 0.0;
        o.Alpha = 0.0;
        o.Gloss = 0.0;
        fixed3 normalWorldVertex = fixed3(0,0,1);
        o.Normal = IN.worldNormal;
        normalWorldVertex = IN.worldNormal;

        // call surface function
        surf (surfIN, o);

        // compute lighting & shadowing factor
        UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
        fixed4 c = 0;

        // Setup lighting environment
        UnityGI gi;
        UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
        gi.indirect.diffuse = 0;
        gi.indirect.specular = 0;
        gi.light.color = _LightColor0.rgb;
        gi.light.dir = lightDir;
        // Call GI (lightmaps/SH/reflections) lighting function
        UnityGIInput giInput;
        UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
        giInput.light = gi.light;
        giInput.worldPos = worldPos;
        giInput.atten = atten;
        #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
          giInput.lightmapUV = IN.lmap;
        #else
          giInput.lightmapUV = 0.0;
        #endif
        #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
          giInput.ambient = IN.sh;
        #else
          giInput.ambient.rgb = 0.0;
        #endif
        giInput.probeHDR[0] = unity_SpecCube0_HDR;
        giInput.probeHDR[1] = unity_SpecCube1_HDR;
        #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
          giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
        #endif
        #ifdef UNITY_SPECCUBE_BOX_PROJECTION
          giInput.boxMax[0] = unity_SpecCube0_BoxMax;
          giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
          giInput.boxMax[1] = unity_SpecCube1_BoxMax;
          giInput.boxMin[1] = unity_SpecCube1_BoxMin;
          giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
        #endif
        LightingLambert_GI(o, giInput, gi);

        // realtime lighting: call lighting function
        c += LightingLambert (o, gi);
        UNITY_APPLY_FOG(_unity_fogCoord, c); // apply fog
        UNITY_OPAQUE_ALPHA(c.a);
        return c;
      }
      ENDHLSL

    }


  }
  FallBack "Diffuse"
}
