// ====================================================================
// VERTEX SHADERS 
vs_out vs_model(vs_in v)
{
    vs_out o = (vs_out)0.0f; // cast all output values to zero to prevent potential errors
    UNITY_SETUP_INSTANCE_ID(v); 
    UNITY_INITIALIZE_OUTPUT(vs_out, o); 
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); 

    float4 pos_ws  = mul(unity_ObjectToWorld, v.vertex);
    float4 pos_wvp = mul(UNITY_MATRIX_VP, pos_ws);
    o.pos = pos_wvp;
    o.uv_a.xy = v.uv_0.xy;
    o.uv_a.zw = v.uv_1.xy;
    o.uv_b.xy = v.uv_2.xy;
    o.uv_b.zw = v.uv_3.xy;
    o.normal = mul((float3x3)unity_ObjectToWorld, v.normal);
    o.tangent.xyz = mul((float3x3)unity_ObjectToWorld, v.tangent);
    o.tangent.w = v.tangent.w * unity_WorldTransformParams.w;
    o.view = camera_position() - mul(unity_ObjectToWorld, v.vertex).xyz;
    o.ws_pos =  mul(unity_ObjectToWorld, v.vertex);
    o.ss_pos = ComputeScreenPos(o.pos);
    o.v_col = v.v_col;
    o.light_pos = mul(_LightMatrix0, o.ws_pos);

    // parallax shit
    // cringe
    float3 bitangent = cross(o.normal.xyz, o.tangent.xyz) * o.tangent.w;
    float3 five;
    float3 six;
    float3 parallax;

    float3 view = normalize(o.view);

    five.x = o.tangent.z;
    five.y = bitangent.x;
    five.z = o.normal.x;
    six.x = o.tangent.x;
    six.y = bitangent.z;
    six.z = o.normal.y;

    parallax = view.yyy * six;
    parallax = five * view.xxx + parallax;
    bitangent.x = o.tangent.y;
    bitangent.z = o.normal.z;
    parallax = bitangent * view.zzz + parallax;
    o.parallax = parallax;  

    TRANSFER_SHADOW(o)

    return o; // output to pixel shader
}

vs_out vs_edge(vs_in v)
{
    vs_out o = (vs_out)0.0f; // cast all output values to zero to prevent potential errors
    if(_OutlineType ==  0.0f)
    {
        vs_out o = (vs_out)0.0f;
    }
    else
    {
        float3 outline_normal = (_OutlineType == 1.0) ? v.normal : v.tangent.xyz;
        float4 wv_pos = mul(UNITY_MATRIX_MV, v.vertex);
        float3 view = _WorldSpaceCameraPos.xyz - (float3)mul(v.vertex.xyz, unity_ObjectToWorld);
        o.view = normalize(view);
        float3 ws_normal = mul(outline_normal, (float3x3)unity_ObjectToWorld);

        outline_normal = mul((float3x3)UNITY_MATRIX_MV, outline_normal);
        outline_normal.z = 0.01f;
        outline_normal.xy = normalize(outline_normal.xyz).xy;

        if(!_FallbackOutlines)
        {
            float fov_matrix = unity_CameraProjection[1].y;

            float fov = 2.414f / fov_matrix; // may need to come back in and change this back to 1.0f
            // can't remember in what vrchat mode this was messing up 

            float depth = -wv_pos.z * fov;

            float2 adjs = (depth <  _OutlineWidthAdjustZs.y) ? _OutlineWidthAdjustZs.xy : _OutlineWidthAdjustZs.yz; 
            float2 scales = (depth <  _OutlineWidthAdjustZs.y) ? _OutlineWidthAdjustScales.xy : _OutlineWidthAdjustScales.yz; 
            
            float z_scale = depth + -(adjs.x);
            float2 z_something = float2((-adjs.x) + adjs.y, (-scales.x) + scales.y);
            z_something.x = max(z_something.x, 0.001f);
            z_scale = z_scale / z_something.x;
            z_scale = saturate(z_scale);
            z_scale = z_scale * z_something.y + scales.x;

            // the next 5 or so lines could be written in one line like the above 
            float outline_scale = (_OutlineWidth * 1.5f) * z_scale;
            outline_scale = outline_scale * 100.0f;
            outline_scale = outline_scale * _Scale;
            outline_scale = outline_scale * 0.414f;
            outline_scale = outline_scale * v.v_col.w;
            if(_UseFaceMapNew) outline_scale = outline_scale * _FaceMapTex.SampleLevel(sampler_FaceMapTex, v.uv_0.xy, 0.0f).z;

            float offset_depth = saturate(1.0f - depth);
            float max_offset = lerp(_MaxOutlineZOffset * 0.1, _MaxOutlineZOffset, offset_depth);

            float3 z_offset = (wv_pos.xyz) * (float3)max_offset * (float3)_Scale;
            // the above version of the line causes a floating point division by zero warning in unity even though it didnt used to do that
            // but it probably has something to do with the instancing support v added

            float blue = v.v_col.z + -0.5f; // never trust this fucking line
            // it always breaks things

            o.pos = wv_pos;
            o.pos.xyz = (o.pos.xyz + (z_offset)) + (outline_normal.xyz * outline_scale);
        }
        else
        {
            o.pos = wv_pos;
            o.pos.xyz = o.pos.xyz + (outline_normal.xyz * (_OutlineWidth * 100.0f * _Scale * 0.414f * v.v_col.w));
        }

        o.ws_pos = o.pos;
        
        o.pos = mul(UNITY_MATRIX_P, o.pos);
        o.ss_pos = ComputeScreenPos(o.pos);
        o.normal = normalize(ws_normal);
        o.uv_a.xy = v.uv_0;
        o.uv_a.zw = v.uv_1;
        o.v_col = v.v_col;
    }
    return o;
}

shadow_out vs_shadow(shadow_in v)
{
    shadow_out o = (shadow_out)0.0f; // initialize so no funny compile errors
    float3 view = _WorldSpaceCameraPos.xyz - (float3)mul(v.vertex.xyz, unity_ObjectToWorld);
    o.view = normalize(view);
    o.normal = mul((float3x3)unity_ObjectToWorld, v.normal);
    float4 pos_ws  = mul(unity_ObjectToWorld, v.vertex);
    o.ws_pos = pos_ws;
    float4 pos_wvp = mul(UNITY_MATRIX_VP, pos_ws);
    o.pos = pos_wvp;
    o.uv_a = float4(v.uv_0.xy, v.uv_1.xy);
    TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
    return o;
}


// ====================================================================
// PIXEL SHADERS
float4 ps_model(vs_out i,  bool vface : SV_ISFRONTFACE) : SV_TARGET
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
    // FUTURE PROOFING :
    if(!_StarCloakEnable) 
    {
        _StarCockType = 10;
    }
    
    // stupid legacy specular support, please mihoyo dont change value names again
    float specular_enabled = 0;
    if(((_SpecularHighlights == 1) && (_UseToonSpecular == 1)) || ((_SpecularHighlights == 1) && (_UseToonSpecular == 0)) || ((_SpecularHighlights == 0) && (_UseToonSpecular == 1)))
    { // cringe ass code
        specular_enabled = 1;
    }
    // GET LIGHT ATTENUATION FOR BOTH PASSES : 
    UNITY_LIGHT_ATTENUATION(atten, i, i.ws_pos.xyz);

    // INITIALIZE PIXEL SHADER OUTPUT: 
    float4 out_color = (float4)1.0f; 
    // INITIALIZE VERTEX INPUTS: 
    float3 normal = normalize(i.normal);
    normal = (vface) ? normal : -normal; // check if back facing and invert the normals 
    float3 view   = normalize(i.view);
    float2 uv_a = (!vface && _UseBackFaceUV2) ? i.uv_a.zw : i.uv_a.xy; 
    float4 uv_b = i.uv_b;
    float3 light = _WorldSpaceLightPos0.xyz;
        
    // SAMPLE TEXTURES : 
    float4 diffuse = _MainTex.Sample(sampler_MainTex, uv_a);
    float4 lightmap = _LightMapTex.Sample(sampler_LightMapTex, uv_a);
    float customao  = _CustomAO.Sample(sampler_LightMapTex, uv_a);
    float4 normalmap = _BumpMap.Sample(sampler_BumpMap, uv_a);
    float4 facemap = _FaceMapTex.Sample(sampler_FaceMapTex, uv_a);

    #ifdef _IS_PASS_BASE // Basic character shading pass, should only include the basic enviro light stuff + debug rendering shit
        float2 metalspec;
        metalspec.x = lightmap.x < 0.50f;
        metalspec.y = lightmap.x < 0.90f; // if metal area

        if(_UseBumpMap) normal = normal_mapping(normalmap, i.ws_pos, uv_a, normal);
        if(_UseBumpMap && _TextureLineUse) detail_line(i.ss_pos.zw, normalmap.z, diffuse.xyz);

        // do this after the bump mapping to ensure that the normals are updated 
        // INITIALIZE INPUT floatTORS :
        float3 halffloattor = normalize(light + view);
        float ndotl = dot(normal, light);
        float ndotv = dot(normal, view);
        float ndoth = dot(normal, halffloattor);

        // lighting
        float3 ambient_color = max(half3(0.05f, 0.05f, 0.05f), max(ShadeSH9(half4(0.0, 0.0, 0.0, 1.0)),ShadeSH9(half4(0.0, -1.0, 0.0, 1.0)).rgb));
        float3 light_color = max(ambient_color, _LightColor0.rgb);

        float3 GI_color = DecodeLightProbe(normal);
        GI_color = GI_color < float3(1,1,1) ? GI_color : float3(1,1,1);
        float GI_intensity = 0.299f * GI_color.r + 0.587f * GI_color.g + 0.114f * GI_color.b;
        GI_intensity = GI_intensity < 1 ? GI_intensity : 1.0f;

        // MATERIAL REGION 
        float material_id = materialID(lightmap.w);

        // COLOR SHIT 
        if(_MainTexColoring) diffuse = maintint(diffuse);
        // sample mask texture
        float4 material_mask = _MaterialMasksTex.Sample(sampler_LightMapTex, uv_a);
        
        // HUE MASKS: 
        float diffuse_mask = packed_channel_picker(sampler_LightMapTex, _HueMaskTexture, uv_a, _DiffuseMaskSource);
        float rim_mask = packed_channel_picker(sampler_LightMapTex, _HueMaskTexture, uv_a, _RimMaskSource);
        float emission_mask = packed_channel_picker(sampler_LightMapTex, _HueMaskTexture, uv_a, _EmissionMaskSource);

        if(!_UseHueMask)
        {
            diffuse_mask = 1.0f;
            rim_mask = 1.0f;
            emission_mask = 1.0f;
        }
              
        if(_MainTexAlphaUse == 0) diffuse.w = 1.0f;
        if(_MainTexAlphaUse == 3) diffuse.xyz = lerp(diffuse, _FaceBlushColor, diffuse.w * _FaceBlushStrength);
        if(_MainTexAlphaUse == 1) clip(diffuse.w - _MainTexAlphaCutoff);

        // EMISSION SHIT :
        float emis_check = (_MainTexAlphaUse == 2 || _EmissionType == 1);
        float emis_check_eye = _EyePulse ? _ToggleEyeGlow * pulsate(_PulseSpeed, _PulseMinStrength, _PulseMaxStrength, _EyeTimeOffset) : _ToggleEyeGlow;
        float mask = diffuse.w; 
        float eye_mask = 0.0f;
        if(_EmissionType == 1) mask = _CustomEmissionTex.Sample(sampler_MainTex, uv_a).x;
        if(_ToggleEyeGlow == 1) eye_mask = lightmap.y > 0.95f;
        mask = saturate(mask - 0.02f); // removing any artifacts
        if(_StarCloakEnable && _StarCockEmis) mask = 1.0f;
        if((_StarCloakEnable && _StarCockEmis) && (_StarCockType == 2) && (!emis_check)) mask = diffuse.w;
        emis_check = _TogglePulse ? emis_check * pulsate(_PulseSpeed, _PulseMinStrength, _PulseMaxStrength, 0.0f) : emis_check; 


        if(_UseFaceMapNew)
        {
            mask = 0.0f;
            eye_mask = 0.0f;
        }

        // SHADOW
        float3 shadow;
        float3 metalshadow;
        float3 s_color;
        shadow_color(lightmap.y, i.v_col.x, customao, i.v_col.y, ndotl, material_id, i.uv_a.xy, shadow, metalshadow, s_color, light);
        // SPECULAR : 
        float3 specular = (float3)0.0f;
        if(_SpecularHighlights) specular_color(ndoth, shadow, lightmap.x, lightmap.z, material_id, specular);
        if(lightmap.x > 0.90f) specular = 0.0f; // making sure the specular doesnt bleed into the metal area
        // METALIC :
        if(_MetalMaterial) metalics(metalshadow, normal, ndoth, lightmap.x, vface, diffuse.xyz);
        if(_DebugMode && (_DebugMetal == 1)) return float4(diffuse.xyz, 1.0f);
        // moving these after the metal so the metal can also be recolored
        if(!_DisableColors) diffuse = diffuse * coloring(material_id, material_mask);
        if(_UseMaterialMasksTex && !_DisableColors) diffuse = diffuse * material_mask_coloring(material_mask);
        if(_EnableColorHue) diffuse.xyz = hue_shift(diffuse.xyz, material_id, _ColorHue, _ColorHue2, _ColorHue3, _ColorHue4, _ColorHue5, _GlobalColorHue, _AutomaticColorShift, _ShiftColorSpeed, diffuse_mask);
        out_color = diffuse;
        out_color.w = 1.0f;

        if(_HandEffectEnable)
        {
            arm_effect(out_color, i.uv_a.xy, i.uv_a.zw, i.uv_b.xy, view, normal, ndotl);\
            if(_EnableColorHue) out_color.xyz = hue_shift(out_color.xyz, material_id, _ColorHue, _ColorHue2, _ColorHue3, _ColorHue4, _ColorHue5, _GlobalColorHue, _AutomaticColorShift, _ShiftColorSpeed, diffuse_mask);

            out_color.xyz = out_color.xyz * light_color;
            out_color.xyz = out_color.xyz + (GI_color * GI_intensity * _GI_Intensity * smoothstep(1.0f ,0.0f, GI_intensity / 2.0f));
            return out_color;
        }

        // FRESNEL : 
        // this is used for skill effects or certain idle animations
        // think like that one idle animation raiden has
        fresnel_hit(ndotv, out_color.xyz);

        if(_StarCloakEnable) star_cocks(float4(out_color.xyz, diffuse.w), i.uv_a.xy, i.uv_a.zw, i.uv_b.xy, i.ss_pos, ndotv, light, i.parallax);
        
        out_color.xyz = out_color.xyz * s_color + specular;
        
        float3 emis_color = out_color.xyz;
        float3 emis_color_eye = out_color.xyz;
        emis_color = emission_color(emis_color, material_id);
        emis_color_eye = emission_color_eyes(emis_color, material_id);
        if(_EnableEmissionHue) emis_color.xyz = hue_shift(emis_color.xyz, material_id, _EmissionHue, _EmissionHue2, _EmissionHue3, _EmissionHue4, _EmissionHue5, _GlobalEmissionHue, _AutomaticEmissionShift, _ShiftEmissionSpeed, emission_mask);
        if(_EnableEmissionHue) emis_color_eye.xyz = hue_shift(emis_color_eye.xyz, material_id, _EmissionHue, _EmissionHue2, _EmissionHue3, _EmissionHue4, _EmissionHue5, _GlobalEmissionHue, _AutomaticEmissionShift, _ShiftEmissionSpeed, emission_mask);

        if(_StarCloakEnable && _StarCockEmis)
        {
            emis_color = emis_color.xyz;
            mask = _StarCockType == 2 ? diffuse.w : 1.0f;
            emis_check = 1.0f;
        }
        
        out_color.xyz = out_color.xyz * light_color;

        
        
        float3 rim_light = rimlighting(i.ss_pos, normal, i.ws_pos, light, material_id, out_color.xyz, view);
        if(_UseFaceMapNew) normal = float3(0.5f, 0.5f, 1.0f);
        if(_EnableRimHue) rim_light.xyz = hue_shift(rim_light.xyz, material_id, _RimHue, _RimHue2, _RimHue3, _RimHue4, _RimHue5, _GlobalRimHue, _AutomaticRimShift, _ShiftRimSpeed, rim_mask);
        if(_UseRimLight) out_color.xyz = out_color.xyz + rim_light;
        out_color.xyz = out_color.xyz + (GI_color * GI_intensity * _GI_Intensity * smoothstep(1.0f ,0.0f, GI_intensity / 2.0f));

        out_color.xyz = lerp(out_color.xyz, emis_color, (mask * emis_check));
        out_color.xyz = lerp(out_color.xyz, emis_color_eye, (eye_mask * emis_check_eye));
        
        // out_color.xyz = material_mask;
        // basic ass transparency
        if(_MainTexAlphaUse == 4) out_color.w = diffuse.w;        
        if(_DebugMode) // debuuuuuug
        {
            if(_DebugDiffuse == 1) return float4(diffuse.xyz, 1.0f);
            if(_DebugDiffuse == 2) return float4(diffuse.www, 1.0f);
            if(_DebugLightMap == 1) return float4(lightmap.xxx, 1.0f);
            if(_DebugLightMap == 2) return float4(lightmap.yyy, 1.0f);
            if(_DebugLightMap == 3) return float4(lightmap.zzz, 1.0f);
            if(_DebugLightMap == 4) return float4(lightmap.www, 1.0f);
            if(_DebugFaceMap == 1) return float4(facemap.xxx, 1.0f);
            if(_DebugFaceMap == 2) return float4(facemap.yyy, 1.0f);
            if(_DebugFaceMap == 3) return float4(facemap.zzz, 1.0f);
            if(_DebugFaceMap == 4) return float4(facemap.www, 1.0f);
            if(_DebugNormalMap == 1) return float4(normalmap.xy, 1.0f, 1.0f);
            if(_DebugNormalMap == 2) return float4(normalmap.zzz, 1.0f);
            if(_DebugVertexColor == 1) return float4(i.v_col.xxx, 1.0f);
            if(_DebugVertexColor == 2) return float4(i.v_col.yyy, 1.0f);
            if(_DebugVertexColor == 3) return float4(i.v_col.zzz, 1.0f);
            if(_DebugVertexColor == 4) return float4(i.v_col.www, 1.0f);
            if(_DebugRimLight == 1) return float4(rim_light.xyz, 1.0f);
            if(_DebugNormalVector == 1) return float4(i.normal.xyz * 0.5f + 0.5f, 1.0f);
            if(_DebugNormalVector == 2) return float4(i.normal.xyz, 1.0f);
            if(_DebugNormalVector == 3) return float4(normal.xyz * 0.5f + 0.5f, 1.0f);
            if(_DebugNormalVector == 4) return float4(normal.xyz, 1.0f);
            if(_DebugTangent == 1) return float4(i.tangent.xyz, 1.0f);
            if(_DebugSpecular == 1) return float4(specular.xyz, 1.0f);
            if(_DebugEmission == 1) return float4((mask * emis_check).xxx, 1.0f);
            if(_DebugEmission == 2) return float4(emis_color.xyz, 1.0f);
            if(_DebugEmission == 3) return float4(emis_color.xyz * (mask * emis_check).xxx, 1.0f);
            if(_DebugFaceVector == 1) return float4(normalize(UnityObjectToWorldDir(_headForwardVector.xyz)).xyz, 1.0f);
            if(_DebugFaceVector == 2) return float4(normalize(UnityObjectToWorldDir(_headRightVector.xyz)).xyz, 1.0f);
            if((_DebugMaterialIDs > 0) && (_DebugMaterialIDs != 6))
            {
                if(_DebugMaterialIDs == material_id)
                {
                    return (float4)1.0f;
                }
                else 
                {
                    return float4((float3)0.0f, 1.0f);
                }
            }
            if(_DebugMaterialIDs == 6)
            {
                float4 debug_color = float4(0.0f, 0.0f, 0.0f, 1.0f);
                if(material_id == 1)
                {
                    debug_color.xyz = float3(1.0f, 0.0f, 0.0f);
                }
                else if(material_id == 2)
                {
                    debug_color.xyz = float3(0.0f, 1.0f, 0.0f);
                }
                else if(material_id == 3)
                {
                    debug_color.xyz = float3(0.0f, 0.0f, 1.0f);
                }
                else if(material_id == 4)
                {
                    debug_color.xyz = float3(1.0f, 0.0f, 1.0f);
                }
                else if(material_id == 5)
                {
                    debug_color.xyz = float3(0.0f, 1.0f, 1.0f);
                }
                return debug_color;
            }
            if(_DebugLights == 1) return float4((float3)0.0f, 1.0f);
        }
        
    #endif
    #ifdef _IS_PASS_LIGHT // Lighting shading pass, should only include the necessary lighting things needed
        if(_UseFaceMapNew) normal = float3(0.5f, 0.5f, 1.0f);
        #if defined(POINT) || defined(SPOT)
        light = normalize(_WorldSpaceLightPos0.xyz - i.ws_pos.xyz);
        #endif
        
        // MATERIAL ID: 
        float material_id = materialID(lightmap.w);

        // SHADOW
        // since this pass doesnt want the colors of the shadows, just use the shadow only functions: 
        float ndotl = dot(normal, light);

        float3 shadow_area = (float3)1.0f;
        shadow_area = shadow_area_transition(lightmap.y, i.v_col.x, ndotl, material_id);
        // metalshadow = shadow_area_transition(lightmapao, vertexao, ndotl, material_id);
        if(_UseFaceMapNew) shadow_area = shadow_area_face(uv_a.xy, light).xxx;
        // shadow = outshadow;

        float bright = lightmap.y > 0.9f && !_UseFaceMapNew;

        float light_intesnity = max(0.001f, (0.299f * _LightColor0.r + 0.587f * _LightColor0.g + 0.114f * _LightColor0.b));
        float3 light_pass_color = ((diffuse.xyz * 5.0f) * _LightColor0.xyz) * atten * shadow_area * 0.5f;
        float3 light_color = lerp(light_pass_color.xyz, lerp(0.0f, min(light_pass_color, light_pass_color / light_intesnity), _WorldSpaceLightPos0.w), _FilterLight); // prevents lights from becoming too intense
        #if defined(POINT) || defined(SPOT)
        out_color.xyz = (light_color * saturate(1.0f - bright)) * 0.5f;
        #elif defined(DIRECTIONAL)
        out_color.xyz = 0.0f; // dont let extra directional lights add onto the model, this will fuck a lot of shit up
        #endif
        
    #endif
    if(_UseWeapon) weapon_shit(out_color.xyz, diffuse.w, i.uv_a.zw, normal, view, i.ws_pos);
    return out_color; 
}

float4 ps_edge(vs_out i, bool vface: SV_ISFRONTFACE) : SV_TARGET
{
    float4 out_color = (float4)1.0f;

    float outline_mask = packed_channel_picker(sampler_LightMapTex, _HueMaskTexture, i.uv_a.xy, _OutlineMaskSource);
    outline_mask = _UseHueMask ? outline_mask : 1.0f;

    float alpha = _MainTex.Sample(sampler_MainTex, i.uv_a.xy);
    float4 lightmap = _LightMapTex.Sample(sampler_LightMapTex, i.uv_a.xy);

    float material_ID = materialID(lightmap.w);

    float4 outline_color[5] =
    {
        _OutlineColor, _OutlineColor2, _OutlineColor3, _OutlineColor4, _OutlineColor5
    };
    
    float3 emission;
    out_color = outline_color[material_ID - 1];
    if(_EnableOutlineGlow)
    {
        emission = outline_emission(out_color.xyz, material_ID);
        out_color.xyz = emission;
    }
    
    if(_MainTexAlphaUse == 1) clip(alpha - _MainTexAlphaCutoff);
    if(_MainTexAlphaUse == 4) out_color.w = alpha;
    if(_UseWeapon) weapon_shit(out_color.xyz, alpha, i.uv_a.zw, i.normal, i.view, i.ws_pos);
    if(_EnableOutlineHue) out_color.xyz = hue_shift(out_color.xyz, material_ID, _OutlineHue, _OutlineHue2, _OutlineHue3, _OutlineHue4, _OutlineHue5, _GlobalOutlineHue, _AutomaticOutlineShift, _ShiftOutlineSpeed, 1.0f);
    if(_MultiLight && !_EnableOutlineGlow) out_color.xyz = out_color.xyz  * (UNITY_LIGHTMODEL_AMBIENT.xyz + _LightColor0.xyz);
    
    return out_color;
}

float4 ps_shadow(shadow_out i, bool vface : SV_ISFRONTFACE) : SV_TARGET
{
    // initialize uv 
    float2 uv = (!vface && _UseBackFaceUV2) ? i.uv_a.zw : i.uv_a.xy;

    float alpha = _MainTex.Sample(sampler_MainTex, uv).w;

    float4 out_color = (float4)0.0f;
    
    if(_MainTexAlphaUse == 1) clip(alpha - _MainTexAlphaCutoff);
    if(_MainTexAlphaUse == 4) out_color.w = alpha;
    if(_UseWeapon) weapon_shit(out_color.xyz, alpha, i.uv_a.zw, i.normal, i.view, i.ws_pos);
    return 0.0f;
}

// ====================================================================
