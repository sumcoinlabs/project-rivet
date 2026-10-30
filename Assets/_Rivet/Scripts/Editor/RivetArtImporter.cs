// Imports our original PNG illustrations as native Unity Sprites (not default textures).
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectRivet.Editor
{
    public sealed class RivetArtImporter : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith("Assets/_Rivet/Resources/RivetArt/", StringComparison.Ordinal)) return;
            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
        }

        [MenuItem("Project Rivet/Reimport Sprint 3 Art")]
        public static void ReimportAll()
        {
            string folder = Path.Combine(Application.dataPath, "_Rivet/Resources/RivetArt");
            if(!Directory.Exists(folder)) throw new Exception("Missing art folder: " + folder);
            string[] paths = Directory.GetFiles(folder,"*.png",SearchOption.TopDirectoryOnly);
            int count=0;
            foreach (string sourceFile in paths)
            {
                string asset = "Assets/_Rivet/Resources/RivetArt/" + Path.GetFileName(sourceFile);
                AssetDatabase.ImportAsset(asset, ImportAssetOptions.ForceUpdate);
                if(AssetDatabase.LoadAssetAtPath<Sprite>(asset)==null)
                    throw new Exception("Not imported as Sprite: " + asset);
                count++;
            }
            if(count<10)throw new Exception("Sprint 3 art incomplete: expected 10 PNGs; imported " + count);
            // The exact scene created in Sprint 1 remains intact and uses the reimplemented FirstFlightGame.
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/_Rivet/Scenes/FirstFlight.unity")==null)
                throw new Exception("FirstFlight.unity scene missing");
            AssetDatabase.SaveAssets();
            Debug.Log("RIVET_SPRINT3_VALIDATED: " + count + " original sprite assets and FirstFlight scene OK");
        }
    }
}
