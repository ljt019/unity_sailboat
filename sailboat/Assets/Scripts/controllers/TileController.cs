using UnityEngine;
using System.Collections.Generic;

public class TileController : MonoBehaviour
{
    [Header("Entity References")]
    [SerializeField] private GameObject waterTilePrefab;
    [SerializeField] private Transform boatTransform;
    [SerializeField] private OceanAdvanced oceanAdvanced;

    private Vector2Int currentGridPosition;
    private readonly Dictionary<Vector2Int, GameObject> tiles = new Dictionary<Vector2Int, GameObject>();
    private float tileSize;
    private Material oceanMaterial;

    void Start()
    {
        ValidateReferences();
        CalculateTileSize();
        currentGridPosition = WorldToGridPosition(boatTransform.position);
        CacheOceanMaterial();
        CreateInitialTiles();
    }

    /// <summary>
    /// Validates essential references to prevent runtime errors.
    /// </summary>
    private void ValidateReferences()
    {
        bool hasError = false;

        if (oceanAdvanced == null)
        {
            Debug.LogError("OceanAdvanced reference is missing!");
            hasError = true;
        }

        if (waterTilePrefab == null)
        {
            Debug.LogError("WaterTilePrefab is not assigned!");
            hasError = true;
        }

        if (boatTransform == null)
        {
            Debug.LogError("BoatTransform is not assigned!");
            hasError = true;
        }

        if (hasError)
        {
            // Disable the script to prevent further errors
            this.enabled = false;
        }
    }

    /// <summary>
    /// Calculates the size of a tile based on the prefab's renderer.
    /// </summary>
    private void CalculateTileSize()
    {
        if (waterTilePrefab.TryGetComponent<Renderer>(out Renderer prefabRenderer))
        {
            tileSize = prefabRenderer.bounds.size.x;
            Debug.Log($"Calculated tile size: {tileSize}");
        }
        else
        {
            Debug.LogError("Water tile prefab is missing a Renderer component!");
            tileSize = 100f; // Fallback size
        }
    }

    /// <summary>
    /// Caches the ocean material for efficient reuse.
    /// </summary>
    private void CacheOceanMaterial()
    {
        if (oceanAdvanced?.ocean != null)
        {
            oceanMaterial = oceanAdvanced.ocean;
        }
        else
        {
            Debug.LogError("OceanAdvanced.ocean material is missing!");
        }
    }

    void Update()
    {
        Vector2Int newGridPosition = WorldToGridPosition(boatTransform.position);
        if (newGridPosition != currentGridPosition)
        {
            Vector2Int delta = newGridPosition - currentGridPosition;
            UpdateTilePositions(newGridPosition, delta);
            currentGridPosition = newGridPosition;
        }

        UpdateOceanPosition();
    }

    /// <summary>
    /// Updates the ocean's position to follow the boat.
    /// </summary>
    private void UpdateOceanPosition()
    {
        Vector3 boatPos = boatTransform.position;
        Vector3 oceanPos = oceanAdvanced.transform.position;
        oceanAdvanced.transform.position = new Vector3(boatPos.x, oceanPos.y, boatPos.z);
    }

    /// <summary>
    /// Creates the initial 3x3 grid of water tiles around the boat.
    /// </summary>
    private void CreateInitialTiles()
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                Vector2Int gridOffset = new Vector2Int(x, z);
                Vector2Int gridPos = currentGridPosition + gridOffset;
                CreateTileAt(gridPos);
            }
        }
    }

    /// <summary>
    /// Updates tile positions based on the movement delta.
    /// </summary>
    /// <param name="newCenterGridPos">New center grid position.</param>
    /// <param name="delta">Change in grid position.</param>
    private void UpdateTilePositions(Vector2Int newCenterGridPos, Vector2Int delta)
    {
        // Shift tiles horizontally based on delta.x
        if (delta.x != 0)
        {
            int directionX = (int)Mathf.Sign(delta.x);
            for (int i = 0; i < Mathf.Abs(delta.x); i++)
            {
                ShiftTilesAlongX(directionX);
            }
        }

        // Shift tiles vertically based on delta.y
        if (delta.y != 0)
        {
            int directionY = (int)Mathf.Sign(delta.y);
            for (int i = 0; i < Mathf.Abs(delta.y); i++)
            {
                ShiftTilesAlongY(directionY);
            }
        }

        // Ensure we always have exactly 9 tiles
        if (tiles.Count != 9)
        {
            Debug.LogWarning($"Tile count mismatch: Expected 9, Found {tiles.Count}");
        }
    }

    /// <summary>
    /// Shifts tiles along the X-axis.
    /// </summary>
    /// <param name="direction">1 for right, -1 for left.</param>
    private void ShiftTilesAlongX(int direction)
    {
        List<Vector2Int> tilesToMove = new List<Vector2Int>();

        // Identify tiles on the opposite edge to move
        foreach (var pos in tiles.Keys)
        {
            if ((direction > 0 && pos.x == currentGridPosition.x - 1) ||
                (direction < 0 && pos.x == currentGridPosition.x + 1))
            {
                tilesToMove.Add(pos);
            }
        }

        // Move identified tiles to the new edge
        foreach (var oldPos in tilesToMove)
        {
            Vector2Int newPos = new Vector2Int(oldPos.x + (direction * 2), oldPos.y);
            MoveTile(oldPos, newPos);
        }
    }

    /// <summary>
    /// Shifts tiles along the Y-axis.
    /// </summary>
    /// <param name="direction">1 for forward, -1 for backward.</param>
    private void ShiftTilesAlongY(int direction)
    {
        List<Vector2Int> tilesToMove = new List<Vector2Int>();

        // Identify tiles on the opposite edge to move
        foreach (var pos in tiles.Keys)
        {
            if ((direction > 0 && pos.y == currentGridPosition.y - 1) ||
                (direction < 0 && pos.y == currentGridPosition.y + 1))
            {
                tilesToMove.Add(pos);
            }
        }

        // Move identified tiles to the new edge
        foreach (var oldPos in tilesToMove)
        {
            Vector2Int newPos = new Vector2Int(oldPos.x, oldPos.y + (direction * 2));
            MoveTile(oldPos, newPos);
        }
    }

    /// <summary>
    /// Creates a tile at the specified grid position.
    /// </summary>
    /// <param name="gridPos">Grid position to place the tile.</param>
    private void CreateTileAt(Vector2Int gridPos)
    {
        Vector3 worldPos = GridToWorldPosition(gridPos);
        GameObject tile = Instantiate(waterTilePrefab, worldPos, Quaternion.identity, transform);
        ApplyOceanToTile(tile);
        tiles[gridPos] = tile;
    }

    /// <summary>
    /// Moves a tile from an old grid position to a new grid position.
    /// </summary>
    /// <param name="oldGridPos">Current grid position of the tile.</param>
    /// <param name="newGridPos">New grid position to move the tile to.</param>
    private void MoveTile(Vector2Int oldGridPos, Vector2Int newGridPos)
    {
        if (tiles.TryGetValue(oldGridPos, out GameObject tile))
        {
            tile.transform.position = GridToWorldPosition(newGridPos);
            tiles.Remove(oldGridPos);
            tiles[newGridPos] = tile;
        }
        else
        {
            Debug.LogError($"Attempted to move non-existent tile from {oldGridPos} to {newGridPos}");
        }
    }

    /// <summary>
    /// Applies the ocean material to a tile.
    /// </summary>
    /// <param name="tile">Tile GameObject.</param>
    private void ApplyOceanToTile(GameObject tile)
    {
        if (tile.TryGetComponent<Renderer>(out Renderer tileRenderer) && oceanMaterial != null)
        {
            tileRenderer.material = oceanMaterial;
        }
        else
        {
            Debug.LogError("Tile Renderer or Ocean Material is missing!");
        }
    }

    /// <summary>
    /// Converts world position to grid position.
    /// </summary>
    /// <param name="worldPos">World position.</param>
    /// <returns>Grid position as Vector2Int.</returns>
    private Vector2Int WorldToGridPosition(Vector3 worldPos)
    {
        return new Vector2Int(
            Mathf.RoundToInt(worldPos.x / tileSize),
            Mathf.RoundToInt(worldPos.z / tileSize)
        );
    }

    /// <summary>
    /// Converts grid position to world position.
    /// </summary>
    /// <param name="gridPos">Grid position as Vector2Int.</param>
    /// <returns>World position as Vector3.</returns>
    private Vector3 GridToWorldPosition(Vector2Int gridPos)
    {
        return new Vector3(
            gridPos.x * tileSize,
            0,
            gridPos.y * tileSize
        );
    }

    /// <summary>
    /// Helper method to get the sign of an integer as an integer.
    /// </summary>
    /// <param name="value">Input integer.</param>
    /// <returns>-1, 0, or 1 based on the sign of the input.</returns>
    private int GetSign(int value)
    {
        if (value > 0) return 1;
        if (value < 0) return -1;
        return 0;
    }
}
