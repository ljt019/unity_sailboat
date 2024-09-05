using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ArchimedsLab;

[RequireComponent(typeof(MeshFilter))]
public class FloatingGameEntityRealist : GameEntity
{
  [Header("Simplified Mesh Reference")]
  [SerializeField] public Mesh simplifiedBuoyancyMesh;

  [SerializeField] private Vector3 meshOffset = new Vector3(0f, 0f, 0.5f);

  [Header("Water Sampling Settings")]
  [SerializeField] private int waterSampleCount = 5;
  [SerializeField] private float waterSampleRadius = 0.5f;

  [Header("Stabilization Settings")]
  [SerializeField] private float stabilizationTorque = 1f;
  [SerializeField] private float angularVelocitySmoothing = 0.1f;

  [Header("Additional Damping Settings")]
  [SerializeField] private float additionalAngularDamping = 0.5f;
  [SerializeField] private float additionalLinearDamping = 0.1f;

  private tri[] _triangles;
  private tri[] worldBuffer;
  private tri[] wetTris;
  private tri[] dryTris;
  private uint nbrWet, nbrDry;

  private Vector3 smoothedAngularVelocity;

  private WaterSurface.GetWaterHeight realist;

  protected override void Awake()
  {
    base.Awake();

    if (simplifiedBuoyancyMesh == null)
    {
      Debug.LogError("Simplified buoyancy mesh is not assigned!");
      enabled = false;
      return;
    }

    InitializeWaterSampling();
    InitializeBuoyancyMesh();
    AdjustCenterOfMass();
  }

  private void InitializeWaterSampling()
  {
    realist = delegate (Vector3 pos)
    {
      float totalHeight = 0f;
      int validSamples = 0;
      for (int i = 0; i < waterSampleCount; i++)
      {
        float angle = (float)i / waterSampleCount * 2f * Mathf.PI;
        Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * waterSampleRadius;
        float height = OceanAdvanced.GetWaterHeight(pos + offset);
        if (!float.IsNaN(height) && !float.IsInfinity(height))
        {
          totalHeight += height;
          validSamples++;
        }
      }
      return validSamples > 0 ? totalHeight / validSamples : 0f;
    };
  }

  private void InitializeBuoyancyMesh()
  {
    Mesh offsetMesh = ApplyMeshOffset(simplifiedBuoyancyMesh, meshOffset);
    WaterCutter.CookCache(offsetMesh, ref _triangles, ref worldBuffer, ref wetTris, ref dryTris);
  }

  private void AdjustCenterOfMass()
  {
    rb.centerOfMass = new Vector3(0, -0.5f, 0.2f);
  }

  private Mesh ApplyMeshOffset(Mesh originalMesh, Vector3 offset)
  {
    Mesh newMesh = new Mesh();
    newMesh.name = originalMesh.name + "_Offset";

    Vector3[] vertices = originalMesh.vertices;
    for (int i = 0; i < vertices.Length; i++)
    {
      vertices[i] += offset;
    }

    newMesh.vertices = vertices;
    newMesh.triangles = originalMesh.triangles;
    newMesh.normals = originalMesh.normals;
    newMesh.uv = originalMesh.uv;

    return newMesh;
  }

  protected override void FixedUpdate()
  {
    base.FixedUpdate();
    if (rb.IsSleeping())
      return;

    try
    {
      UpdateBuoyancy();
      ApplyAdditionalForces();
    }
    catch (System.Exception e)
    {
      Debug.LogError($"Exception in FloatingGameEntityRealist.FixedUpdate: {e.GetType().Name} - {e.Message}\n{e.StackTrace}");
    }
  }

  private void UpdateBuoyancy()
  {
    WaterCutter.CookMesh(transform.position, transform.rotation, ref _triangles, ref worldBuffer);
    WaterCutter.SplitMesh(worldBuffer, ref wetTris, ref dryTris, out nbrWet, out nbrDry, realist);

    // Use ComputeAllForces instead of ComputeForces
    Archimeds.ComputeAllForces(wetTris, dryTris, nbrWet, nbrDry, speed, rb);
  }

  private void ApplyAdditionalForces()
  {
    rb.angularVelocity *= (1f - additionalAngularDamping * Time.fixedDeltaTime);
    rb.velocity *= (1f - additionalLinearDamping * Time.fixedDeltaTime);

    Vector3 stabilizingTorque = Vector3.Cross(transform.up, Vector3.up) * this.stabilizationTorque;
    rb.AddTorque(stabilizingTorque);

    smoothedAngularVelocity = Vector3.Lerp(smoothedAngularVelocity, rb.angularVelocity, angularVelocitySmoothing);
    rb.angularVelocity = smoothedAngularVelocity;
  }

#if UNITY_EDITOR
  protected override void OnDrawGizmos()
  {
    base.OnDrawGizmos();

    if (!Application.isPlaying)
      return;

    DrawBuoyancyMesh();
    DrawCenterOfMass();
    DrawUpVector();
    DrawStabilizingTorque();
  }

  private void DrawBuoyancyMesh()
  {
    Gizmos.color = Color.blue;
    for (uint i = 0; i < nbrWet; i++)
    {
      Gizmos.DrawLine(wetTris[i].a, wetTris[i].b);
      Gizmos.DrawLine(wetTris[i].b, wetTris[i].c);
      Gizmos.DrawLine(wetTris[i].a, wetTris[i].c);
    }

    Gizmos.color = Color.yellow;
    for (uint i = 0; i < nbrDry; i++)
    {
      Gizmos.DrawLine(dryTris[i].a, dryTris[i].b);
      Gizmos.DrawLine(dryTris[i].b, dryTris[i].c);
      Gizmos.DrawLine(dryTris[i].a, dryTris[i].c);
    }
  }

  private void DrawCenterOfMass()
  {
    Gizmos.color = Color.red;
    Gizmos.DrawSphere(transform.TransformPoint(rb.centerOfMass), 0.2f);
  }

  private void DrawUpVector()
  {
    Gizmos.color = Color.green;
    Gizmos.DrawLine(transform.position, transform.position + transform.up * 2f);
  }

  private void DrawStabilizingTorque()
  {
    Gizmos.color = Color.cyan;
    Vector3 torqueDirection = Vector3.Cross(transform.up, Vector3.up).normalized;
    Gizmos.DrawLine(transform.position, transform.position + torqueDirection * stabilizationTorque);
  }
#endif
}