using UnityEngine;

namespace BoxHead2.Utils
{
    public static class ShaderPropertyIDs
    {
        public static readonly int UnityMatrixMvpId = Shader.PropertyToID("_UNITY_MATRIX_MVP"); 
        public static readonly int AllGrassBufferId = Shader.PropertyToID("_AllGrassBuffer"); 
        public static readonly int VisibleGrassIDBufferId = Shader.PropertyToID("_VisibleGrassIDBuffer"); 
        public static readonly int DispatchOffsetId = Shader.PropertyToID("_DispatchOffset"); 
        public static readonly int PositionOffset = Shader.PropertyToID("_PositionOffset"); 
        public static readonly int MaxDrawDistanceId = Shader.PropertyToID("_MaxDrawDistance");
        public static readonly int DayNightShadowColorId = Shader.PropertyToID("_DayNightShadowColor");
        public static readonly int IsDayId = Shader.PropertyToID("_IsDay");
        public static readonly int CloudShadowMapId = Shader.PropertyToID("_CloudShadow");
        public static readonly int CloudShadowConfigId = Shader.PropertyToID("_CloudShadowConfig");
        public static readonly int TMPId1 = Shader.PropertyToID("tmpBlurRT1");
        public static readonly int TMPId2 = Shader.PropertyToID("tmpBlurRT2");
        public static readonly int FowMapId = Shader.PropertyToID("_FOWMap");
        public static readonly int FowAlphaId = Shader.PropertyToID("_FOWAlpha");
        public static readonly int FowFlowMapId = Shader.PropertyToID("_FOWFlowMap");
        public static readonly int FowNoiseId = Shader.PropertyToID("_FOWNoise");
        public static readonly int FowBufferId = Shader.PropertyToID("_FOWBuffer");
        public static readonly int AllHexBufferId = Shader.PropertyToID("_AllHexBuffer");
        public static readonly int InteractVectorId = Shader.PropertyToID("_InteractVector");
        public static readonly int ChunkMapId = Shader.PropertyToID("_ChunkMap");
        public static readonly int ChunkPositionId = Shader.PropertyToID("_ChunkPosition");
        public static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        public static readonly int IndexId = Shader.PropertyToID("_Index");
        public static readonly int FillId = Shader.PropertyToID("_Fill");
        public static readonly int ArcId = Shader.PropertyToID("_Arc");
        public static readonly int BillboardEnlargeId = Shader.PropertyToID("_BillboardEnlarge");
        public static readonly int CaveFactorId = Shader.PropertyToID("_CaveFactor");
        public static readonly int DitherId = Shader.PropertyToID("_Dither");
        public static readonly int AlphaId = Shader.PropertyToID("_Alpha");
    }
}