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
            renderer.material = mat;
        }
    }
}