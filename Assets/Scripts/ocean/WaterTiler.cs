using UnityEngine;
using System.Collections.Generic;

public class WaterTiler : MonoBehaviour
{
    public GameObject waterTilePrefab;
    public Transform playerTransform;
    public float tileSize = 100f;

    private Vector3 startPosition;
    private Queue<GameObject> tilePool = new Queue<GameObject>();
    private Dictionary<Vector3, GameObject> activeTiles = new Dictionary<Vector3, GameObject>();

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
        for (int i = 0; i < 4; i++)
        {
            GameObject tile = Instantiate(waterTilePrefab, transform);
            tile.SetActive(false);
            tilePool.Enqueue(tile);
        }
    }

    void CreateInitialTiles()
    {
        for (int x = -1; x <= 0; x++)
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
        Vector3 playerPosition = playerTransform.position;
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
    }

    void MoveTiles(Vector3 moveDirection)
    {
        List<Vector3> tilesToRemove = new List<Vector3>();
        List<Vector3> tilesToAdd = new List<Vector3>();

        foreach (Vector3 position in activeTiles.Keys)
        {
            Vector3 newPosition = position - moveDirection;
            if (Vector3.Distance(playerTransform.position, newPosition) <= tileSize * Mathf.Sqrt(2))
            {
                tilesToAdd.Add(newPosition);
            }
            else
            {
                tilesToRemove.Add(position);
            }
        }

        foreach (Vector3 position in tilesToRemove)
        {
            DeactivateTile(position);
        }

        foreach (Vector3 position in tilesToAdd)
        {
            ActivateTile(position);
        }
    }

    void ActivateTile(Vector3 position)
    {
        if (!activeTiles.ContainsKey(position))
        {
            GameObject tile = GetPooledTile();
            if (tile != null)
            {
                tile.transform.position = position;
                tile.SetActive(true);
                activeTiles[position] = tile;
                OceanController.Instance.ApplyOceanToTile(tile);
            }
            else
            {
                Debug.LogWarning("WaterTiler: Failed to get a pooled tile.");
            }
        }
    }

    void DeactivateTile(Vector3 position)
    {
        if (activeTiles.TryGetValue(position, out GameObject tile))
        {
            if (tile != null)
            {
                tile.SetActive(false);
                tilePool.Enqueue(tile);
            }
            activeTiles.Remove(position);
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
        foreach (GameObject tile in activeTiles.Values)
        {
            OceanController.Instance.ApplyOceanToTile(tile);
        }
    }
}