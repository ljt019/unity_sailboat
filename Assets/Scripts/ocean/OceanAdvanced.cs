using UnityEngine;
using System.Collections;

public class OceanAdvanced : MonoBehaviour
{
  [System.Serializable]
  public class Wave
  {
    public float waveLength;
    public float speed;
    public float amplitude;
    public float sharpness;
    public Vector2 direction;

    [System.NonSerialized] public float frequency;
    [System.NonSerialized] public float phase;

    public void UpdateFrequencyAndPhase()
    {
      frequency = 2 * Mathf.PI / waveLength;
      phase = frequency * speed;
    }

    public static Wave Lerp(Wave a, Wave b, float t)
    {
      return new Wave
      {
        waveLength = Mathf.Lerp(a.waveLength, b.waveLength, t),
        speed = Mathf.Lerp(a.speed, b.speed, t),
        amplitude = Mathf.Lerp(a.amplitude, b.amplitude, t),
        sharpness = Mathf.Lerp(a.sharpness, b.sharpness, t),
        direction = Vector2.Lerp(a.direction, b.direction, t).normalized
      };
    }
  }

  public Material ocean;
  public Light sun;

  private int interaction_id = 0;
  private Vector4[] interactions;

  const int NB_WAVE = 5;
  const int NB_INTERACTIONS = 64;

  [SerializeField] private Wave[] calmWaves = new Wave[NB_WAVE];
  [SerializeField] private Wave[] stormyWaves = new Wave[NB_WAVE];
  private Wave[] activeWaves = new Wave[NB_WAVE];

  private Vector4[] v_waves;
  private Vector4[] v_waves_dir;

  private float currentWaveHeightMultiplier = 1f;
  private float targetWaveHeightMultiplier = 1f;
  private float waveTransitionSpeed = 0.1f;

  private Coroutine updateWavesCoroutine;

  private static OceanAdvanced Instance;

  public Shader oceanShader;

  void Awake()
  {
    Instance = this;
    interactions = new Vector4[NB_INTERACTIONS];
    v_waves = new Vector4[NB_WAVE];
    v_waves_dir = new Vector4[NB_WAVE];
    InitializeWaves();
    EnsureShaderAssigned();
  }

  void InitializeWaves()
  {
    for (int i = 0; i < NB_WAVE; i++)
    {
      calmWaves[i].UpdateFrequencyAndPhase();
      stormyWaves[i].UpdateFrequencyAndPhase();
      activeWaves[i] = new Wave();
    }
    UpdateWaveArrays(1f);
  }

  void FixedUpdate()
  {
    UpdateOceanMaterial();
  }

  void EnsureShaderAssigned()
  {
    if (ocean != null && oceanShader != null)
    {
      if (ocean.shader != oceanShader)
      {
        Debug.Log("Reassigning ocean shader");
        ocean.shader = oceanShader;
      }
    }
    else
    {
      Debug.LogError("Ocean material or shader is missing!");
    }
  }

  private void UpdateOceanMaterial()
  {
    ocean.SetVector("world_light_dir", -sun.transform.forward);
    ocean.SetVector("sun_color", new Vector4(sun.color.r, sun.color.g, sun.color.b, 0.0F));

    currentWaveHeightMultiplier = Mathf.MoveTowards(currentWaveHeightMultiplier, targetWaveHeightMultiplier, waveTransitionSpeed * Time.deltaTime);

    for (int i = 0; i < NB_WAVE; i++)
    {
      v_waves[i].x = activeWaves[i].frequency;
      v_waves[i].y = activeWaves[i].amplitude * currentWaveHeightMultiplier;
      v_waves[i].z = activeWaves[i].phase;
      v_waves[i].w = activeWaves[i].sharpness;

      v_waves_dir[i].x = activeWaves[i].direction.x;
      v_waves_dir[i].y = activeWaves[i].direction.y;
    }

    ocean.SetVectorArray("waves_p", v_waves);
    ocean.SetVectorArray("waves_d", v_waves_dir);
    ocean.SetVectorArray("interactions", interactions);
  }

  public void SetWaveHeight(float height)
  {
    targetWaveHeightMultiplier = height;
    if (updateWavesCoroutine != null)
    {
      StopCoroutine(updateWavesCoroutine);
    }
    updateWavesCoroutine = StartCoroutine(UpdateWavesOverTime());
  }

  private IEnumerator UpdateWavesOverTime()
  {
    float t = 0f;
    while (t < 1f)
    {
      t += Time.deltaTime;
      UpdateWaveArrays(t);
      yield return null;
    }
    UpdateWaveArrays(1f);
  }

  private void UpdateWaveArrays(float t)
  {
    for (int i = 0; i < NB_WAVE; i++)
    {
      activeWaves[i] = Wave.Lerp(calmWaves[i], stormyWaves[i], t);
      activeWaves[i].UpdateFrequencyAndPhase();
    }
  }

  public void RegisterInteraction(Vector3 pos, float strength)
  {
    interactions[interaction_id].x = pos.x;
    interactions[interaction_id].y = pos.z;
    interactions[interaction_id].z = strength;
    interactions[interaction_id].w = Time.time;
    interaction_id = (interaction_id + 1) % NB_INTERACTIONS;
  }

  public static float GetWaterHeight(Vector3 p)
  {
    if (Instance == null || Instance.activeWaves == null)
    {
      Debug.LogWarning("OceanAdvanced instance or activeWaves is null. Returning 0 for water height.");
      return 0f;
    }

    float height = 0f;
    for (int i = 0; i < NB_WAVE; i++)
    {
      Wave wave = Instance.activeWaves[i];
      height += wave.amplitude * Instance.currentWaveHeightMultiplier *
                Mathf.Sin(Vector2.Dot(wave.direction, new Vector2(p.x, p.z)) * wave.frequency + Time.time * wave.phase);
    }
    return height;
  }

  private void OnDisable()
  {
    if (Instance == this)
    {
      Instance = null;
    }
  }
}