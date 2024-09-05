using UnityEngine;
using System.Collections.Generic;

public class TileController : MonoBehaviour
{
    [Header("Entity References")]
    [SerializeField] public GameObject waterTilePrefab;
    [SerializeField] public Transform boatTransform;
    [SerializeField] public OceanAdvanced oceanAdvanced;

    private Vector2Int currentGridPosition;
    private Dictionary<Vector2Int, GameObject> tiles = new Dictionary<Vector2Int, GameObject>();
    private float tileSize;

    void Start()
    {
        if (oceanAdvanced == null)
        {
            Debug.LogError("OceanAdvanced reference is missing!");
            return;
        }

        CalculateTileSize();
        currentGridPosition = WorldToGridPosition(boatTransform.position);
        CreateInitialTiles();
    }

    void CalculateTileSize()
    {
        Renderer prefabRenderer = waterTilePrefab.GetComponent<Renderer>();
        if (prefabRenderer != null)
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

    void Update()
    {
        Vector2Int newGridPosition = WorldToGridPosition(boatTransform.position);
        if (newGridPosition != currentGridPosition)
        {
            UpdateTilePositions(newGridPosition);
            currentGridPosition = newGridPosition;
        }

        // Update ocean position
        oceanAdvanced.transform.position = new Vector3(boatTransform.position.x, oceanAdvanced.transform.position.y, boatTransform.position.z);
    }

    void CreateInitialTiles()
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                Vector2Int gridPos = new Vector2Int(x, z);
                CreateTileAt(currentGridPosition + gridPos);
            }
        }
    }

    void UpdateTilePositions(Vector2Int newCenterGridPos)
    {
        HashSet<Vector2Int> newPositions = new HashSet<Vector2Int>();
        HashSet<Vector2Int> oldPositions = new HashSet<Vector2Int>(tiles.Keys);

        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                Vector2Int gridPos = newCenterGridPos + new Vector2Int(x, z);
                newPositions.Add(gridPos);

                if (!tiles.ContainsKey(gridPos))
                {
                    // If there's no tile at this position, move an old tile here
                    Vector2Int oldPos = FindFurthestTile(newCenterGridPos, oldPositions);
                    if (oldPos != gridPos)
                    {
                        MoveTile(oldPos, gridPos);
                        oldPositions.Remove(oldPos);
                    }
                }
            }
        }

        // Ensure we always have exactly 9 tiles
        Debug.Assert(tiles.Count == 9, "There should always be exactly 9 tiles.");
    }

    Vector2Int FindFurthestTile(Vector2Int center, HashSet<Vector2Int> availablePositions)
    {
        Vector2Int furthest = center;
        float maxDistanceSq = float.MinValue;

        foreach (Vector2Int pos in availablePositions)
        {
            float distanceSq = (pos - center).sqrMagnitude;
            if (distanceSq > maxDistanceSq)
            {
                maxDistanceSq = distanceSq;
                furthest = pos;
            }
        }

        return furthest;
    }


    void CreateTileAt(Vector2Int gridPos)
    {
        Vector3 worldPos = GridToWorldPosition(gridPos);
        GameObject tile = Instantiate(waterTilePrefab, worldPos, Quaternion.identity, transform);
        ApplyOceanToTile(tile);
        tiles[gridPos] = tile;
    }

    void MoveTile(Vector2Int oldGridPos, Vector2Int newGridPos)
    {
        if (tiles.TryGetValue(oldGridPos, out GameObject tile))
        {
            tile.transform.position = GridToWorldPosition(newGridPos);
            tiles.Remove(oldGridPos);
            tiles[newGridPos] = tile;
        }
        else
        {
            Debug.LogError($"Tried to move non-existent tile from {oldGridPos} to {newGridPos}");
        }
    }

    void ApplyOceanToTile(GameObject tile)
    {
        Renderer tileRenderer = tile.GetComponent<Renderer>();
        if (tileRenderer != null && oceanAdvanced.ocean != null)
        {
            tileRenderer.material = oceanAdvanced.ocean;
        }
    }

    Vector2Int WorldToGridPosition(Vector3 worldPos)
    {
        return new Vector2Int(
            Mathf.RoundToInt(worldPos.x / tileSize),
            Mathf.RoundToInt(worldPos.z / tileSize)
        );
    }

    Vector3 GridToWorldPosition(Vector2Int gridPos)
    {
        return new Vector3(
            gridPos.x * tileSize,
            0,
            gridPos.y * tileSize
        );
    }
}