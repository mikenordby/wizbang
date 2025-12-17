using UnityEditor;
using UnityEngine;

public class FixWizardSprites
{
    [MenuItem("Tools/Fix Wizard Sprite Imports")]
    public static void FixImports()
    {
        string[] paths = new string[] {
            "Assets/Resources/Sprites/PlayerWizard/south.png",
            "Assets/Resources/Sprites/PlayerWizard/south_west.png",
            "Assets/Resources/Sprites/PlayerWizard/west.png",
            "Assets/Resources/Sprites/PlayerWizard/north_west.png",
            "Assets/Resources/Sprites/PlayerWizard/north.png",
            "Assets/Resources/Sprites/PlayerWizard/north_east.png",
            "Assets/Resources/Sprites/PlayerWizard/east.png",
            "Assets/Resources/Sprites/PlayerWizard/south_east.png"
        };
        
        foreach (string path in paths)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 32;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                Debug.Log($"Fixed import: {path}");
            }
        }
        
        AssetDatabase.Refresh();
        Debug.Log("All wizard sprites reimported with correct settings!");
    }
}
