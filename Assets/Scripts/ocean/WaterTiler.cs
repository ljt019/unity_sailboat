using UnityEngine;
using System.Collections.Generic;

public class WaterTiler : MonoBehaviour
{
    public GameObject waterTilePrefab; // Reference to the water tile prefab
    public Transform boatTransform; // Reference to the player's transform
    public float tileSize = 100f; // Size of each tile (ensure this matches your plane's scale)
    public OceanAdvanced oceanAdvanced; // Reference to the OceanAdvanced script

    private Vector3 startPosition;
    private Queue<GameObject> tilePool = new Queue<GameObject>();
    private HashSet<Vector3> activeTiles = new HashSet<Vector3>();

    void Start()
    {
        if (oceanAdvanced == null)
        {
            Debug.LogError("OceanAdvanced reference is missing!");
            return;
        }

        startPosition = boatTransform.position;
        InitializeTilePool();
        CreateInitialTiles();
    }

    void Update()
    {
        UpdateTiles();
    }

    void InitializeTilePool()
    {
        for (int i = 0; i < 4; i++) // 2x2 grid requires 4 tiles
        {
            GameObject tile = Instantiate(waterTilePrefab);
            tile.SetActive(false);
            tilePool.Enqueue(tile);
        }
    }

    void CreateInitialTiles()
    {
        for (int x = -1; x <= 0; x++) // Adjusted for 2x2 grid
        {
            for (int z = -1; z <= 0; z++)
            {
                Vector3 position = new Vector3(
                    startPosition.x + x * tileSize,
                    startPosition.y,
                    startPosition.z + z * tileSize);

                ActivateTile(position);
            }
        }
    }

    void UpdateTiles()
    {
        Vector3 playerPosition = boatTransform.position;
        Vector3 offset = playerPosition - startPosition;

        if (Mathf.Abs(offset.x) >= tileSize || Mathf.Abs(offset.z) >= tileSize)
        {
            Vector3 moveDirection = new Vector3(
                Mathf.Round(offset.x / tileSize) * tileSize,
                0,
                Mathf.Round(offset.z / tileSize) * tileSize);

            startPosition += moveDirection;
            MoveTiles(moveDirection);
        }

        // Update the ocean's position to follow the boat
        oceanAdvanced.transform.position = new Vector3(playerPosition.x, oceanAdvanced.transform.position.y, playerPosition.z);
    }

    void MoveTiles(Vector3 moveDirection)
    {
        List<Vector3> newTiles = new List<Vector3>();

        foreach (Vector3 position in activeTiles)
        {
            Vector3 newPosition = position - moveDirection;
            if (Vector3.Distance(boatTransform.position, newPosition) <= tileSize * Mathf.Sqrt(2))
            {
                newTiles.Add(newPosition);
            }
            else
            {
                DeactivateTile(position);
            }
        }

        foreach (Vector3 position in newTiles)
        {
            ActivateTile(position);
        }
    }

    void ActivateTile(Vector3 position)
    {
        if (!activeTiles.Contains(position))
        {
            GameObject tile = GetPooledTile();
            tile.transform.position = position;
            tile.SetActive(true);
            activeTiles.Add(position);

            // Apply ocean material and properties to the new tile
            ApplyOceanToTile(tile);
        }
    }

    void DeactivateTile(Vector3 position)
    {
        if (activeTiles.Contains(position))
        {
            GameObject tile = GetTileAtPosition(position);
            tile.SetActive(false);
            activeTiles.Remove(position);
            tilePool.Enqueue(tile);
        }
    }

    GameObject GetPooledTile()
    {
        if (tilePool.Count > 0)
        {
            return tilePool.Dequeue();
        }
        else
        {
            return Instantiate(waterTilePrefab);
        }
    }

    GameObject GetTileAtPosition(Vector3 position)
    {
        foreach (Transform child in transform)
        {
            if (Vector3.Distance(child.position, position) < 0.1f)
            {
                return child.gameObject;
            }
        }
        return null;
    }

    void ApplyOceanToTile(GameObject tile)
    {
        // Apply the ocean material to the tile
        Renderer tileRenderer = tile.GetComponent<Renderer>();
        if (tileRenderer != null && oceanAdvanced.ocean != null)
        {
            tileRenderer.material = oceanAdvanced.ocean;
        }

        // You might need to add additional components or scripts to the tile
        // to make it work with the OceanAdvanced system
    }
}