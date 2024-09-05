using UnityEngine;
using System.Collections;

public class OceanAdvanced : MonoBehaviour
{
  class Wave
  {
    public float waveLength { get; private set; }
    public float speed { get; private set; }
    public float amplitude { get; private set; }
    public float sharpness { get; private set; }
    public float frequency { get; private set; }
    public float phase { get; private set; }
    public Vector2 direction { get; private set; }

    public Wave(float waveLength, float speed, float amplitude, float sharpness, Vector2 direction)
    {
      this.waveLength = waveLength;
      this.speed = speed;
      this.amplitude = amplitude;
      this.sharpness = sharpness;
      this.direction = direction.normalized;
      frequency = 2 * Mathf.PI / waveLength;
      phase = frequency * speed;
    }

    public static Wave Lerp(Wave a, Wave b, float t)
    {
      return new Wave(
          Mathf.Lerp(a.waveLength, b.waveLength, t),
          Mathf.Lerp(a.speed, b.speed, t),
          Mathf.Lerp(a.amplitude, b.amplitude, t),
          Mathf.Lerp(a.sharpness, b.sharpness, t),
          Vector2.Lerp(a.direction, b.direction, t)
      );
    }
  };

  public Material ocean;
  public Light sun;

  private int interaction_id = 0;
  private Vector4[] interactions = new Vector4[NB_INTERACTIONS];

  const int NB_WAVE = 5;
  const int NB_INTERACTIONS = 64;

  static Wave[] calmWaves =
  {
        new Wave(99, 0.5f, 0.2f, 0.5f, new Vector2(1.0f, 0.2f)),
        new Wave(60, 0.6f, 0.1f, 0.3f, new Vector2(1.0f, 3.0f)),
        new Wave(20, 1.0f, 0.05f, 0.4f, new Vector2(2.0f, 4.0f)),
        new Wave(30, 0.75f, 0.05f, 0.2f, new Vector2(-1.0f, 0.0f)),
        new Wave(10, 1.0f, 0.01f, 0.5f, new Vector2(-1.0f, 1.2f))
    };

  static Wave[] choppyWaves = {
        new Wave(99, 0.8f, 0.3f, 0.5f, new Vector2(1.0f, 0.2f)),
        new Wave(60, 1.2f, 0.2f, 0.3f, new Vector2(1.0f, 3.0f)),
        new Wave(20, 1.5f, 0.1f, 0.4f, new Vector2(2.0f, 4.0f)),
        new Wave(30, 1.25f, 0.1f, 0.2f, new Vector2(-1.0f, 0.0f)),
        new Wave(10, 1.5f, 0.02f, 0.5f, new Vector2(-1.0f, 1.2f))
    };

  static Wave[] stormyWaves =
  {
        new Wave(99, 1.2f, 0.9f, 0.4f, new Vector2(1.0f, 0.2f)),
        new Wave(60, 1.8f, 0.5f, 0.3f, new Vector2(1.0f, 3.0f)),
        new Wave(20, 3.0f, 0.4f, 0.4f, new Vector2(2.0f, 4.0f)),
        new Wave(30, 2.5f, 0.4f, 0.3f, new Vector2(-1.0f, 0.0f)),
        new Wave(10, 3.5f, 0.08f, 0.6f, new Vector2(-1.0f, 1.2f))
    };

  static Wave[] activeWaves = new Wave[NB_WAVE];

  public enum WaterState
  {
    Calm,
    Choppy,
    Stormy
  }

  private WaterState currentState = WaterState.Calm;
  private WaterState targetState = WaterState.Calm;
  private float transitionProgress = 1f;

  [SerializeField]
  private float transitionDuration = 10f; // Duration of transition in seconds

  void Awake()
  {
    InitializeActiveWaves();
    SetWaterState(WaterState.Calm, true);
  }

  void InitializeActiveWaves()
  {
    for (int i = 0; i < NB_WAVE; i++)
    {
      activeWaves[i] = new Wave(
          calmWaves[i].waveLength,
          calmWaves[i].speed,
          calmWaves[i].amplitude,
          calmWaves[i].sharpness,
          calmWaves[i].direction
      );
    }
  }

  void FixedUpdate()
  {
    ocean.SetVector("world_light_dir", -sun.transform.forward);
    ocean.SetVector("sun_color", new Vector4(sun.color.r, sun.color.g, sun.color.b, 0.0F));

    UpdateWaves();
  }

  public void SetWaterState(WaterState state, bool immediate = false)
  {
    if (state == currentState && transitionProgress == 1f) return;

    targetState = state;
    if (immediate)
    {
      currentState = targetState;
      transitionProgress = 1f;
      UpdateWaveArrays();
    }
    else if (transitionProgress == 1f)
    {
      StartCoroutine(TransitionWaterState());
    }
  }

  private IEnumerator TransitionWaterState()
  {
    transitionProgress = 0f;
    WaterState startState = currentState;

    while (transitionProgress < 1f)
    {
      transitionProgress += Time.deltaTime / transitionDuration;
      UpdateWaveArrays();
      yield return null;
    }

    currentState = targetState;
    transitionProgress = 1f;
  }

  private void UpdateWaveArrays()
  {
    Wave[] startWaves = GetWavesForState(currentState);
    Wave[] endWaves = GetWavesForState(targetState);

    for (int i = 0; i < NB_WAVE; i++)
    {
      activeWaves[i] = Wave.Lerp(startWaves[i], endWaves[i], transitionProgress);
    }
  }

  private Wave[] GetWavesForState(WaterState state)
  {
    switch (state)
    {
      case WaterState.Calm:
        return calmWaves;
      case WaterState.Choppy:
        return choppyWaves;
      case WaterState.Stormy:
        return stormyWaves;
      default:
        return calmWaves;
    }
  }

  private void UpdateWaves()
  {
    Vector4[] v_waves = new Vector4[NB_WAVE];
    Vector4[] v_waves_dir = new Vector4[NB_WAVE];
    for (int i = 0; i < NB_WAVE; i++)
    {
      v_waves[i] = new Vector4(activeWaves[i].frequency, activeWaves[i].amplitude, activeWaves[i].phase, activeWaves[i].sharpness);
      v_waves_dir[i] = new Vector4(activeWaves[i].direction.x, activeWaves[i].direction.y, 0, 0);
    }

    ocean.SetVectorArray("waves_p", v_waves);
    ocean.SetVectorArray("waves_d", v_waves_dir);
  }

  public void RegisterInteraction(Vector3 pos, float strength)
  {
    interactions[interaction_id].x = pos.x;
    interactions[interaction_id].y = pos.z;
    interactions[interaction_id].z = strength;
    interactions[interaction_id].w = Time.time;
    ocean.SetVectorArray("interactions", interactions);
    interaction_id = (interaction_id + 1) % NB_INTERACTIONS;
  }

  static public float GetWaterHeight(Vector3 p)
  {
    if (activeWaves == null || activeWaves.Length == 0)
    {
      Debug.LogWarning("ActiveWaves not initialized in OceanAdvanced. Returning 0 for water height.");
      return 0;
    }

    float height = 0;
    for (int i = 0; i < NB_WAVE; i++)
    {
      if (activeWaves[i] != null)
      {
        height += activeWaves[i].amplitude * Mathf.Sin(Vector2.Dot(activeWaves[i].direction, new Vector2(p.x, p.z)) * activeWaves[i].frequency + Time.time * activeWaves[i].phase);
      }
    }
    return height;
  }
}