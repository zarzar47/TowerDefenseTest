using UnityEngine;
using UnityEngine.SceneManagement;

public class Tile
{
    public Vector2Int gridPos;
    public TileType type;
    public bool isOccupied;
    public GameObject visual;
    
    public Tile(int x, int z, GameObject visualObj)
    {
        gridPos = new Vector2Int(x, z);
        type = TileType.Empty;
        isOccupied = false;
        visual = visualObj;
    }

    public void UpdateVisual(Material mat, TileType tileType = TileType.Empty)
    {
        if (visual.TryGetComponent<Renderer>(out Renderer renderer))
        {
            if (tileType == TileType.Path)
            {

                visual.layer = 7; // Default layer for path tiles
            }
            else if (tileType == TileType.Blocked)
            {
                visual.layer = 8; // Default layer for blocked tiles
            }
            else
            {
                visual.layer = 6; // Buildable layer
            }
            renderer.material = mat;
        }
    }
}