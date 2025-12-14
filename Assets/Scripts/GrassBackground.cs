using UnityEngine;

/// <summary>
/// Generates a large static grass checkerboard grid (like the original CheckeredBackground)
/// </summary>
public class GrassBackground : MonoBehaviour
{
    [SerializeField] private int gridWidth = 100; // Large map
    [SerializeField] private int gridHeight = 100; // Large map
    [SerializeField] private float tileSize = 1f;
    [SerializeField] private Color grassColor1 = new Color(0.2f, 0.5f, 0.2f); // Dark green
    [SerializeField] private Color grassColor2 = new Color(0.25f, 0.55f, 0.25f); // Slightly lighter green
    
    private void Start()
    {
        GenerateGrassGrid();
    }
    
    private void GenerateGrassGrid()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                // Create a tile
                GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Quad);
                tile.name = $"GrassTile_{x}_{y}";
                tile.transform.parent = transform;
                
                // Position the tile (centered around 0,0)
                float posX = (x - gridWidth / 2f) * tileSize + tileSize / 2f;
                float posY = (y - gridHeight / 2f) * tileSize + tileSize / 2f;
                tile.transform.position = new Vector3(posX, posY, 1f); // z=1 behind sprites at z=0
                tile.transform.localScale = new Vector3(tileSize, tileSize, 1f);
                
                // Set the tile color based on checkerboard pattern
                Renderer renderer = tile.GetComponent<Renderer>();
                renderer.material = new Material(Shader.Find("Sprites/Default"));
                renderer.material.color = (x + y) % 2 == 0 ? grassColor1 : grassColor2;
                
                // Remove the collider
                Destroy(tile.GetComponent<Collider>());
            }
        }
    }
}
