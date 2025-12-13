using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CreatePlayerSprite : MonoBehaviour
{
    #if UNITY_EDITOR
    [MenuItem("Tools/Create Player Sprite")]
    public static void CreateSprite()
    {
        // Create a simple white texture
        Texture2D texture = new Texture2D(64, 64);
        Color[] pixels = new Color[64 * 64];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white;
        }
        texture.SetPixels(pixels);
        texture.Apply();
        
        // Save the texture as a PNG
        byte[] bytes = texture.EncodeToPNG();
        string path = "Assets/Sprites/PlayerSquare.png";
        System.IO.File.WriteAllBytes(path, bytes);
        
        AssetDatabase.Refresh();
        
        // Get the texture importer and set it up for sprites
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 64;
            importer.filterMode = FilterMode.Point;
            importer.SaveAndReimport();
        }
        
        Debug.Log("Player sprite created at: " + path);
    }
    #endif
}