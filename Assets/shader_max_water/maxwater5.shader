// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// converted to Unity - mgear - http://graphicsrunner.blogspot.fi/2009/07/water-in-your-browser.html
// not exactly working yet..

Shader "mShaders/maxwater5" {
	Properties {
		
		WaterColor ("WaterColor", Color)  = (0.5,0.79,0.75, 1)
		
		SunDirection ("SunDirection", Vector) = (2.6, -1.0, -1.5, 1.0)
		SunColor ("SunColor", Color)  = (1,0.8,0.4, 1)
		SunFactor ("SunFactor", Float) = 1.5
		SunPower ("SunPower", Float) = 1
		SunShininess ("SunShininess", Float) = 250
		
		
		EnvMap1 ("EnvMap1", Cube) = "" {}
		
		WaveMapS0 ("WaveMapS0", 2D) = "white" {}
		WaveMapS1 ("WaveMapS1", 2D) = "white" {}
		
		RO ("RO", Float) = 0.02037

		ReflectMap ("ReflectMap", 2D) = "white" {}
		//ReflectMap ("ReflectMap", 2D) = "" { TexGen ObjectLinear }
		//		RefractMap ("RefractMap", 2D) = "" { TexGen ObjectLinear }
		RefractMap ("RefractMap", 2D) = "white" {}


	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Lambert vertex:vert nolightmap nodirlightmap
		#include "UnityCG.cginc"
		#pragma glsl

		// http://forum.unity3d.com/threads/142461-Shader-error-too-many-quot-TEXCOORD-quot
		// Program 'frag_surf', input semantic attribute "TEXCOORD" has too big of a numeric index (8) at line 74 Keywords: DIRECTIONAL, LIGHTMAP_OFF, DIRLIGHTMAP_OFF, SHADOWS_SCREEN
		
		

		float4 WaterColor;
		float4 SunDirection;
		float4 SunColor;
		
		float SunFactor;
		float SunPower;
		float SunShininess;
		
		float RO;

		sampler2D ReflectMap;
		sampler2D RefractMap;
		sampler2D WaveMapS0;
		sampler2D WaveMapS1;
		samplerCUBE EnvMap1;




		struct Input {
			float2 uvReflectMap;
			//			float3 viewDir;
			
			float4 posH; //			: POSITION0;
			float3 toEyeW		: TEXCOORD0;
			float3 eye;
			//			float2 tex0			: TEXCOORD1;
			//			float2 tex1			: TEXCOORD2;
			float4 projTexC;//		: TEXCOORD3;	
			//float4 screenPos;
			float3 worldRefl;
			INTERNAL_DATA
		};

		/*
		float3 water_scattering(float3 InColor, float depth, float length)
		{
			float extinction = exp(-0.02f*max(depth,0));
			float alpha = (1 - extinction*exp(-max(length,0)*0.2f));
			
			return lerp(InColor, 0.7f*float3(0.157, 0.431, 0.706), alpha);   
		}
		*/

		void vert (inout appdata_full v, out Input o) {
			
			UNITY_INITIALIZE_OUTPUT(Input,o);
			// Transform vertex position to world space.
			//float3 posW = mul (UNITY_MATRIX_MVP, v.vertex).xyz; // which transforms the local position directly into viewspace.
			float4 posW = UnityObjectToClipPos (v.vertex); // which transforms the local position directly into viewspace.
			//float3 posW  = mul(float4(posL, 1.0f), World).xyz;
			// posL = local pos?
			
			//			float3 eye = float3(ViewInv._41, ViewInv._42, ViewInv._43);
			float3 eyeOrig = WorldSpaceViewDir(v.vertex);
			o.eye = eyeOrig;
			
			// Compute the unit vector from the vertex to the eye.
			//o.toEyeW = posW - eyeOrig; // bug
			//o.toEyeW= ObjSpaceViewDir(v.vertex); // ok?, if minus, same error..
			//o.toEyeW= ObjSpaceViewDir(-v.vertex); // ok?
			o.toEyeW= WorldSpaceViewDir(v.vertex); // ok?
			//o.toEyeW= float3(1,1,1);
			
			// Transform to homogeneous clip space.
			//o.posH = mul(float4(posL, 1.0f), WorldViewProj);
			o.posH = mul(float4(v.vertex.xyz, 1.0f), UNITY_MATRIX_MVP);
			
			// Scroll texture coordinates.
			//o.tex0 = (tex0 * TexScale) + WaveMapOffset0;
			//o.tex1 = (tex0 * TexScale) + WaveMapOffset1;
			
			// Generate projective texture coordinates from camera's perspective.
			o.projTexC = o.posH;
			
		}		
		

		void surf (Input IN, inout SurfaceOutput o) 
		{
			
			//float3 eye = normalize(-posW.xyz);
			
			IN.projTexC.xyz /= IN.projTexC.w;            
			IN.projTexC.x =  0.5f*IN.projTexC.x + 0.5f; 
			IN.projTexC.y = -0.5f*IN.projTexC.y + 0.5f;
			IN.projTexC.z = .1f / IN.projTexC.z;
			
			//ObjectSpaceCameraPos
			
			float3 toEyeW = normalize(IN.toEyeW);
			float3 sunDir = normalize(SunDirection.xyz);
			
			float3 lightVecW = -sunDir;	
			
			// Sample normal map.
			float3 normalT0 = tex2D(WaveMapS0, IN.uvReflectMap+float2(_Time.x*0.2f,0)).rgb;
			float3 normalT1 = tex2D(WaveMapS1, IN.uvReflectMap+float2(_Time.y*0.05f,0)).rgb;
			
			//unroll the normals retrieved from the normalmaps
			normalT0.yz = normalT0.zy;	
			normalT1.yz = normalT1.zy;
			
			normalT0 = 2.0f*normalT0 - 1.0f;
			normalT1 = 2.0f*normalT1 - 1.0f;
			
			float3 normalT = normalize(0.5f*(normalT0 + normalT1));
			float3 n1 = float3(0,1,0); //we'll just use the y unit vector for spec reflection.
			
			//get the reflection vector from the eye
			float3 R = normalize(reflect(IN.toEyeW, normalT));	
			
			float r0 = 0.02037f;
			float ang = saturate(dot(-IN.toEyeW, n1));
			float f = r0 + (1.0f - r0) * pow(1.0f - ang, 5.0f);	
			
			//also blend based on distance
			//			float3 eye = float3(ViewInv._41, ViewInv._42, ViewInv._43);
			//float3 eye = float3(ViewInv._41, ViewInv._42, ViewInv._43);
			f = min(1.0f, f + 0.007f * IN.eye.y);
			//f = min(1.0f, f + 0.007f * IN.viewDir.y);
			
			float4 finalColor;
			finalColor.a = 1;
			
			//float4 reflectColor = tex2D(ReflectMap, IN.projTexC.xy + IN.projTexC.z * normalT.xz);//texCUBE(EnvMapS, R);
			//			float4 reflectColor = texCUBE(EnvMap1, IN.worldRefl); // R = IN.worldRefl ?
			float4 reflectColor = texCUBE(EnvMap1, R); // R = IN.worldRefl ?
			//			float4 reflectColor = texCUBE (EnvMap1, WorldReflectionVector (IN, o.Normal)).rgba;
			//			float4 refractColor = tex2D(RefractMap, IN.projTexC.xy - IN.projTexC.z * normalT.xz);

			//float2 screenUV = IN.screenPos.xy / IN.screenPos.w;

			float4 refractColor = tex2D(RefractMap, IN.projTexC.xy - IN.projTexC.z * normalT.xz);
			//float4 refractColor = tex2Dproj(RefractMap, UNITY_PROJ_COORD(IN.uvReflectMap)) * half4(1,0,0,1);
			
			//			float4 refractColor = tex2D (RefractMap, IN.uvReflectMap).rgba;
			//			float4 refractColor = tex2D (RefractMap, IN.uvReflectMap).rgba;
			
			
			
			//float3 sunlight = SunFactor * pow(saturate(dot(R, lightVecW)), SunPower) * SunColor.rgb;
			float3 sunlight = SunFactor * pow(saturate(dot(R, lightVecW)), SunPower) * SunColor.rgb;

			//finalColor.xyz = refractColor.xyz;// + sunlight.xyz;//refractColor.xyz;
			//finalColor.xyz = lerp( refractColor.xyz, reflectColor.xyz, f).xyz;
			//finalColor.xyz = (WaterColor.xyz * texCUBE(EnvMapS, R).xyz) + sunlight.xyz;
			finalColor.xyz = WaterColor.rgb * lerp( refractColor, reflectColor, f).rgb + sunlight.rgb;
			
			
			
			
			
			
			
			//half4 c = tex2D (ReflectMap, IN.uvReflectMap);
			
			
			
			o.Albedo = finalColor.rgb;
			o.Alpha = finalColor.a;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}

