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
        Debug.Log($"OceanController initialized. Initial wave height: {currentWaveHeight}");
    }

    private void Start()
    {
        // Ensure WaterTiler is initialized after OceanController
        if (waterTiler != null)
        {
            waterTiler.UpdateAllTiles();
        }
        else
        {
            Debug.LogError("WaterTiler reference is missing in OceanController!");
        }
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
            Debug.Log($"Wave height updated. Current: {currentWaveHeight}, Target: {targetWaveHeight}");
        }
    }

    public void SetOceanState(OceanState state)
    {
        if (state == currentState) return;

        currentState = state;
        targetWaveHeight = (state == OceanState.Calm) ? calmWaveHeight : stormyWaveHeight;
        OnOceanStateChanged?.Invoke(currentState);
        Debug.Log($"Ocean state changed to {state}. Target wave height: {targetWaveHeight}");
    }

    private void SetWaveParameters(float waveHeight)
    {
        oceanAdvanced.SetWaveHeight(waveHeight);
        waterTiler.UpdateAllTiles();
        Debug.Log($"Wave parameters set. Height: {waveHeight}");
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
            Debug.Log($"Ocean material applied to tile: {tile.name}");
        }
        else
        {
            Debug.LogWarning($"Failed to apply ocean material to tile: {tile.name}");
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
        float waveHeight = Mathf.Lerp(calmWaveHeight, stormyWaveHeight, intensity);
        targetWaveHeight = waveHeight;
        SetWaveParameters(waveHeight);
        Debug.Log($"Ocean intensity updated. Intensity: {intensity}, New target wave height: {targetWaveHeight}");
    }
}