#if UNITY_EDITOR
using UnityEditor;

namespace Dacodelaac.Utils
{
    public class AssetImporterPostprocessor : AssetPostprocessor
    {
        void OnPreprocessModel()
        {
            if (assetImporter is ModelImporter modelImporter && modelImporter.importSettingsMissing)
            {
                modelImporter.materialImportMode = ModelImporterMaterialImportMode.None;
            }
        }

        void OnPreprocessTexture()
        {
            if (assetImporter is TextureImporter textureImporter)
            {
                textureImporter.mipmapEnabled = false;
                if (assetPath.Contains("_not_process_tex")) return;

                var defaultSettings = textureImporter.GetDefaultPlatformTextureSettings();
                
                var settings = textureImporter.GetPlatformTextureSettings("Android");
                //settings.overridden = false;
                settings.maxTextureSize = defaultSettings.maxTextureSize;
                settings.resizeAlgorithm = defaultSettings.resizeAlgorithm;
                settings.compressionQuality = defaultSettings.compressionQuality;
                settings.format = TextureImporterFormat.ASTC_6x6;
                textureImporter.SetPlatformTextureSettings(settings);
                
                settings = textureImporter.GetPlatformTextureSettings("iOS");
                //settings.overridden = false;
                settings.maxTextureSize = defaultSettings.maxTextureSize;
                settings.resizeAlgorithm = defaultSettings.resizeAlgorithm;
                settings.compressionQuality = defaultSettings.compressionQuality;
                settings.format = TextureImporterFormat.ASTC_6x6;
                textureImporter.SetPlatformTextureSettings(settings);
            }
        }
    }
}
#endif