using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the grid of tiles, including path generation, tile types, and decorations.
/// This class is a singleton, ensuring only one instance exists in the scene.
/// </summary>
public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; } // Singleton instance
    public int width = 10;
    public int height = 10;
    public GameObject tilePrefab;
    public Material pathMaterial;
    public Material buildableMaterial;
    public Material blockedMaterial; // assign in Inspector
    public List<Vector3> pathWorldPositions = new();
    public GameObject[] decorativePrefabs; // drag trees, rocks, etc. in inspector
    private Tile[,] grid;
    private Transform decorationParent;

    /// <summary>World-space integer position → grid coordinate.</summary>
    private readonly Dictionary<Vector3Int, Vector2Int> worldToGrid = new();

    public string buildableLayerName = "Buildable"; // set in inspector or hardcoded
    private int buildableLayer;
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // this will make decorations easier to manage
        decorationParent = transform.GetChild(0); // this will always get the decoration parent if it exists
        if (decorationParent == null || decorationParent.name != "DecorationParent")
        {
            decorationParent = new GameObject("DecorationParent").transform;
            decorationParent.SetParent(transform); // nesting it under the GridManager
        }

        buildableLayer = LayerMask.NameToLayer(buildableLayerName);
        if (buildableLayer == -1)
        {
            Debug.LogError("Buildable layer not found! Please create a layer named 'Buildable'.");
        }

        GenerateGrid();
        GenerateRandomPath(new Vector2Int(0, 0), new Vector2Int(width - 1, height - 1));
    }

    // Physical grid of tiles, each with a visual representation.
    void GenerateGrid()
    {
        grid = new Tile[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector3 position = new Vector3(x, 0, z);
                GameObject tileObj = Instantiate(tilePrefab, position, Quaternion.identity, transform);
                tileObj.name = $"Tile_{x}_{z}"; // Naming conventions for easier debugging

                grid[x, z] = new Tile(x, z, tileObj);
                Vector3Int key = new Vector3Int(x, 0, z);
                worldToGrid[key] = new Vector2Int(x, z);
            }
        }
    }

    public void GenerateNewPath()
    {
        pathWorldPositions.Clear();
        Vector2Int start = new Vector2Int(0, 0);
        Vector2Int end = new Vector2Int(width - 1, height - 1);

        // Clear previous path tiles
        foreach (Tile tile in grid)
        {
            if (tile.type == TileType.Path)
            {
                tile.type = TileType.Empty;
            }
        }
        worldToGrid.Clear();
        GenerateRandomPath(start, end); // regenerate a new random path
    }


    // Generates a random path from start to end, ensuring it is walkable.
    // This method uses a simple random walk algorithm to create a path.
    // All enemies will follow this path.
    // But also all towers will be placed around this path. Within 1 tile radius.
    void GenerateRandomPath(Vector2Int start, Vector2Int end)
    {
        Vector2Int current = start;
        grid[current.x, current.y].type = TileType.Path;
        pathWorldPositions.Add(grid[current.x, current.y].visual.transform.position);

        while (current != end)
        {
            List<Vector2Int> directions = new();

            if (current.x < end.x) directions.Add(Vector2Int.right);
            if (current.x > end.x) directions.Add(Vector2Int.left);
            if (current.y < end.y) directions.Add(Vector2Int.up);
            if (current.y > end.y) directions.Add(Vector2Int.down);

            if (directions.Count == 0) break;

            Vector2Int move = directions[Random.Range(0, directions.Count)];
            current += move;

            grid[current.x, current.y].type = TileType.Path;
            pathWorldPositions.Add(grid[current.x, current.y].visual.transform.position);
        }
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                if (grid[x, z].type != TileType.Path)
                {
                    grid[x, z].type = TileType.Blocked; // not placeable by default
                }
            }
        }

        // Then, mark tiles near the path (1-tile radius) as buildable
        foreach (Vector3 pathWorld in pathWorldPositions)
        {
            Vector2Int pathPos = new Vector2Int((int)pathWorld.x, (int)pathWorld.z);

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dz = -1; dz <= 1; dz++)
                {
                    int nx = pathPos.x + dx;
                    int nz = pathPos.y + dz;

                    if (nx >= 0 && nx < width && nz >= 0 && nz < height)
                    {
                        if (grid[nx, nz].type != TileType.Path)
                        {
                            grid[nx, nz].type = TileType.Empty; // buildable
                        }
                    }
                }
            }
        }
        RefreshTileVisuals(); // this makes the path tiles visible after every Random path generation
    }

    public bool TryGetTileCoordinate(Vector3 worldPos, out Vector2Int coord)
    {
        // Snap to the nearest integer cell centre.  
        Vector3Int key = new Vector3Int(
            Mathf.RoundToInt(worldPos.x),
            0,
            Mathf.RoundToInt(worldPos.z));

        return worldToGrid.TryGetValue(key, out coord);
    }

    // Convenience wrapper if you want the Tile object itself (or null if none).
    public Tile GetTileAtWorldPosition(Vector3 worldPos)
    {
        return TryGetTileCoordinate(worldPos, out var c) ? grid[c.x, c.y] : null;
    }

    void RefreshTileVisuals()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Tile tile = grid[x, z];

                Material mat = buildableMaterial;

                if (tile.type == TileType.Path)
                {
                    mat = pathMaterial;
                }
                else if (tile.type == TileType.Blocked)
                {
                    mat = blockedMaterial;
                }

                tile.UpdateVisual(mat, tile.type);

                // This is where we instantiate decorations on blocked tiles (trees, rocks, etc.)
                if (tile.type == TileType.Blocked && decorativePrefabs.Length > 0 && Random.value < 0.3f)
                {
                    Vector3 pos = tile.visual.transform.position;
                    pos.y = 0.5f; // Slightly above ground to avoid clipping
                    GameObject deco = Instantiate(
                        decorativePrefabs[Random.Range(0, decorativePrefabs.Length)],
                        pos,
                        Quaternion.identity,
                        decorationParent
                    );
                }
            }
        }
    }

    public void ResetBoard()
    {
        // 1. Clear all decorations
        if (decorationParent != null)
        {
            foreach (Transform child in decorationParent)
            {
                Destroy(child.gameObject);
            }
        }

        // 2. Clear tile visuals if necessary (optional)
        foreach (Tile tile in grid)
        {
            if (tile.visual != null)
            {
                Destroy(tile.visual);
            }
        }

        // 3. Clear grid, regenerate
        worldToGrid.Clear();
        GenerateGrid();
        GenerateNewPath();
    }

    public Tile GetTile(int x, int z) => grid[x, z];
}
