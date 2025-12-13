using UnityEngine;

public class CheckeredBackground : MonoBehaviour
{
    [SerializeField] private int gridWidth = 20;
    [SerializeField] private int gridHeight = 20;
    [SerializeField] private float tileSize = 1f;
    [SerializeField] private Color color1 = Color.white;
    [SerializeField] private Color color2 = Color.gray;
    
    private void Start()
    {
        GenerateCheckeredBackground();
    }
    
    private void GenerateCheckeredBackground()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                // Create a tile
                GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Quad);
                tile.name = $"Tile_{x}_{y}";
                tile.transform.parent = transform;
                
                // Position the tile (centered around 0,0)
                float posX = (x - gridWidth / 2f) * tileSize + tileSize / 2f;
                float posY = (y - gridHeight / 2f) * tileSize + tileSize / 2f;
                tile.transform.position = new Vector3(posX, posY, 1f); // z=1 to put behind player
                tile.transform.localScale = new Vector3(tileSize, tileSize, 1f);
                
                // Set the tile color based on checkerboard pattern
                Renderer renderer = tile.GetComponent<Renderer>();
                renderer.material = new Material(Shader.Find("Sprites/Default"));
                renderer.material.color = (x + y) % 2 == 0 ? color1 : color2;
                
                // Remove the collider that comes with primitives
                Destroy(tile.GetComponent<Collider>());
            }
        }
    }
}