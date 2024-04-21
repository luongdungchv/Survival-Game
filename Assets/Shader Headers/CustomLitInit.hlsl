InputData InitializePBRInput(Varyings input){
    InputData pbrInput = (InputData)0;
    pbrInput.positionWS = input.positionWS;
    pbrInput.positionCS = input.positionCS;
    pbrInput.fogCoord = input.fogCoord;
    pbrInput.shadowCoord = input.shadowCoord;
    pbrInput.normalWS = NormalizeNormalPerPixel(input.normalWS);
    pbrInput.viewDirectionWS = input.viewDirectionWS;
    pbrInput.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.vertexSH, pbrInput.normalWS);
    return pbrInput;
}
SurfaceData InitializeSurfaceData(Varyings input){
    SurfaceData surfData = (SurfaceData)0;
    half4 baseMap = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
    surfData.albedo = baseMap;
    surfData.smoothness = _Glossiness;
    surfData.metallic = _Metallic;
    surfData.alpha = baseMap.a;
    return surfData;
}