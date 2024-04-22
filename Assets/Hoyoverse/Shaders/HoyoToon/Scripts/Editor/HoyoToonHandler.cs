using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
public class HoyoToonHandler
{
    #region Constants
    public const string HSRShader = "HoyoToon/Star Rail/Character";
    private const string GIShader = "HoyoToon/Genshin/Character";
    private const string Hi3Shader = "HoyoToon/Honkai Impact/Character Part 1";
    private const string Hi3P2Shader = "HoyoToon/Honkai Impact/Character Part 2";
    private static readonly string[] clampKeyword = { "Dissolve", "ramp", "Star" };
    private static readonly string[] nonSRGBKeywords = { "normalmap", "lightmap", "face_shadow", "specular_ramp", "gradient", "Grain", "Dissolve", "Repeat", "Stockings", "ExpressionMap", "FaceMap", "materialidvalueslut", "ColorMask", "_Mask", "_Normal" };
    private static readonly string[] NonPower2Keywords = { "materialidvalueslut" };

    public enum BodyType
    {
        GIBoy,
        GIGirl,
        GILady,
        GIMale,
        GILoli,
        HSRMaid,
        HSRKid,
        HSRLad,
        HSRMale,
        HSRLady,
        HSRGirl,
        HSRBoy,
        HSRMiss,
        HI3P1,
        Hi3P2
    }

    public static BodyType currentBodyType;

    #endregion

    #region Setup

    [MenuItem("Assets/HoyoToon/Setup FBX")]
    private static void SetupFBX()
    {
        SetFBXImportSettings(GetAssetSelectionPaths());
    }

    // [MenuItem("Assets/HoyoToon/Bodytype")]
    // private static void CheckBody()
    // {
    //     DetermineBodyType();
    // }

    #endregion

    #region Parsing

    private static string[] GetAssetSelectionPaths()
    {
        return Selection.assetGUIDs.Select(AssetDatabase.GUIDToAssetPath).ToArray();
    }

    public static void DetermineBodyType()
    {
        string selectedAssetPath = AssetDatabase.GetAssetPath(Selection.activeObject);

        if (Selection.activeObject is GameObject gameObject)
        {
            MeshFilter[] meshFilters = gameObject.GetComponentsInChildren<MeshFilter>();
            SkinnedMeshRenderer[] skinnedMeshRenderers = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();

            Mesh mesh = null;

            if (meshFilters.Length > 0)
            {
                mesh = meshFilters[0].sharedMesh;
            }
            else if (skinnedMeshRenderers.Length > 0)
            {
                mesh = skinnedMeshRenderers[0].sharedMesh;
            }

            if (mesh == null)
            {
                throw new MissingComponentException("<color=purple>[Hoyotoon]</color> The GameObject or its children must have a MeshFilter or SkinnedMeshRenderer component.");
            }

            selectedAssetPath = AssetDatabase.GetAssetPath(mesh);
        }
        else
        {
            selectedAssetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
        }


        string directoryPath = Path.GetDirectoryName(selectedAssetPath);
        string texturesPath = Path.Combine(directoryPath, "Textures");

        if (Directory.Exists(texturesPath))
        {
            string[] textureFiles = Directory.GetFiles(texturesPath, "*.png");
            bool bodyTypeSet = false;

            foreach (string textureFile in textureFiles)
            {
                string textureName = Path.GetFileNameWithoutExtension(textureFile);

                if (!bodyTypeSet && textureName.ToLower().Contains("hair_mask".ToLower()))
                {
                    currentBodyType = BodyType.Hi3P2;
                    bodyTypeSet = true;
                    Debug.Log($"<color=purple>[Hoyotoon]</color> Matched texture: {textureName} with BodyType.Hi3P2");
                }
                else if (!bodyTypeSet && textureName.ToLower().Contains("expressionmap".ToLower()))
                {
                    if (textureFile.Contains("Lady")) { currentBodyType = BodyType.HSRLady; bodyTypeSet = true; }
                    else if (textureName.Contains("Maid")) { currentBodyType = BodyType.HSRMaid; bodyTypeSet = true; }
                    else if (textureName.Contains("Girl")) { currentBodyType = BodyType.HSRGirl; bodyTypeSet = true; }
                    else if (textureName.Contains("Kid")) { currentBodyType = BodyType.HSRKid; bodyTypeSet = true; }
                    else if (textureName.Contains("Lad")) { currentBodyType = BodyType.HSRLad; bodyTypeSet = true; }
                    else if (textureName.Contains("Male")) { currentBodyType = BodyType.HSRMale; bodyTypeSet = true; }
                    else if (textureName.Contains("Boy")) { currentBodyType = BodyType.HSRBoy; bodyTypeSet = true; }
                    else if (textureName.Contains("Miss")) { currentBodyType = BodyType.HSRMiss; bodyTypeSet = true; }
                }
                else if (!bodyTypeSet && textureName.ToLower().Contains("lightmap".ToLower()))
                {
                    if (textureName.Contains("Boy")) { currentBodyType = BodyType.GIBoy; bodyTypeSet = true; }
                    else if (textureName.Contains("Girl")) { currentBodyType = BodyType.GIGirl; bodyTypeSet = true; }
                    else if (textureName.Contains("Lady")) { currentBodyType = BodyType.GILady; bodyTypeSet = true; }
                    else if (textureName.Contains("Male")) { currentBodyType = BodyType.GIMale; bodyTypeSet = true; }
                    else if (textureName.Contains("Loli")) { currentBodyType = BodyType.GILoli; bodyTypeSet = true; }
                    else if (!textureName.ToLower().Contains("girl") && !textureName.ToLower().Contains("lady")
                    && !textureName.ToLower().Contains("male") && !textureName.ToLower().Contains("loli") && !Regex.IsMatch(textureName, @"\d{2}"))
                    {
                        currentBodyType = BodyType.HI3P1;
                        bodyTypeSet = false;
                        Debug.Log($"<color=purple>[Hoyotoon]</color> Matched texture: {textureName} with BodyType.Hi3P1");
                    }
                }
            }
            if (!bodyTypeSet)
            {
                currentBodyType = BodyType.HI3P1;
                Debug.Log($"<color=purple>[Hoyotoon]</color> No specific match found. Setting BodyType to HI3P1");
            }
        }
        Debug.Log($"<color=purple>[Hoyotoon]</color> Current Body Type: {currentBodyType}");
    }

    #endregion

    #region Material Generation

    [MenuItem("Assets/HoyoToon/Generate Materials")]
    public static void GenerateMaterialsFromJson()
    {
        // Start asset editing
        AssetDatabase.StartAssetEditing();

        try
        {
            DetermineBodyType();
            var textureCache = new Dictionary<string, Texture>();
            UnityEngine.Object selectedObject = Selection.activeObject;
            string selectedPath = AssetDatabase.GetAssetPath(selectedObject);

            List<string> loadedTexturePaths = new List<string>();
            if (Path.GetExtension(selectedPath) == ".json")
            {
                // Process the selected JSON file
                ProcessJsonFile(selectedPath, textureCache, loadedTexturePaths);
            }
            else
            {
                string materialsFolderPath = Path.GetDirectoryName(selectedPath) + "/Materials";
                if (materialsFolderPath != null)
                {
                    if (Directory.Exists(materialsFolderPath))
                    {
                        string[] jsonFiles = Directory.GetFiles(materialsFolderPath, "*.json");
                        foreach (string jsonFile in jsonFiles)
                        {
                            ProcessJsonFile(jsonFile, textureCache, loadedTexturePaths);
                        }
                    }
                }
                else
                {
                    Debug.LogError("<color=purple>[Hoyotoon]</color> Materials folder path does not exist. Ensure your materials are in a folder named 'Materials'.");
                }
            }
        }
        finally
        {
            // Stop asset editing
            AssetDatabase.StopAssetEditing();

            // Save assets and refresh the asset database once
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    private static void ProcessJsonFile(string jsonFile, Dictionary<string, Texture> textureCache, List<string> loadedTexturePaths)
    {
        TextAsset jsonTextAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(jsonFile);
        string jsonContent = jsonTextAsset.text;
        JObject jsonObject = JObject.Parse(jsonContent);
        string jsonFileName = Path.GetFileNameWithoutExtension(jsonFile);

        Dictionary<string, string> shaderKeys = new Dictionary<string, string>
    {
        { "_UtilityDisplay1", GIShader },
        {"_DisableCGP", GIShader},
        { "_SPCubeMapIntensity", Hi3Shader },
        { "_DissolveDistortionIntensity", HSRShader },
        { "_ScreenLineInst", HSRShader},
        { "_RampTexV", Hi3P2Shader},
        { "_MiscGrp", Hi3P2Shader}
    };

        Shader shaderToApply = null;

        foreach (var shaderKey in shaderKeys)
        {
            JToken texEnvsToken = jsonObject["m_SavedProperties"]["m_TexEnvs"];
            JToken floatsToken = jsonObject["m_SavedProperties"]["m_Floats"];

            bool texEnvsContainsKey = ContainsKey(texEnvsToken, shaderKey.Key);
            bool floatsContainsKey = ContainsKey(floatsToken, shaderKey.Key);

            if (texEnvsContainsKey || floatsContainsKey)
            {
                Shader shader = Shader.Find(shaderKey.Value);
                if (shader != null)
                {
                    shaderToApply = shader;
                    break;
                }
            }
        }

        if (shaderToApply != null)
        {
            string materialPath = Path.GetDirectoryName(jsonFile) + "/" + jsonFileName + ".mat";

            Material newMaterial = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            bool isNewMaterial = false;
            if (newMaterial == null)
            {
                newMaterial = new Material(shaderToApply);
                isNewMaterial = true;
            }

            ProcessProperties(jsonObject["m_SavedProperties"]["m_Floats"], newMaterial, (propertyName, propertyValue) =>
            {
                if (newMaterial.HasProperty(propertyName) && propertyValue.Type == JTokenType.Float)
                {
                    newMaterial.SetFloat(propertyName, propertyValue.Value<float>());
                }
            });

            ProcessProperties(jsonObject["m_SavedProperties"]["m_Colors"], newMaterial, (propertyName, propertyValue) =>
            {
                if (newMaterial.HasProperty(propertyName))
                {
                    JObject colorObject = propertyValue.ToObject<JObject>();
                    Color color = new Color(colorObject["r"].Value<float>(), colorObject["g"].Value<float>(), colorObject["b"].Value<float>(), colorObject["a"].Value<float>());
                    newMaterial.SetColor(propertyName, color);
                }
            });

            ProcessProperties(jsonObject["m_SavedProperties"]["m_TexEnvs"], newMaterial, (propertyName, propertyValue) =>
            {
                if (newMaterial.HasProperty(propertyName))
                {

                    JObject textureObject = propertyValue["m_Texture"].ToObject<JObject>();

                    if (!textureObject.ContainsKey("Name"))
                    {
                        throw new Exception("<color=purple>[Hoyotoon]</color> You're using outdated materials. Please download/extract using the latest AssetStudio.");
                    }

                    string textureName = textureObject["Name"].Value<string>();

                    if (!string.IsNullOrEmpty(textureName))
                    {
                        Texture texture = null;

                        // Check if the texture is in the cache
                        if (textureCache.ContainsKey(textureName))
                        {
                            texture = textureCache[textureName];
                        }
                        else
                        {
                            string[] textureGUIDs = AssetDatabase.FindAssets(textureName + " t:texture");

                            if (textureGUIDs.Length > 0)
                            {
                                string texturePath = AssetDatabase.GUIDToAssetPath(textureGUIDs[0]);
                                texture = AssetDatabase.LoadAssetAtPath<Texture>(texturePath);

                                // Add the texture to the cache
                                if (texture != null)
                                {
                                    textureCache.Add(textureName, texture);
                                }
                            }
                        }

                        if (texture != null)
                        {
                            newMaterial.SetTexture(propertyName, texture);
                            string texturePath = AssetDatabase.GetAssetPath(texture);
                            loadedTexturePaths.Add(texturePath);

                            Vector2 scale = new Vector2(propertyValue["m_Scale"]["X"].Value<float>(), propertyValue["m_Scale"]["Y"].Value<float>());
                            Vector2 offset = new Vector2(propertyValue["m_Offset"]["X"].Value<float>(), propertyValue["m_Offset"]["Y"].Value<float>());
                            newMaterial.SetTextureScale(propertyName, scale);
                            newMaterial.SetTextureOffset(propertyName, offset);
                        }
                    }
                }
            });

            ApplyCustomSettingsToMaterial(newMaterial, jsonFileName);

            if (isNewMaterial)
            {
                HardSetTextures(newMaterial, loadedTexturePaths);
                AssetDatabase.CreateAsset(newMaterial, materialPath);
            }
        }
        else
        {
            Debug.LogError("<color=purple>[Hoyotoon]</color> No compatible shader found for " + jsonFileName);
        }
    }

    public static void HardSetTextures(Material newMaterial, List<string> loadedTexturePaths)
    {
        var textureMap = new Dictionary<string, string>
    {
        { "_MTMap", "Avatar_Tex_MetalMap" },
        { "_MTSpecularRamp", "Avatar_Tex_Specular_Ramp"},
        { "_DissolveMap", "Eff_Noise_607" },
        { "_DissolveMask", "UI_Noise_29" },
        { "_WeaponDissolveTex", "Eff_WeaponsTotem_Dissolve_00" },
        { "_WeaponPatternTex", "Eff_WeaponsTotem_Grain_00" },
        { "_ScanPatternTex", "Eff_Gradient_Repeat_01" }
    };

        if (currentBodyType.ToString().StartsWith("GI"))
        {
            string bodyType = currentBodyType.ToString().Substring(2);
            textureMap["_FaceMapTex"] = $"Avatar_{bodyType}_Tex_FaceLightmap";
        }

        // Cache for loaded textures
        var textureCache = new Dictionary<string, Texture>();

        foreach (var textureProperty in textureMap)
        {
            string textureName = textureProperty.Value;
            Texture texture = null;

            // Check if the texture is in the cache
            if (textureCache.ContainsKey(textureName))
            {
                texture = textureCache[textureName];
            }
            else
            {
                string[] textureGUIDs = AssetDatabase.FindAssets(textureName + " t:texture");

                if (textureGUIDs.Length > 0)
                {
                    string texturePath = AssetDatabase.GUIDToAssetPath(textureGUIDs[0]);
                    texture = AssetDatabase.LoadAssetAtPath<Texture>(texturePath);

                    // Add the texture to the cache
                    if (texture != null)
                    {
                        textureCache.Add(textureName, texture);
                    }
                }
            }

            if (texture != null)
            {
                newMaterial.SetTexture(textureProperty.Key, texture);
                string texturePath = AssetDatabase.GetAssetPath(texture);
                loadedTexturePaths.Add(texturePath);
            }
        }
        SetTextureImportSettings(loadedTexturePaths);
    }

    private static void ProcessProperties(JToken token, Material material, Action<string, JToken> action)
    {
        if (token is JArray array)
        {
            foreach (var item in array)
            {
                string propertyName = item["Key"].Value<string>();
                JToken propertyValue = item["Value"];
                action(propertyName, propertyValue);
            }
        }
        else if (token is JObject obj)
        {
            foreach (var item in obj)
            {
                string propertyName = item.Key;
                JToken propertyValue = item.Value;
                action(propertyName, propertyValue);
            }
        }
    }

    private static bool ContainsKey(JToken token, string key)
    {
        if (token is JArray array)
        {
            return array.Any(j => j["Key"].Value<string>() == key);
        }
        else if (token is JObject obj)
        {
            return obj.ContainsKey(key);
        }
        return false;
    }

    public static void ApplyCustomSettingsToMaterial(Material material, string jsonFileName)
    {
        if (material.shader.name == HSRShader && jsonFileName.Contains("Face"))
        {
            material.SetInt("variant_selector", 1);
            material.SetInt("_BaseMaterial", 0);
            material.SetInt("_HairMaterial", 0);
            material.SetInt("_FaceMaterial", 1);
            material.SetInt("_EyeShadowMat", 0);
            material.SetInt("_CullMode", 2);
            material.SetInt("_SrcBlend", 1);
            material.SetInt("_DstBlend", 0);
            material.SetInt("_StencilPassA", 0);
            material.SetInt("_StencilPassB", 2);
            material.SetInt("_StencilCompA", 5);
            material.SetInt("_StencilCompB", 5);
            material.SetInt("_StencilRef", 100);
            material.renderQueue = 2010;
        }
        else if (material.shader.name == HSRShader && jsonFileName.Contains("EyeShadow"))
        {
            material.SetInt("variant_selector", 2);
            material.SetInt("_BaseMaterial", 0);
            material.SetInt("_HairMaterial", 0);
            material.SetInt("_FaceMaterial", 0);
            material.SetInt("_EyeShadowMat", 1);
            material.SetInt("_CullMode", 0);
            material.SetInt("_SrcBlend", 2);
            material.SetInt("_DstBlend", 0);
            material.SetInt("_StencilPassA", 0);
            material.SetInt("_StencilPassB", 2);
            material.SetInt("_StencilCompA", 0);
            material.SetInt("_StencilCompB", 8);
            material.SetInt("_StencilRef", 0);
            material.renderQueue = 2015;
        }
        else if (material.shader.name == HSRShader && jsonFileName.Contains("FaceMask"))
        {
            material.SetInt("variant_selector", 1);
            material.SetInt("_BaseMaterial", 0);
            material.SetInt("_HairMaterial", 0);
            material.SetInt("_FaceMaterial", 1);
            material.SetInt("_EyeShadowMat", 0);
            material.SetInt("_CullMode", 0);
            material.SetInt("_SrcBlend", 1);
            material.SetInt("_DstBlend", 0);
            material.SetInt("_StencilPassA", 0);
            material.SetInt("_StencilPassB", 2);
            material.SetInt("_StencilCompA", 5);
            material.SetInt("_StencilCompB", 5);
            material.SetInt("_StencilRef", 99);
            material.SetInt("_OutlineWidth", 0);
            material.renderQueue = 2010;
        }
        else if (material.shader.name == HSRShader && jsonFileName.Contains("Trans"))
        {
            material.SetInt("_IsTransparent", 1);
            material.SetInt("variant_selector", 0);
            material.SetInt("_BaseMaterial", 1);
            material.SetInt("_HairMaterial", 0);
            material.SetInt("_FaceMaterial", 0);
            material.SetInt("_EyeShadowMat", 0);
            material.SetInt("_CullMode", 0);
            material.SetInt("_SrcBlend", 5);
            material.SetInt("_DstBlend", 10);
            material.SetInt("_StencilPassA", 2);
            material.SetInt("_StencilPassB", 0);
            material.SetInt("_StencilCompA", 0);
            material.SetInt("_StencilCompB", 0);
            material.SetInt("_StencilRef", 0);
            material.renderQueue = 2041;
        }
        else if (material.shader.name == HSRShader && jsonFileName.Contains("Hair"))
        {
            material.SetInt("variant_selector", 3);
            material.SetInt("_BaseMaterial", 0);
            material.SetInt("_HairMaterial", 1);
            material.SetInt("_FaceMaterial", 0);
            material.SetInt("_EyeShadowMat", 0);
            material.SetInt("_CullMode", 0);
            material.SetInt("_SrcBlend", 1);
            material.SetInt("_DstBlend", 0);
            material.SetInt("_StencilPassA", 0);
            material.SetInt("_StencilPassB", 0);
            material.SetInt("_StencilCompA", 5);
            material.SetInt("_StencilCompB", 8);
            material.SetInt("_StencilRef", 100);
            material.renderQueue = 2020;
        }
        else if (material.shader.name == HSRShader)
        {
            material.SetInt("variant_selector", 0);
            material.SetInt("_BaseMaterial", 1);
            material.SetInt("_HairMMaterial", 0);
            material.SetInt("_FaceMaterial", 0);
            material.SetInt("_EyeShadowMat", 0);
            material.SetInt("_CullMode", 0);
            material.SetInt("_SrcBlend", 5);
            material.SetInt("_DstBlend", 10);
            material.SetInt("_StencilPassA", 2);
            material.SetInt("_StencilPassB", 0);
            material.SetInt("_StencilCompA", 0);
            material.SetInt("_StencilCompB", 0);
            material.SetInt("_StencilRef", 0);
            material.SetFloat("_OutlineScale", 0.187f);
            material.SetFloat("_RimWidth", 1f);
            material.renderQueue = 2040;
        }
        else if (material.shader.name == GIShader && jsonFileName.Contains("Face"))
        {
            material.SetInt("variant_selector", 1);
            material.SetInt("_UseFaceMapNew", 1);
        }
        else if (material.shader.name == GIShader && jsonFileName.Contains("Equip"))
        {
            material.SetInt("variant_selector", 2);
            material.SetInt("_UseWeapon", 1);
        }
        else if (material.shader.name == GIShader)
        {
            material.SetInt("variant_selector", 0);
        }
        else if (material.shader.name == Hi3Shader && jsonFileName.Contains("Face"))
        {
            material.SetInt("variant_selector", 1);
            material.SetInt("_StencilPassA", 0);
            material.SetInt("_StencilPassB", 2);
            material.SetInt("_StencilCompA", 6);
            material.SetInt("_StencilCompB", 8);
            material.SetInt("_StencilRef", 16);
            material.renderQueue = 2000;
        }
        else if (material.shader.name == Hi3Shader && jsonFileName.Contains("Hair"))
        {
            material.SetInt("variant_selector", 2);
            material.SetInt("_StencilPassA", 0);
            material.SetInt("_StencilPassB", 2);
            material.SetInt("_StencilCompA", 6);
            material.SetInt("_StencilCompB", 8);
            material.SetInt("_StencilRef", 16);
            material.renderQueue = 2002;
        }
        else if (material.shader.name == Hi3Shader && jsonFileName.Contains("Eye"))
        {
            material.SetInt("variant_selector", 3);
            material.SetInt("_StencilPassA", 0);
            material.SetInt("_StencilPassB", 2);
            material.SetInt("_StencilCompA", 6);
            material.SetInt("_StencilCompB", 8);
            material.SetInt("_StencilRef", 16);
            material.renderQueue = 2001;
        }
        else if (material.shader.name == Hi3Shader && jsonFileName.Contains("Alpha"))
        {
            material.SetInt("_AlphaType", 1);
            material.SetInt("_SrcBlend", 5);
            material.SetInt("_DstBlend", 10);
            material.renderQueue = 2003;
        }
        else if (material.shader.name == Hi3P2Shader && jsonFileName.Contains("Face"))
        {
            material.SetInt("variant_selector", 1);
            material.SetInt("_StencilPassA", 0);
            material.SetInt("_StencilPassB", 2);
            material.SetInt("_StencilCompA", 6);
            material.SetInt("_StencilCompB", 8);
            material.SetInt("_StencilRef", 16);
            material.renderQueue = 2000;
        }
        else if (material.shader.name == Hi3P2Shader && jsonFileName.Contains("Hair"))
        {
            material.SetInt("variant_selector", 2);
            material.SetInt("_StencilPassA", 0);
            material.SetInt("_StencilPassB", 2);
            material.SetInt("_StencilCompA", 6);
            material.SetInt("_StencilCompB", 8);
            material.SetInt("_StencilRef", 16);
            material.renderQueue = 2002;
        }
        else if (material.shader.name == Hi3P2Shader && jsonFileName.Contains("Eye"))
        {
            material.SetInt("variant_selector", 3);
            material.SetInt("_StencilPassA", 0);
            material.SetInt("_StencilPassB", 2);
            material.SetInt("_StencilCompA", 6);
            material.SetInt("_StencilCompB", 8);
            material.SetInt("_StencilRef", 16);
            material.renderQueue = 2001;
        }
    }

    private static void SetTextureImportSettings(IEnumerable<string> paths)
    {
        var pathsToReimport = new List<string>();

        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (var p in paths)
            {
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(p);
                if (!texture) continue;

                TextureImporter importer = AssetImporter.GetAtPath(p) as TextureImporter;
                if (!importer) continue;

                bool settingsChanged = false;

                if (importer.textureType != TextureImporterType.Default ||
                    importer.textureCompression != TextureImporterCompression.Uncompressed ||
                    importer.mipmapEnabled != false ||
                    importer.streamingMipmaps != false ||
                    (clampKeyword.Any(k => texture.name.IndexOf(k, System.StringComparison.InvariantCultureIgnoreCase) >= 0) && importer.wrapMode != TextureWrapMode.Clamp) ||
                    (nonSRGBKeywords.Any(k => texture.name.IndexOf(k, System.StringComparison.InvariantCultureIgnoreCase) >= 0) && importer.sRGBTexture != false) ||
                    (NonPower2Keywords.Any(k => texture.name.IndexOf(k, System.StringComparison.InvariantCultureIgnoreCase) >= 0) && importer.npotScale != TextureImporterNPOTScale.None))
                {
                    importer.textureType = TextureImporterType.Default;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.mipmapEnabled = false;
                    importer.streamingMipmaps = false;

                    if (clampKeyword.Any(k => texture.name.IndexOf(k, System.StringComparison.InvariantCultureIgnoreCase) >= 0))
                        importer.wrapMode = TextureWrapMode.Clamp;

                    if (nonSRGBKeywords.Any(k => texture.name.IndexOf(k, System.StringComparison.InvariantCultureIgnoreCase) >= 0))
                        importer.sRGBTexture = false;

                    if (NonPower2Keywords.Any(k => texture.name.IndexOf(k, System.StringComparison.InvariantCultureIgnoreCase) >= 0))
                        importer.npotScale = TextureImporterNPOTScale.None;

                    settingsChanged = true;
                }

                if (settingsChanged)
                {
                    pathsToReimport.Add(p);
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }

        foreach (var p in pathsToReimport)
        {
            AssetDatabase.ImportAsset(p, ImportAssetOptions.ForceUpdate);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    #endregion

    #region FBX Setup

    private static void SetFBXImportSettings(IEnumerable<string> paths)
    {
        bool changesMade = false;

        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (var p in paths)
            {
                var fbx = AssetDatabase.LoadAssetAtPath<Mesh>(p);
                if (!fbx) continue;

                ModelImporter importer = AssetImporter.GetAtPath(p) as ModelImporter;
                if (!importer) continue;

                importer.globalScale = 1;
                importer.isReadable = true;
                importer.SearchAndRemapMaterials(ModelImporterMaterialName.BasedOnMaterialName, ModelImporterMaterialSearch.Everywhere);
                if (importer.animationType != ModelImporterAnimationType.Human || importer.avatarSetup != ModelImporterAvatarSetup.CreateFromThisModel)
                {
                    importer.animationType = ModelImporterAnimationType.Human;
                    importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                    changesMade = true;
                }

                if (ModifyAndSaveHumanoidBoneMapping(importer))
                {
                    changesMade = true;
                }

                string pName = "legacyComputeAllNormalsFromSmoothingGroupsWhenMeshHasBlendShapes";
                PropertyInfo prop = importer.GetType().GetProperty(pName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                prop.SetValue(importer, true);
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }

        if (changesMade)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    private static bool ModifyAndSaveHumanoidBoneMapping(ModelImporter importer)
    {
        HumanDescription humanDescription = importer.humanDescription;
        List<HumanBone> humanBones = new List<HumanBone>(humanDescription.human);

        bool changesMade = false;

        for (int i = 0; i < humanBones.Count; i++)
        {
            if (humanBones[i].humanName == "Jaw")
            {
                humanBones.RemoveAt(i);
                changesMade = true;
                break;
            }
        }

        string leftEyeBoneName = null;
        string rightEyeBoneName = null;

        if (humanBones.Exists(bone => bone.boneName == "+EyeBoneLA02" || bone.boneName == "EyeBoneLA02"))
        {
            leftEyeBoneName = "+EyeBoneLA02";
            rightEyeBoneName = "+EyeBoneRA02";
        }
        else if (humanBones.Exists(bone => bone.boneName == "Eye_L"))
        {
            leftEyeBoneName = "Eye_L";
            rightEyeBoneName = "Eye_R";
        }
        else
        {
            leftEyeBoneName = "Eye_L";
            rightEyeBoneName = "Eye_R";
        }

        for (int i = 0; i < humanBones.Count; i++)
        {
            if (humanBones[i].humanName == "LeftEye")
            {
                HumanBone bone = humanBones[i];
                bone.boneName = leftEyeBoneName;
                humanBones[i] = bone;
                changesMade = true;
            }
            else if (humanBones[i].humanName == "RightEye")
            {
                HumanBone bone = humanBones[i];
                bone.boneName = rightEyeBoneName;
                humanBones[i] = bone;
                changesMade = true;
            }
        }

        if (changesMade)
        {
            humanDescription.human = humanBones.ToArray();
            importer.humanDescription = humanDescription;
        }

        if (changesMade)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        return changesMade;
    }

    #endregion

    #region Tangent Generation

    [MenuItem("GameObject/HoyoToon/Generate Tangents", false, 0)]
    public static void GenTangents()
    {
        DetermineBodyType();

        MeshFilter[] meshFilters = Selection.activeGameObject.GetComponentsInChildren<MeshFilter>();
        foreach (var meshFilter in meshFilters)
        {
            Mesh mesh = meshFilter.sharedMesh;
            if (currentBodyType == BodyType.Hi3P2)
            {
                MoveColors(mesh);
                meshFilter.sharedMesh = mesh;
            }
            else
            {

                ModifyMeshTangents(mesh);
                meshFilter.sharedMesh = mesh;
            }

        }

        SkinnedMeshRenderer[] skinMeshRenders = Selection.activeGameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (var skinMeshRender in skinMeshRenders)
        {
            Mesh mesh = skinMeshRender.sharedMesh;
            if (currentBodyType == BodyType.Hi3P2)
            {
                MoveColors(mesh);
                skinMeshRender.sharedMesh = mesh;
            }
            else
            {

                ModifyMeshTangents(mesh);
                skinMeshRender.sharedMesh = mesh;
            }
        }

        SaveMeshAssets(Selection.activeGameObject, currentBodyType);
    }

    private static Mesh ModifyMeshTangents(Mesh mesh)
    {
        Mesh newMesh = UnityEngine.Object.Instantiate(mesh);

        var vertices = newMesh.vertices;
        var triangles = newMesh.triangles;
        var unmerged = new Vector3[newMesh.vertexCount];
        var merged = new Dictionary<Vector3, Vector3>(); // Use a dictionary to map vertices to their merged normals
        var tangents = new Vector4[newMesh.vertexCount];

        for (int i = 0; i < triangles.Length; i += 3)
        {
            var i0 = triangles[i + 0];
            var i1 = triangles[i + 1];
            var i2 = triangles[i + 2];

            var v0 = vertices[i0] * 100;
            var v1 = vertices[i1] * 100;
            var v2 = vertices[i2] * 100;

            var normal_ = Vector3.Cross(v1 - v0, v2 - v0).normalized;

            unmerged[i0] += normal_ * Vector3.Angle(v1 - v0, v2 - v0);
            unmerged[i1] += normal_ * Vector3.Angle(v0 - v1, v2 - v1);
            unmerged[i2] += normal_ * Vector3.Angle(v0 - v2, v1 - v2);
        }

        for (int i = 0; i < vertices.Length; i++)
        {
            if (!merged.ContainsKey(vertices[i]))
            {
                merged[vertices[i]] = unmerged[i];
            }
            else
            {
                merged[vertices[i]] += unmerged[i];
            }
        }

        for (int i = 0; i < vertices.Length; i++)
        {
            var normal = merged[vertices[i]].normalized;
            tangents[i] = new Vector4(normal.x, normal.y, normal.z, 0);
        }

        newMesh.tangents = tangents;

        return newMesh;
    }

    private static Mesh MoveColors(Mesh mesh)
    {
        Mesh newMesh = UnityEngine.Object.Instantiate(mesh);

        var vertices = newMesh.vertices;
        var tangents = newMesh.tangents;
        var colors = newMesh.colors;

        // Initialize colors array if it's null or doesn't have the same length as vertices array
        if (colors == null || colors.Length != vertices.Length)
        {
            colors = new Color[vertices.Length];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = Color.white; // or any default color
            }
            newMesh.colors = colors;
        }

        for (int i = 0; i < vertices.Length; i++)
        {
            tangents[i].x = colors[i].r * 2 - 1;
            tangents[i].y = colors[i].g * 2 - 1;
            tangents[i].z = colors[i].b * 2 - 1;
        }
        newMesh.SetTangents(tangents);

        return newMesh;
    }

    private static void SaveMeshAssets(GameObject gameObject, BodyType currentBodyType)
    {
        MeshFilter[] meshFilters = gameObject.GetComponentsInChildren<MeshFilter>();
        SkinnedMeshRenderer[] skinMeshRenderers = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();

        foreach (var meshFilter in meshFilters)
        {
            Mesh mesh = meshFilter.sharedMesh;
            Mesh newMesh;
            if (currentBodyType == BodyType.Hi3P2)
            {
                newMesh = MoveColors(mesh);
            }
            else
            {
                newMesh = ModifyMeshTangents(mesh);
            }
            newMesh.name = mesh.name; // Set the name of the new mesh to the name of the original mesh
            meshFilter.sharedMesh = newMesh;

            string path = AssetDatabase.GetAssetPath(mesh);
            string folderPath = Path.GetDirectoryName(path) + "/Meshes";
            if (!Directory.Exists(folderPath))
            {
                AssetDatabase.CreateFolder(Path.GetDirectoryName(path), "Meshes");
            }
            path = folderPath + "/" + newMesh.name + ".asset";
            AssetDatabase.CreateAsset(newMesh, path);
        }

        foreach (var skinMeshRenderer in skinMeshRenderers)
        {
            Mesh mesh = skinMeshRenderer.sharedMesh;
            Mesh newMesh;
            if (currentBodyType == BodyType.Hi3P2)
            {
                newMesh = MoveColors(mesh);
            }
            else
            {
                newMesh = ModifyMeshTangents(mesh);
            }
            newMesh.name = mesh.name; // Set the name of the new mesh to the name of the original mesh
            skinMeshRenderer.sharedMesh = newMesh;

            string path = AssetDatabase.GetAssetPath(mesh);
            string folderPath = Path.GetDirectoryName(path) + "/Meshes";
            if (!Directory.Exists(folderPath))
            {
                AssetDatabase.CreateFolder(Path.GetDirectoryName(path), "Meshes");
            }
            path = folderPath + "/" + newMesh.name + ".asset";
            AssetDatabase.CreateAsset(newMesh, path);
        }
    }

    #endregion
}

