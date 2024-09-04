using UnityEngine;
using System.Collections.Generic;

public class OceanController : MonoBehaviour
{
    public static OceanController Instance { get; private set; }

    [SerializeField] private OceanAdvanced oceanAdvanced;
    [SerializeField] private WaterTiler waterTiler;
    [SerializeField] private Transform playerTransform;

    [Header("Ocean State")]
    [SerializeField] private float calmWaveHeight = 0.1f;
    [SerializeField] private float stormyWaveHeight = 1.0f;
    [SerializeField] private float transitionDuration = 5f;

    private Vector3 oceanOffset;
    private float currentWaveHeight;
    private float targetWaveHeight;

    public enum OceanState
    {
        Calm,
        Stormy
    }

    private OceanState currentState = OceanState.Calm;

    public delegate void OceanStateChangedHandler(OceanState newState);
    public event OceanStateChangedHandler OnOceanStateChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (oceanAdvanced == null || waterTiler == null || playerTransform == null)
        {
            Debug.LogError("OceanController: Required references are missing!");
            enabled = false;
            return;
        }

        oceanOffset = oceanAdvanced.transform.position - playerTransform.position;
        currentWaveHeight = calmWaveHeight;
        targetWaveHeight = calmWaveHeight;
    }

    private void Update()
    {
        UpdateOceanPosition();
        UpdateWaveHeight();
    }

    private void UpdateOceanPosition()
    {
        Vector3 newPosition = new Vector3(
            playerTransform.position.x + oceanOffset.x,
            oceanAdvanced.transform.position.y,
            playerTransform.position.z + oceanOffset.z
        );
        oceanAdvanced.transform.position = newPosition;
    }

    private void UpdateWaveHeight()
    {
        if (currentWaveHeight != targetWaveHeight)
        {
            currentWaveHeight = Mathf.MoveTowards(currentWaveHeight, targetWaveHeight, Time.deltaTime / transitionDuration);
            SetWaveParameters(currentWaveHeight);
        }
    }

    public void SetOceanState(OceanState state)
    {
        if (state == currentState) return;

        currentState = state;
        targetWaveHeight = (state == OceanState.Calm) ? calmWaveHeight : stormyWaveHeight;
        OnOceanStateChanged?.Invoke(currentState);
    }

    private void SetWaveParameters(float waveHeight)
    {
        // Update OceanAdvanced wave parameters
        oceanAdvanced.SetWaveHeight(waveHeight);

        // Update all active water tiles
        waterTiler.UpdateAllTiles();
    }

    public float GetWaterHeight(Vector3 worldPosition)
    {
        Vector3 localPosition = worldPosition - oceanAdvanced.transform.position;
        return OceanAdvanced.GetWaterHeight(localPosition) + oceanAdvanced.transform.position.y;
    }

    public void RegisterInteraction(Vector3 worldPosition, float strength)
    {
        Vector3 localPosition = worldPosition - oceanAdvanced.transform.position;
        oceanAdvanced.RegisterInteraction(localPosition, strength);
    }

    public Material GetOceanMaterial()
    {
        return oceanAdvanced.ocean;
    }

    public void ApplyOceanToTile(GameObject tile)
    {
        Renderer tileRenderer = tile.GetComponent<Renderer>();
        if (tileRenderer != null && oceanAdvanced.ocean != null)
        {
            tileRenderer.material = oceanAdvanced.ocean;
            // Apply any additional ocean properties to the tile here
        }
    }

    public OceanState GetCurrentOceanState()
    {
        return currentState;
    }

    public float GetCurrentWaveHeight()
    {
        return currentWaveHeight;
    }

    public void UpdateOceanIntensity(float intensity)
    {
        // Example implementation: Adjust wave height based on intensity
        float waveHeight = Mathf.Lerp(calmWaveHeight, stormyWaveHeight, intensity);
        targetWaveHeight = waveHeight;
        SetWaveParameters(waveHeight);
    }

}