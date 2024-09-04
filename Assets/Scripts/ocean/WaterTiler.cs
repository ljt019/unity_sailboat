using UnityEngine;
using System.Collections.Generic;

public class WaterTiler : MonoBehaviour
{
    public GameObject waterTilePrefab;
    public Transform playerTransform;
    public float tileSize = 100f;

    private Vector3 startPosition;
    private Queue<GameObject> tilePool = new Queue<GameObject>();
    private Dictionary<Vector2, GameObject> activeTiles = new Dictionary<Vector2, GameObject>();

    void Start()
    {
        if (waterTilePrefab == null || playerTransform == null)
        {
            Debug.LogError("WaterTiler: Required references are missing!");
            enabled = false;
            return;
        }

        startPosition = playerTransform.position;
        InitializeTilePool();
        CreateInitialTiles();
        Debug.Log($"WaterTiler initialized. Start position: {startPosition}, Tile size: {tileSize}");
    }

    void Update()
    {
        if (playerTransform != null)
        {
            UpdateTiles();
        }
    }

    void InitializeTilePool()
    {
        for (int i = 0; i < 9; i++) // Increase pool size to 9 (3x3 grid)
        {
            GameObject tile = Instantiate(waterTilePrefab, transform);
            tile.SetActive(false);
            tilePool.Enqueue(tile);
        }
        Debug.Log($"Tile pool initialized with {tilePool.Count} tiles");
    }

    void CreateInitialTiles()
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                Vector3 position = new Vector3(
                    startPosition.x + x * tileSize,
                    0, // Set y to 0 or your desired water level
                    startPosition.z + z * tileSize);

                ActivateTile(position);
            }
        }
        Debug.Log($"Initial tiles created. Active tiles: {activeTiles.Count}");
    }

    void UpdateTiles()
    {
        Vector3 playerPosition = playerTransform.position;
        Vector2 currentTile = new Vector2(
            Mathf.Round(playerPosition.x / tileSize) * tileSize,
            Mathf.Round(playerPosition.z / tileSize) * tileSize);

        List<Vector2> neededTiles = new List<Vector2>();
        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                neededTiles.Add(new Vector2(
                    currentTile.x + x * tileSize,
                    currentTile.y + z * tileSize));
            }
        }

        foreach (Vector2 tilePos in new List<Vector2>(activeTiles.Keys))
        {
            if (!neededTiles.Contains(tilePos))
            {
                DeactivateTile(tilePos);
            }
        }

        foreach (Vector2 tilePos in neededTiles)
        {
            if (!activeTiles.ContainsKey(tilePos))
            {
                ActivateTile(new Vector3(tilePos.x, 0, tilePos.y));
            }
        }

        Debug.Log($"Tiles updated. Active tiles: {activeTiles.Count}");
        Debug.Log($"Player position: {playerPosition}, Current tile: {currentTile}");
        Debug.Log($"Needed tiles: {string.Join(", ", neededTiles)}");
        Debug.Log($"Active tiles: {string.Join(", ", activeTiles.Keys)}");
        // Log tile positions 
        foreach (Vector2 tilePos in activeTiles.Keys)
        {
            Debug.Log($"Active tile: {tilePos}");
        }
    }

    void ActivateTile(Vector3 position)
    {
        Vector2 tileKey = new Vector2(position.x, position.z);
        if (!activeTiles.ContainsKey(tileKey))
        {
            GameObject tile = GetPooledTile();
            if (tile != null)
            {
                tile.transform.position = position;
                tile.SetActive(true);
                activeTiles[tileKey] = tile;
                if (OceanController.Instance != null)
                {
                    OceanController.Instance.ApplyOceanToTile(tile);
                }
                else
                {
                    Debug.LogWarning("OceanController.Instance is null. Cannot apply ocean to tile.");
                }
                Debug.Log($"Tile activated at position: {position}");
            }
            else
            {
                Debug.LogWarning("WaterTiler: Failed to get a pooled tile.");
            }
        }
    }

    void DeactivateTile(Vector2 tileKey)
    {
        if (activeTiles.TryGetValue(tileKey, out GameObject tile))
        {
            if (tile != null)
            {
                tile.SetActive(false);
                tilePool.Enqueue(tile);
            }
            activeTiles.Remove(tileKey);
            Debug.Log($"Tile deactivated at position: {tileKey}");
        }
    }

    GameObject GetPooledTile()
    {
        if (tilePool.Count > 0)
        {
            return tilePool.Dequeue();
        }
        else if (waterTilePrefab != null)
        {
            Debug.Log("Creating new tile as pool is empty");
            return Instantiate(waterTilePrefab, transform);
        }
        else
        {
            Debug.LogError("WaterTiler: Water tile prefab is missing!");
            return null;
        }
    }

    public void UpdateAllTiles()
    {
        if (OceanController.Instance == null)
        {
            Debug.LogWarning("OceanController.Instance is null. Cannot update tiles.");
            return;
        }

        foreach (GameObject tile in activeTiles.Values)
        {
            OceanController.Instance.ApplyOceanToTile(tile);
        }
        Debug.Log($"All tiles updated. Active tiles: {activeTiles.Count}");
    }
}