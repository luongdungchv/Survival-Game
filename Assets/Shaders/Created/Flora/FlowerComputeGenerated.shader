// Upgrade NOTE: replaced 'defined FOG_COMBINED_WITH_WORLD_POS' with 'defined (FOG_COMBINED_WITH_WORLD_POS)'

Shader "Environment/Flora/Flower Compute Gen"
{
  Properties
  {
    _Color ("Color", Color) = (1,1,1,1)
    _MainTex ("Albedo (RGB)", 2D) = "white" {}
    _MainTex1 ("Albedo (RGB) 1", 2D) = "white" {}
    _MainTex2 ("Albedo (RGB) 2", 2D) = "white" {}

    _Glossiness ("Smoothness", Range(0,1)) = 0.5
    _Metallic ("Metallic", Range(0,1)) = 0.0
    _Scale ("Noise Scale", float) = 1
    _TopColor ("Top Color", Color) = (1,1,1,1)
    _BotColor ("Bot Color", Color) = (1,1,1,1)
    _BlendFactor("Blend Factor", float) = 0.5
    _SmoothnessState("Smoothness State", float) = 0
    _WindSpeed("Wind Speed", float) = 1
    _WindStrength("Wind Strength", float) = 1
  }
  SubShader
  {
    Tags { "RenderType"="Opaque" }
    LOD 200
    Cull Off

    
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
      #pragma target 5.0 
      #pragma multi_compile_instancing
      #pragma multi_compile_fog
      #pragma multi_compile_fwdbase nodynlightmap nolightmap
      #include "HLSLSupport.cginc"
      #define UNITY_INSTANCED_LOD_FADE
      #define UNITY_INSTANCED_SH
      #define UNITY_INSTANCED_LIGHTMAPSTS
      #define UNITY_INSTANCED_RENDERER_BOUNDS
      #include "UnityShaderVariables.cginc"
      #include "UnityShaderUtilities.cginc"
      // -------- variant for: <when no other keywords are defined>
      #if !defined(INSTANCING_ON)
        // Surface shader code generated based on:
        // vertex modifier: 'vert'
        // writes to per-pixel normal: no
        // writes to emission: no
        // writes to occlusion: no
        // needs world space reflection vector: no
        // needs world space normal vector: no
        // needs screen space position: no
        // needs world space position: no
        // needs view direction: no
        // needs world space view direction: no
        // needs world space position for lighting: YES
        // needs world space view direction for lighting: no
        // needs world space view direction for lightmaps: no
        // needs vertex color: no
        // needs VFACE: no
        // needs SV_IsFrontFace: no
        // passes tangent-to-world matrix to pixel shader: no
        // reads from normal: no
        // 1 texcoords actually used
        //   float2 _MainTex
        #include "UnityCG.cginc"
        //Shader does not support lightmap thus we always want to fallback to SH.
        #undef UNITY_SHOULD_SAMPLE_SH
        #define UNITY_SHOULD_SAMPLE_SH (!defined(UNITY_PASS_FORWARDADD) && !defined(UNITY_PASS_SHADOWCASTER) && !defined(UNITY_PASS_META))
        #include "Lighting.cginc"
        #include "AutoLight.cginc"

        #define INTERNAL_DATA
        #define WorldReflectionVector(data,normal) data.worldRefl
        #define WorldNormalVector(data,normal) normal

        // Original surface shader snippet:
        #line 24 ""
        #ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
        #endif
        /* UNITY: Original start of shader */
        // Physically based Standard lighting model, and enable shadows on all light types
        //#pragma surface surf Lambert vertex:vert

        // Use shader model 3.0 target, to get nicer looking lighting
        //#pragma target 5.0 
        
        #include "../../Headers/PerlinNoise.cginc"
        #include "UnityCG.cginc"

        sampler2D _MainTex;
        sampler2D _MainTex1;
        sampler2D _MainTex2;

        struct Input
        {
          float2 uv_MainTex;
          float3 worldPos;
          float2 uv2;
          float3 localPos;
          int texIndex;
        };
        struct appdata{
          float4 vertex: POSITION;
          float2 texcoord: TEXCOORD0;
          float3 normal: NORMAL;
          float4 tangent: TANGENT;
          UNITY_VERTEX_INPUT_INSTANCE_ID
          uint id : SV_VertexID;
          uint inst : SV_InstanceID;
        };
        struct InstanceData{
          float3 pos;
          float4x4 trs;
          int texIndex;  
          int chunkIndex;
        };

        half _Glossiness;
        half _Metallic;
        float4 _Color;
        float _Scale;
        float4 _TopColor;
        float4 _BotColor;
        float _BlendFactor;
        float _SmoothnessState;
        float _WindSpeed;
        float _WindStrength;
        
          StructuredBuffer<InstanceData> instDatas; 
        
        void vert(inout appdata data, out Input o){
          UNITY_INITIALIZE_OUTPUT(Input, o);
          UNITY_SETUP_INSTANCE_ID(data)
            float3 worldPos = mul(instDatas[data.inst].trs, data.vertex).xyz;
            float3 worldOrigin = instDatas[data.inst].pos;   
            
            float2 offsetX = worldOrigin.xy / 5 + float2(_Time.y * _WindSpeed, 0);
            float2 offSetY = worldOrigin.xy / 5 + float2(0, _Time.y * _WindSpeed );
            float perlinVal = perlinNoise(offsetX) - 0.5;
            float perlinVal2 = perlinNoise(offSetY) - 0.5;
            float4 newPos = float4(worldPos, 0) + float4(perlinVal * _WindStrength , 0, perlinVal2 * _WindStrength, 0);
            data.vertex = newPos;
            data.normal = float3(0,1,0);
            o.texIndex = instDatas[data.inst].texIndex;
        }
        
        void surf (Input i, inout SurfaceOutput o)
        {    
          float texId = i.texIndex;
          float4 col ;
          
          // if(texId == 0) col = tex2D(_MainTex, i.uv_MainTex);
          // else if(texId == 1) col = tex2D(_MainTex1, i.uv_MainTex);
          // else if (texId == 2) col = tex2D(_MainTex2, i.uv_MainTex);
          col = tex2D(_MainTex, i.uv_MainTex);
          o.Albedo = col;
          //o.Emission = 0.2;
          // o.Smoothness = 0;
          // o.Metallic = 0;
          
        }
        

        // vertex-to-fragment interpolation data
        // no lightmaps:
        #ifndef LIGHTMAP_ON
          // half-precision fragment shader registers:
          #ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
            #define FOG_COMBINED_WITH_WORLD_POS
            struct v2f_surf {
              UNITY_POSITION(pos);
              float2 pack0 : TEXCOORD0; // _MainTex
              float3 worldNormal : TEXCOORD1;
              float4 worldPos : TEXCOORD2;
              float3 custompack0 : TEXCOORD3; // localPos
              float2 custompack1 : TEXCOORD4; // texIndex
              #if UNITY_SHOULD_SAMPLE_SH
                half3 sh : TEXCOORD5; // SH
              #endif
              UNITY_LIGHTING_COORDS(6,7)
              UNITY_VERTEX_INPUT_INSTANCE_ID
              UNITY_VERTEX_OUTPUT_STEREO
            };
          #endif
          // high-precision fragment shader registers:
          #ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
            struct v2f_surf {
              UNITY_POSITION(pos);
              float2 pack0 : TEXCOORD0; // _MainTex
              float3 worldNormal : TEXCOORD1;
              float3 worldPos : TEXCOORD2;
              float3 custompack0 : TEXCOORD3; // localPos
              float2 custompack1 : TEXCOORD4; // texIndex
              #if UNITY_SHOULD_SAMPLE_SH
                half3 sh : TEXCOORD5; // SH
              #endif
              UNITY_FOG_COORDS(6)
              UNITY_SHADOW_COORDS(7)
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
              float2 pack0 : TEXCOORD0; // _MainTex
              float3 worldNormal : TEXCOORD1;
              float4 worldPos : TEXCOORD2;
              float3 custompack0 : TEXCOORD3; // localPos
              float2 custompack1 : TEXCOORD4; // texIndex
              float4 lmap : TEXCOORD5;
              UNITY_LIGHTING_COORDS(6,7)
              UNITY_VERTEX_INPUT_INSTANCE_ID
              UNITY_VERTEX_OUTPUT_STEREO
            };
          #endif
          // high-precision fragment shader registers:
          #ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
            struct v2f_surf {
              UNITY_POSITION(pos);
              float2 pack0 : TEXCOORD0; // _MainTex
              float3 worldNormal : TEXCOORD1;
              float3 worldPos : TEXCOORD2;
              float3 custompack0 : TEXCOORD3; // localPos
              float2 custompack1 : TEXCOORD4; // texIndex
              float4 lmap : TEXCOORD5;
              UNITY_FOG_COORDS(6)
              UNITY_SHADOW_COORDS(7)
              UNITY_VERTEX_INPUT_INSTANCE_ID
              UNITY_VERTEX_OUTPUT_STEREO
            };
          #endif
        #endif
        float4 _MainTex_ST;

        // vertex shader
        v2f_surf vert_surf (appdata v) {
          UNITY_SETUP_INSTANCE_ID(v);
          v2f_surf o;
          UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
          UNITY_TRANSFER_INSTANCE_ID(v,o);
          UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
          Input customInputData;
          vert (v, customInputData);
          o.custompack0.xyz = customInputData.localPos;
          o.custompack1.xy = customInputData.texIndex;
          o.pos = UnityObjectToClipPos(v.vertex);
          o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
          float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
          float3 worldNormal = UnityObjectToWorldNormal(v.normal);
          #if defined(LIGHTMAP_ON) && defined(DIRLIGHTMAP_COMBINED)
            float3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
            float tangentSign = v.tangent.w * unity_WorldTransformParams.w;
            float3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
          #endif
          #if defined(LIGHTMAP_ON) && defined(DIRLIGHTMAP_COMBINED) && !defined(UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS)
            o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
            o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
            o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
          #endif
          o.worldPos.xyz = worldPos;
          o.worldNormal = worldNormal;
          #ifdef LIGHTMAP_ON
            o.lmap.xy = half2(0.0, 0.0) * unity_LightmapST.xy + unity_LightmapST.zw;
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

          UNITY_TRANSFER_LIGHTING(o,half2(0.0, 0.0)); // pass shadow and, possibly, light cookie coordinates to pixel shader
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
        float4 frag_surf (v2f_surf IN) : SV_Target {
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
          surfIN.uv_MainTex.x = 1.0;
          surfIN.worldPos.x = 1.0;
          surfIN.uv2.x = 1.0;
          surfIN.localPos.x = 1.0;
          surfIN.uv_MainTex = IN.pack0.xy;
          surfIN.localPos = IN.custompack0.xyz;
          surfIN.texIndex = IN.custompack1.xy;
          float3 worldPos = IN.worldPos.xyz;
          #ifndef USING_DIRECTIONAL_LIGHT
            float3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
          #else
            float3 lightDir = _WorldSpaceLightPos0.xyz;
          #endif
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
          float3 normalWorldVertex = float3(0,0,1);
          o.Normal = IN.worldNormal;
          normalWorldVertex = IN.worldNormal;

          // call surface function
          surf (surfIN, o);

          // compute lighting & shadowing factor
          UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
          float4 c = 0;

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


      #endif

      // -------- variant for: INSTANCING_ON 
      #if defined(INSTANCING_ON)
        // Surface shader code generated based on:
        // vertex modifier: 'vert'
        // writes to per-pixel normal: no
        // writes to emission: no
        // writes to occlusion: no
        // needs world space reflection vector: no
        // needs world space normal vector: no
        // needs screen space position: no
        // needs world space position: no
        // needs view direction: no
        // needs world space view direction: no
        // needs world space position for lighting: YES
        // needs world space view direction for lighting: no
        // needs world space view direction for lightmaps: no
        // needs vertex color: no
        // needs VFACE: no
        // needs SV_IsFrontFace: no
        // passes tangent-to-world matrix to pixel shader: no
        // reads from normal: no
        // 1 texcoords actually used
        //   float2 _MainTex
        #include "UnityCG.cginc"
        //Shader does not support lightmap thus we always want to fallback to SH.
        #undef UNITY_SHOULD_SAMPLE_SH
        #define UNITY_SHOULD_SAMPLE_SH (!defined(UNITY_PASS_FORWARDADD) && !defined(UNITY_PASS_SHADOWCASTER) && !defined(UNITY_PASS_META))
        #include "Lighting.cginc"
        #include "AutoLight.cginc"

        #define INTERNAL_DATA
        #define WorldReflectionVector(data,normal) data.worldRefl
        #define WorldNormalVector(data,normal) normal

        // Original surface shader snippet:
        #line 24 ""
        #ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
        #endif
        /* UNITY: Original start of shader */
        // Physically based Standard lighting model, and enable shadows on all light types
        //#pragma surface surf Lambert vertex:vert

        // Use shader model 3.0 target, to get nicer looking lighting
        //#pragma target 5.0 
        
        #include "../../Headers/PerlinNoise.cginc"
        #include "UnityCG.cginc"

        sampler2D _MainTex;
        sampler2D _MainTex1;
        sampler2D _MainTex2;

        struct Input
        {
          float2 uv_MainTex;
          float3 worldPos;
          float2 uv2;
          float3 localPos;
          int texIndex;
        };
        struct appdata{
          float4 vertex: POSITION;
          float2 texcoord: TEXCOORD0;
          float3 normal: NORMAL;
          float4 tangent: TANGENT;
          UNITY_VERTEX_INPUT_INSTANCE_ID
          uint id : SV_VertexID;
          uint inst : SV_InstanceID;
        };
        struct InstanceData{
          float3 pos;
          float4x4 trs;
          int texIndex;  
          int chunkIndex;
        };

        half _Glossiness;
        half _Metallic;
        float4 _Color;
        float _Scale;
        float4 _TopColor;
        float4 _BotColor;
        float _BlendFactor;
        float _SmoothnessState;
        float _WindSpeed;
        float _WindStrength;
        
          StructuredBuffer<InstanceData> instDatas; 
        
        
        void vert(inout appdata data, out Input o){
          UNITY_INITIALIZE_OUTPUT(Input, o);
          UNITY_SETUP_INSTANCE_ID(data)
            float3 worldPos = mul(instDatas[data.inst].trs, data.vertex).xyz;
            float3 worldOrigin = instDatas[data.inst].pos;   
            
            float2 offsetX = worldOrigin.xy / 5 + float2(_Time.y * _WindSpeed, 0);
            float2 offSetY = worldOrigin.xy / 5 + float2(0, _Time.y * _WindSpeed );
            float perlinVal = perlinNoise(offsetX) - 0.5;
            float perlinVal2 = perlinNoise(offSetY) - 0.5;
            float4 newPos = float4(worldPos, 0) + float4(perlinVal * _WindStrength , 0, perlinVal2 * _WindStrength, 0);
            data.vertex = newPos;
            data.normal = float3(0,1,0);
            o.texIndex = instDatas[data.inst].texIndex;
        }
        
        void surf (Input i, inout SurfaceOutput o)
        {    
          float texId = i.texIndex;
          float4 col ;
          
          // if(texId == 0) col = tex2D(_MainTex, i.uv_MainTex);
          // else if(texId == 1) col = tex2D(_MainTex1, i.uv_MainTex);
          // else if (texId == 2) col = tex2D(_MainTex2, i.uv_MainTex);
          col = tex2D(_MainTex, i.uv_MainTex);
          o.Albedo = col;
          //o.Emission = 0.2;
          // o.Smoothness = 0;
          // o.Metallic = 0;
          
        }
        

        // vertex-to-fragment interpolation data
        // no lightmaps:
        #ifndef LIGHTMAP_ON
          // half-precision fragment shader registers:
          #ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
            #define FOG_COMBINED_WITH_WORLD_POS
            struct v2f_surf {
              UNITY_POSITION(pos);
              float2 pack0 : TEXCOORD0; // _MainTex
              float3 worldNormal : TEXCOORD1;
              float4 worldPos : TEXCOORD2;
              float3 custompack0 : TEXCOORD3; // localPos
              float2 custompack1 : TEXCOORD4; // texIndex
              #if UNITY_SHOULD_SAMPLE_SH
                half3 sh : TEXCOORD5; // SH
              #endif
              UNITY_LIGHTING_COORDS(6,7)
              UNITY_VERTEX_INPUT_INSTANCE_ID
              UNITY_VERTEX_OUTPUT_STEREO
            };
          #endif
          // high-precision fragment shader registers:
          #ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
            struct v2f_surf {
              UNITY_POSITION(pos);
              float2 pack0 : TEXCOORD0; // _MainTex
              float3 worldNormal : TEXCOORD1;
              float3 worldPos : TEXCOORD2;
              float3 custompack0 : TEXCOORD3; // localPos
              float2 custompack1 : TEXCOORD4; // texIndex
              #if UNITY_SHOULD_SAMPLE_SH
                half3 sh : TEXCOORD5; // SH
              #endif
              UNITY_FOG_COORDS(6)
              UNITY_SHADOW_COORDS(7)
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
              float2 pack0 : TEXCOORD0; // _MainTex
              float3 worldNormal : TEXCOORD1;
              float4 worldPos : TEXCOORD2;
              float3 custompack0 : TEXCOORD3; // localPos
              float2 custompack1 : TEXCOORD4; // texIndex
              float4 lmap : TEXCOORD5;
              UNITY_LIGHTING_COORDS(6,7)
              UNITY_VERTEX_INPUT_INSTANCE_ID
              UNITY_VERTEX_OUTPUT_STEREO
            };
          #endif
          // high-precision fragment shader registers:
          #ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
            struct v2f_surf {
              UNITY_POSITION(pos);
              float2 pack0 : TEXCOORD0; // _MainTex
              float3 worldNormal : TEXCOORD1;
              float3 worldPos : TEXCOORD2;
              float3 custompack0 : TEXCOORD3; // localPos
              float2 custompack1 : TEXCOORD4; // texIndex
              float4 lmap : TEXCOORD5;
              UNITY_FOG_COORDS(6)
              UNITY_SHADOW_COORDS(7)
              UNITY_VERTEX_INPUT_INSTANCE_ID
              UNITY_VERTEX_OUTPUT_STEREO
            };
          #endif
        #endif
        float4 _MainTex_ST;

        // vertex shader
        v2f_surf vert_surf (appdata v) {
          UNITY_SETUP_INSTANCE_ID(v);
          v2f_surf o;
          UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
          UNITY_TRANSFER_INSTANCE_ID(v,o);
          UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
          Input customInputData;
          vert (v, customInputData);
          o.custompack0.xyz = customInputData.localPos;
          o.custompack1.xy = customInputData.texIndex;
          o.pos = UnityObjectToClipPos(v.vertex);
          o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
          float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
          float3 worldNormal = UnityObjectToWorldNormal(v.normal);
          #if defined(LIGHTMAP_ON) && defined(DIRLIGHTMAP_COMBINED)
            float3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
            float tangentSign = v.tangent.w * unity_WorldTransformParams.w;
            float3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
          #endif
          #if defined(LIGHTMAP_ON) && defined(DIRLIGHTMAP_COMBINED) && !defined(UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS)
            o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
            o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
            o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
          #endif
          o.worldPos.xyz = worldPos;
          o.worldNormal = worldNormal;
          #ifdef LIGHTMAP_ON
            o.lmap.xy = half2(0.0, 0.0) * unity_LightmapST.xy + unity_LightmapST.zw;
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

          UNITY_TRANSFER_LIGHTING(o,half2(0.0, 0.0)); // pass shadow and, possibly, light cookie coordinates to pixel shader
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
        float4 frag_surf (v2f_surf IN) : SV_Target {
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
          surfIN.uv_MainTex.x = 1.0;
          surfIN.worldPos.x = 1.0;
          surfIN.uv2.x = 1.0;
          surfIN.localPos.x = 1.0;
          surfIN.uv_MainTex = IN.pack0.xy;
          surfIN.localPos = IN.custompack0.xyz;
          surfIN.texIndex = IN.custompack1.xy;
          float3 worldPos = IN.worldPos.xyz;
          #ifndef USING_DIRECTIONAL_LIGHT
            float3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
          #else
            float3 lightDir = _WorldSpaceLightPos0.xyz;
          #endif
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
          float3 normalWorldVertex = float3(0,0,1);
          o.Normal = IN.worldNormal;
          normalWorldVertex = IN.worldNormal;

          // call surface function
          surf (surfIN, o);

          // compute lighting & shadowing factor
          UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
          float4 c = 0;

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


      #endif


      ENDHLSL

    }

    // ---- forward rendering additive lights pass:
    Pass {
      Name "FORWARD"
      Tags { "LightMode" = "ForwardAdd" }
      ZWrite Off Blend One One

      HLSLPROGRAM
      // compile directives
      #pragma vertex vert_surf
      #pragma fragment frag_surf
      #pragma target 5.0 
      #pragma multi_compile_instancing
      #pragma multi_compile_fog
      #pragma skip_variants INSTANCING_ON
      #pragma multi_compile_fwdadd nodynlightmap nolightmap
      #include "HLSLSupport.cginc"
      #define UNITY_INSTANCED_LOD_FADE
      #define UNITY_INSTANCED_SH
      #define UNITY_INSTANCED_LIGHTMAPSTS
      #define UNITY_INSTANCED_RENDERER_BOUNDS
      #include "UnityShaderVariables.cginc"
      #include "UnityShaderUtilities.cginc"
      // -------- variant for: <when no other keywords are defined>
      #if !defined(INSTANCING_ON)
        // Surface shader code generated based on:
        // vertex modifier: 'vert'
        // writes to per-pixel normal: no
        // writes to emission: no
        // writes to occlusion: no
        // needs world space reflection vector: no
        // needs world space normal vector: no
        // needs screen space position: no
        // needs world space position: no
        // needs view direction: no
        // needs world space view direction: no
        // needs world space position for lighting: YES
        // needs world space view direction for lighting: no
        // needs world space view direction for lightmaps: no
        // needs vertex color: no
        // needs VFACE: no
        // needs SV_IsFrontFace: no
        // passes tangent-to-world matrix to pixel shader: no
        // reads from normal: no
        // 1 texcoords actually used
        //   float2 _MainTex
        #include "UnityCG.cginc"
        //Shader does not support lightmap thus we always want to fallback to SH.
        #undef UNITY_SHOULD_SAMPLE_SH
        #define UNITY_SHOULD_SAMPLE_SH (!defined(UNITY_PASS_FORWARDADD) && !defined(UNITY_PASS_SHADOWCASTER) && !defined(UNITY_PASS_META))
        #include "Lighting.cginc"
        #include "AutoLight.cginc"

        #define INTERNAL_DATA
        #define WorldReflectionVector(data,normal) data.worldRefl
        #define WorldNormalVector(data,normal) normal

        // Original surface shader snippet:
        #line 24 ""
        #ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
        #endif
        /* UNITY: Original start of shader */
        // Physically based Standard lighting model, and enable shadows on all light types
        //#pragma surface surf Lambert vertex:vert

        // Use shader model 3.0 target, to get nicer looking lighting
        //#pragma target 5.0 
        
        #include "../../Headers/PerlinNoise.cginc"
        #include "UnityCG.cginc"

        sampler2D _MainTex;
        sampler2D _MainTex1;
        sampler2D _MainTex2;

        struct Input
        {
          float2 uv_MainTex;
          float3 worldPos;
          float2 uv2;
          float3 localPos;
          int texIndex;
        };
        struct appdata{
          float4 vertex: POSITION;
          float2 texcoord: TEXCOORD0;
          float3 normal: NORMAL;
          float4 tangent: TANGENT;
          UNITY_VERTEX_INPUT_INSTANCE_ID
          uint id : SV_VertexID;
          uint inst : SV_InstanceID;
        };
        struct InstanceData{
          float3 pos;
          float4x4 trs;
          int texIndex;  
          int chunkIndex;
        };

        half _Glossiness;
        half _Metallic;
        float4 _Color;
        float _Scale;
        float4 _TopColor;
        float4 _BotColor;
        float _BlendFactor;
        float _SmoothnessState;
        float _WindSpeed;
        float _WindStrength;
        
          StructuredBuffer<InstanceData> instDatas; 
        
        void vert(inout appdata data, out Input o){
          UNITY_INITIALIZE_OUTPUT(Input, o);
          UNITY_SETUP_INSTANCE_ID(data)
            float3 worldPos = mul(instDatas[data.inst].trs, data.vertex).xyz;
            float3 worldOrigin = instDatas[data.inst].pos;   
            
            float2 offsetX = worldOrigin.xy / 5 + float2(_Time.y * _WindSpeed, 0);
            float2 offSetY = worldOrigin.xy / 5 + float2(0, _Time.y * _WindSpeed );
            float perlinVal = perlinNoise(offsetX) - 0.5;
            float perlinVal2 = perlinNoise(offSetY) - 0.5;
            float4 newPos = float4(worldPos, 0) + float4(perlinVal * _WindStrength , 0, perlinVal2 * _WindStrength, 0);
            data.vertex = newPos;
            data.normal = float3(0,1,0);
            o.texIndex = instDatas[data.inst].texIndex;
        }
        
        void surf (Input i, inout SurfaceOutput o)
        {    
          float texId = i.texIndex;
          float4 col ;
          
          // if(texId == 0) col = tex2D(_MainTex, i.uv_MainTex);
          // else if(texId == 1) col = tex2D(_MainTex1, i.uv_MainTex);
          // else if (texId == 2) col = tex2D(_MainTex2, i.uv_MainTex);
          col = tex2D(_MainTex, i.uv_MainTex);
          o.Albedo = col;
          //o.Emission = 0.2;
          // o.Smoothness = 0;
          // o.Metallic = 0;
          
        }
        

        // vertex-to-fragment interpolation data
        struct v2f_surf {
          UNITY_POSITION(pos);
          float2 pack0 : TEXCOORD0; // _MainTex
          float3 worldNormal : TEXCOORD1;
          float3 worldPos : TEXCOORD2;
          float3 custompack0 : TEXCOORD3; // localPos
          float2 custompack1 : TEXCOORD4; // texIndex
          UNITY_LIGHTING_COORDS(5,6)
          UNITY_FOG_COORDS(7)
          UNITY_VERTEX_INPUT_INSTANCE_ID
          UNITY_VERTEX_OUTPUT_STEREO
        };
        float4 _MainTex_ST;

        // vertex shader
        v2f_surf vert_surf (appdata v) {
          UNITY_SETUP_INSTANCE_ID(v);
          v2f_surf o;
          UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
          UNITY_TRANSFER_INSTANCE_ID(v,o);
          UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
          Input customInputData;
          vert (v, customInputData);
          o.custompack0.xyz = customInputData.localPos;
          o.custompack1.xy = customInputData.texIndex;
          o.pos = UnityObjectToClipPos(v.vertex);
          o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
          float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
          float3 worldNormal = UnityObjectToWorldNormal(v.normal);
          o.worldPos.xyz = worldPos;
          o.worldNormal = worldNormal;

          UNITY_TRANSFER_LIGHTING(o,half2(0.0, 0.0)); // pass shadow and, possibly, light cookie coordinates to pixel shader
          UNITY_TRANSFER_FOG(o,o.pos); // pass fog coordinates to pixel shader
          return o;
        }

        // fragment shader
        float4 frag_surf (v2f_surf IN) : SV_Target {
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
          surfIN.uv_MainTex.x = 1.0;
          surfIN.worldPos.x = 1.0;
          surfIN.uv2.x = 1.0;
          surfIN.localPos.x = 1.0;
          surfIN.uv_MainTex = IN.pack0.xy;
          surfIN.localPos = IN.custompack0.xyz;
          surfIN.texIndex = IN.custompack1.xy;
          float3 worldPos = IN.worldPos.xyz;
          #ifndef USING_DIRECTIONAL_LIGHT
            float3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
          #else
            float3 lightDir = _WorldSpaceLightPos0.xyz;
          #endif
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
          float3 normalWorldVertex = float3(0,0,1);
          o.Normal = IN.worldNormal;
          normalWorldVertex = IN.worldNormal;

          // call surface function
          surf (surfIN, o);
          UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
          float4 c = 0;

          // Setup lighting environment
          UnityGI gi;
          UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
          gi.indirect.diffuse = 0;
          gi.indirect.specular = 0;
          gi.light.color = _LightColor0.rgb;
          gi.light.dir = lightDir;
          gi.light.color *= atten;
          c += LightingLambert (o, gi);
          c.a = 0.0;
          UNITY_APPLY_FOG(_unity_fogCoord, c); // apply fog
          UNITY_OPAQUE_ALPHA(c.a);
          return c;
        }


      #endif


      ENDHLSL

    }

    // ---- deferred shading pass:
    Pass {
      Name "DEFERRED"
      Tags { "LightMode" = "Deferred" }

      HLSLPROGRAM
      // compile directives
      #pragma vertex vert_surf
      #pragma fragment frag_surf
      #pragma target 5.0 
      #pragma multi_compile_instancing
      #pragma exclude_renderers nomrt
      #pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
      #pragma multi_compile_prepassfinal nodynlightmap nolightmap
      #include "HLSLSupport.cginc"
      #define UNITY_INSTANCED_LOD_FADE
      #define UNITY_INSTANCED_SH
      #define UNITY_INSTANCED_LIGHTMAPSTS
      #define UNITY_INSTANCED_RENDERER_BOUNDS
      #include "UnityShaderVariables.cginc"
      #include "UnityShaderUtilities.cginc"
      // -------- variant for: <when no other keywords are defined>
      #if !defined(INSTANCING_ON)
        // Surface shader code generated based on:
        // vertex modifier: 'vert'
        // writes to per-pixel normal: no
        // writes to emission: no
        // writes to occlusion: no
        // needs world space reflection vector: no
        // needs world space normal vector: no
        // needs screen space position: no
        // needs world space position: no
        // needs view direction: no
        // needs world space view direction: no
        // needs world space position for lighting: YES
        // needs world space view direction for lighting: no
        // needs world space view direction for lightmaps: no
        // needs vertex color: no
        // needs VFACE: no
        // needs SV_IsFrontFace: no
        // passes tangent-to-world matrix to pixel shader: no
        // reads from normal: YES
        // 1 texcoords actually used
        //   float2 _MainTex
        #include "UnityCG.cginc"
        //Shader does not support lightmap thus we always want to fallback to SH.
        #undef UNITY_SHOULD_SAMPLE_SH
        #define UNITY_SHOULD_SAMPLE_SH (!defined(UNITY_PASS_FORWARDADD) && !defined(UNITY_PASS_SHADOWCASTER) && !defined(UNITY_PASS_META))
        #include "Lighting.cginc"

        #define INTERNAL_DATA
        #define WorldReflectionVector(data,normal) data.worldRefl
        #define WorldNormalVector(data,normal) normal

        // Original surface shader snippet:
        #line 24 ""
        #ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
        #endif
        /* UNITY: Original start of shader */
        // Physically based Standard lighting model, and enable shadows on all light types
        //#pragma surface surf Lambert vertex:vert

        // Use shader model 3.0 target, to get nicer looking lighting
        //#pragma target 5.0 
        
        #include "../../Headers/PerlinNoise.cginc"
        #include "UnityCG.cginc"

        sampler2D _MainTex;
        sampler2D _MainTex1;
        sampler2D _MainTex2;

        struct Input
        {
          float2 uv_MainTex;
          float3 worldPos;
          float2 uv2;
          float3 localPos;
          int texIndex;
        };
        struct appdata{
          float4 vertex: POSITION;
          float2 texcoord: TEXCOORD0;
          float3 normal: NORMAL;
          float4 tangent: TANGENT;
          UNITY_VERTEX_INPUT_INSTANCE_ID
          uint id : SV_VertexID;
          uint inst : SV_InstanceID;
        };
        struct InstanceData{
          float3 pos;
          float4x4 trs;
          int texIndex;  
          int chunkIndex;
        };

        half _Glossiness;
        half _Metallic;
        float4 _Color;
        float _Scale;
        float4 _TopColor;
        float4 _BotColor;
        float _BlendFactor;
        float _SmoothnessState;
        float _WindSpeed;
        float _WindStrength;
        
          StructuredBuffer<InstanceData> instDatas; 
        
        void vert(inout appdata data, out Input o){
          UNITY_INITIALIZE_OUTPUT(Input, o);
          UNITY_SETUP_INSTANCE_ID(data)
            float3 worldPos = mul(instDatas[data.inst].trs, data.vertex).xyz;
            float3 worldOrigin = instDatas[data.inst].pos;   
            
            float2 offsetX = worldOrigin.xy / 5 + float2(_Time.y * _WindSpeed, 0);
            float2 offSetY = worldOrigin.xy / 5 + float2(0, _Time.y * _WindSpeed );
            float perlinVal = perlinNoise(offsetX) - 0.5;
            float perlinVal2 = perlinNoise(offSetY) - 0.5;
            float4 newPos = float4(worldPos, 0) + float4(perlinVal * _WindStrength , 0, perlinVal2 * _WindStrength, 0);
            data.vertex = newPos;
            data.normal = float3(0,1,0);
            o.texIndex = instDatas[data.inst].texIndex;
        }
        
        void surf (Input i, inout SurfaceOutput o)
        {    
          float texId = i.texIndex;
          float4 col ;
          
          // if(texId == 0) col = tex2D(_MainTex, i.uv_MainTex);
          // else if(texId == 1) col = tex2D(_MainTex1, i.uv_MainTex);
          // else if (texId == 2) col = tex2D(_MainTex2, i.uv_MainTex);
          col = tex2D(_MainTex, i.uv_MainTex);
          o.Albedo = col;
          //o.Emission = 0.2;
          // o.Smoothness = 0;
          // o.Metallic = 0;
          
        }
        

        // vertex-to-fragment interpolation data
        struct v2f_surf {
          UNITY_POSITION(pos);
          float2 pack0 : TEXCOORD0; // _MainTex
          float3 worldNormal : TEXCOORD1;
          float3 worldPos : TEXCOORD2;
          float3 custompack0 : TEXCOORD3; // localPos
          float2 custompack1 : TEXCOORD4; // texIndex
          float4 lmap : TEXCOORD5;
          #ifndef LIGHTMAP_ON
            #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
              half3 sh : TEXCOORD6; // SH
            #endif
          #else
            #ifdef DIRLIGHTMAP_OFF
              float4 lmapFadePos : TEXCOORD6;
            #endif
          #endif
          UNITY_VERTEX_INPUT_INSTANCE_ID
          UNITY_VERTEX_OUTPUT_STEREO
        };
        float4 _MainTex_ST;

        // vertex shader
        v2f_surf vert_surf (appdata v) {
          UNITY_SETUP_INSTANCE_ID(v);
          v2f_surf o;
          UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
          UNITY_TRANSFER_INSTANCE_ID(v,o);
          UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
          Input customInputData;
          vert (v, customInputData);
          o.custompack0.xyz = customInputData.localPos;
          o.custompack1.xy = customInputData.texIndex;
          o.pos = UnityObjectToClipPos(v.vertex);
          o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
          float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
          float3 worldNormal = UnityObjectToWorldNormal(v.normal);
          o.worldPos.xyz = worldPos;
          o.worldNormal = worldNormal;
          o.lmap.zw = 0;
          #ifdef LIGHTMAP_ON
            o.lmap.xy = half2(0.0, 0.0) * unity_LightmapST.xy + unity_LightmapST.zw;
            #ifdef DIRLIGHTMAP_OFF
              o.lmapFadePos.xyz = (mul(unity_ObjectToWorld, v.vertex).xyz - unity_ShadowFadeCenterAndType.xyz) * unity_ShadowFadeCenterAndType.w;
              o.lmapFadePos.w = (-UnityObjectToViewPos(v.vertex).z) * (1.0 - unity_ShadowFadeCenterAndType.w);
            #endif
          #else
            o.lmap.xy = 0;
            #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
              o.sh = 0;
              o.sh = ShadeSHPerVertex (worldNormal, o.sh);
            #endif
          #endif
          return o;
        }
        #ifdef LIGHTMAP_ON
          float4 unity_LightmapFade;
        #endif
        float4 unity_Ambient;

        // fragment shader
        void frag_surf (v2f_surf IN,
        out half4 outGBuffer0 : SV_Target0,
        out half4 outGBuffer1 : SV_Target1,
        out half4 outGBuffer2 : SV_Target2,
        out half4 outEmission : SV_Target3
        #if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
          , out half4 outShadowMask : SV_Target4
        #endif
        ) {
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
          surfIN.uv_MainTex.x = 1.0;
          surfIN.worldPos.x = 1.0;
          surfIN.uv2.x = 1.0;
          surfIN.localPos.x = 1.0;
          surfIN.uv_MainTex = IN.pack0.xy;
          surfIN.localPos = IN.custompack0.xyz;
          surfIN.texIndex = IN.custompack1.xy;
          float3 worldPos = IN.worldPos.xyz;
          #ifndef USING_DIRECTIONAL_LIGHT
            float3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
          #else
            float3 lightDir = _WorldSpaceLightPos0.xyz;
          #endif
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
          float3 normalWorldVertex = float3(0,0,1);
          o.Normal = IN.worldNormal;
          normalWorldVertex = IN.worldNormal;

          // call surface function
          surf (surfIN, o);
          float3 originalNormal = o.Normal;
          half atten = 1;

          // Setup lighting environment
          UnityGI gi;
          UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
          gi.indirect.diffuse = 0;
          gi.indirect.specular = 0;
          gi.light.color = 0;
          gi.light.dir = half3(0,1,0);
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

          // call lighting function to output g-buffer
          outEmission = LightingLambert_Deferred (o, gi, outGBuffer0, outGBuffer1, outGBuffer2);
          #if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
            outShadowMask = UnityGetRawBakedOcclusions (IN.lmap.xy, worldPos);
          #endif
          #ifndef UNITY_HDR_ON
            outEmission.rgb = exp2(-outEmission.rgb);
          #endif
        }


      #endif

      // -------- variant for: INSTANCING_ON 
      #if defined(INSTANCING_ON)
        // Surface shader code generated based on:
        // vertex modifier: 'vert'
        // writes to per-pixel normal: no
        // writes to emission: no
        // writes to occlusion: no
        // needs world space reflection vector: no
        // needs world space normal vector: no
        // needs screen space position: no
        // needs world space position: no
        // needs view direction: no
        // needs world space view direction: no
        // needs world space position for lighting: YES
        // needs world space view direction for lighting: no
        // needs world space view direction for lightmaps: no
        // needs vertex color: no
        // needs VFACE: no
        // needs SV_IsFrontFace: no
        // passes tangent-to-world matrix to pixel shader: no
        // reads from normal: YES
        // 1 texcoords actually used
        //   float2 _MainTex
        #include "UnityCG.cginc"
        //Shader does not support lightmap thus we always want to fallback to SH.
        #undef UNITY_SHOULD_SAMPLE_SH
        #define UNITY_SHOULD_SAMPLE_SH (!defined(UNITY_PASS_FORWARDADD) && !defined(UNITY_PASS_SHADOWCASTER) && !defined(UNITY_PASS_META))
        #include "Lighting.cginc"

        #define INTERNAL_DATA
        #define WorldReflectionVector(data,normal) data.worldRefl
        #define WorldNormalVector(data,normal) normal

        // Original surface shader snippet:
        #line 24 ""
        #ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
        #endif
        /* UNITY: Original start of shader */
        // Physically based Standard lighting model, and enable shadows on all light types
        //#pragma surface surf Lambert vertex:vert

        // Use shader model 3.0 target, to get nicer looking lighting
        //#pragma target 5.0 
        
        #include "../../Headers/PerlinNoise.cginc"
        #include "UnityCG.cginc"

        sampler2D _MainTex;
        sampler2D _MainTex1;
        sampler2D _MainTex2;

        struct Input
        {
          float2 uv_MainTex;
          float3 worldPos;
          float2 uv2;
          float3 localPos;
          int texIndex;
        };
        struct appdata{
          float4 vertex: POSITION;
          float2 texcoord: TEXCOORD0;
          float3 normal: NORMAL;
          float4 tangent: TANGENT;
          UNITY_VERTEX_INPUT_INSTANCE_ID
          uint id : SV_VertexID;
          uint inst : SV_InstanceID;
        };
        struct InstanceData{
          float3 pos;
          float4x4 trs;
          int texIndex;  
          int chunkIndex;
        };

        half _Glossiness;
        half _Metallic;
        float4 _Color;
        float _Scale;
        float4 _TopColor;
        float4 _BotColor;
        float _BlendFactor;
        float _SmoothnessState;
        float _WindSpeed;
        float _WindStrength;
        
          StructuredBuffer<InstanceData> instDatas; 
        
        void vert(inout appdata data, out Input o){
          UNITY_INITIALIZE_OUTPUT(Input, o);
          UNITY_SETUP_INSTANCE_ID(data)
            float3 worldPos = mul(instDatas[data.inst].trs, data.vertex).xyz;
            float3 worldOrigin = instDatas[data.inst].pos;   
            
            float2 offsetX = worldOrigin.xy / 5 + float2(_Time.y * _WindSpeed, 0);
            float2 offSetY = worldOrigin.xy / 5 + float2(0, _Time.y * _WindSpeed );
            float perlinVal = perlinNoise(offsetX) - 0.5;
            float perlinVal2 = perlinNoise(offSetY) - 0.5;
            float4 newPos = float4(worldPos, 0) + float4(perlinVal * _WindStrength , 0, perlinVal2 * _WindStrength, 0);
            data.vertex = newPos;
            data.normal = float3(0,1,0);
            o.texIndex = instDatas[data.inst].texIndex;
        }
        
        void surf (Input i, inout SurfaceOutput o)
        {    
          float texId = i.texIndex;
          float4 col ;
          
          // if(texId == 0) col = tex2D(_MainTex, i.uv_MainTex);
          // else if(texId == 1) col = tex2D(_MainTex1, i.uv_MainTex);
          // else if (texId == 2) col = tex2D(_MainTex2, i.uv_MainTex);
          col = tex2D(_MainTex, i.uv_MainTex);
          o.Albedo = col;
          //o.Emission = 0.2;
          // o.Smoothness = 0;
          // o.Metallic = 0;
          
        }
        

        // vertex-to-fragment interpolation data
        struct v2f_surf {
          UNITY_POSITION(pos);
          float2 pack0 : TEXCOORD0; // _MainTex
          float3 worldNormal : TEXCOORD1;
          float3 worldPos : TEXCOORD2;
          float3 custompack0 : TEXCOORD3; // localPos
          float2 custompack1 : TEXCOORD4; // texIndex
          float4 lmap : TEXCOORD5;
          #ifndef LIGHTMAP_ON
            #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
              half3 sh : TEXCOORD6; // SH
            #endif
          #else
            #ifdef DIRLIGHTMAP_OFF
              float4 lmapFadePos : TEXCOORD6;
            #endif
          #endif
          UNITY_VERTEX_INPUT_INSTANCE_ID
          UNITY_VERTEX_OUTPUT_STEREO
        };
        float4 _MainTex_ST;

        // vertex shader
        v2f_surf vert_surf (appdata v) {
          UNITY_SETUP_INSTANCE_ID(v);
          v2f_surf o;
          UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
          UNITY_TRANSFER_INSTANCE_ID(v,o);
          UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
          Input customInputData;
          vert (v, customInputData);
          o.custompack0.xyz = customInputData.localPos;
          o.custompack1.xy = customInputData.texIndex;
          o.pos = UnityObjectToClipPos(v.vertex);
          o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
          float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
          float3 worldNormal = UnityObjectToWorldNormal(v.normal);
          o.worldPos.xyz = worldPos;
          o.worldNormal = worldNormal;
          o.lmap.zw = 0;
          #ifdef LIGHTMAP_ON
            o.lmap.xy = half2(0.0, 0.0) * unity_LightmapST.xy + unity_LightmapST.zw;
            #ifdef DIRLIGHTMAP_OFF
              o.lmapFadePos.xyz = (mul(unity_ObjectToWorld, v.vertex).xyz - unity_ShadowFadeCenterAndType.xyz) * unity_ShadowFadeCenterAndType.w;
              o.lmapFadePos.w = (-UnityObjectToViewPos(v.vertex).z) * (1.0 - unity_ShadowFadeCenterAndType.w);
            #endif
          #else
            o.lmap.xy = 0;
            #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
              o.sh = 0;
              o.sh = ShadeSHPerVertex (worldNormal, o.sh);
            #endif
          #endif
          return o;
        }
        #ifdef LIGHTMAP_ON
          float4 unity_LightmapFade;
        #endif
        float4 unity_Ambient;

        // fragment shader
        void frag_surf (v2f_surf IN,
        out half4 outGBuffer0 : SV_Target0,
        out half4 outGBuffer1 : SV_Target1,
        out half4 outGBuffer2 : SV_Target2,
        out half4 outEmission : SV_Target3
        #if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
          , out half4 outShadowMask : SV_Target4
        #endif
        ) {
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
          surfIN.uv_MainTex.x = 1.0;
          surfIN.worldPos.x = 1.0;
          surfIN.uv2.x = 1.0;
          surfIN.localPos.x = 1.0;
          surfIN.uv_MainTex = IN.pack0.xy;
          surfIN.localPos = IN.custompack0.xyz;
          surfIN.texIndex = IN.custompack1.xy;
          float3 worldPos = IN.worldPos.xyz;
          #ifndef USING_DIRECTIONAL_LIGHT
            float3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
          #else
            float3 lightDir = _WorldSpaceLightPos0.xyz;
          #endif
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
          float3 normalWorldVertex = float3(0,0,1);
          o.Normal = IN.worldNormal;
          normalWorldVertex = IN.worldNormal;

          // call surface function
          surf (surfIN, o);
          float3 originalNormal = o.Normal;
          half atten = 1;

          // Setup lighting environment
          UnityGI gi;
          UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
          gi.indirect.diffuse = 0;
          gi.indirect.specular = 0;
          gi.light.color = 0;
          gi.light.dir = half3(0,1,0);
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

          // call lighting function to output g-buffer
          outEmission = LightingLambert_Deferred (o, gi, outGBuffer0, outGBuffer1, outGBuffer2);
          #if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
            outShadowMask = UnityGetRawBakedOcclusions (IN.lmap.xy, worldPos);
          #endif
          #ifndef UNITY_HDR_ON
            outEmission.rgb = exp2(-outEmission.rgb);
          #endif
        }


      #endif


      ENDHLSL

    }

    // ---- end of surface shader generated code

    #LINE 112

  }
}
