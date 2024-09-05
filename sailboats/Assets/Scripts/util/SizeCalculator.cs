using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SizeCalculator : MonoBehaviour
{
    public GameObject waterTilePrefab;
    // Start is called before the first frame update
    void Start()
    {
        MeshRenderer meshRenderer = waterTilePrefab.GetComponent<MeshRenderer>();
        Vector3 tileSize = meshRenderer.bounds.size;
        Debug.Log("Tile Size: " + tileSize);

    }

    // Update is called once per frame
    void Update()
    {

    }
}
