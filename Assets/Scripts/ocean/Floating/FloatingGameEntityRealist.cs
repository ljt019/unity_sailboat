﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ArchimedsLab;

[RequireComponent(typeof(MeshFilter))]
public class FloatingGameEntityRealist : GameEntity
{
  public Mesh buoyancyMesh;
  public Mesh simplifiedBuoyancyMesh;

  [SerializeField] private Vector3 meshOffset = new Vector3(0f, 0f, 0.5f);

  /* These 4 arrays are cache array, preventing some operations to be done each frame. */
  tri[] _triangles;
  tri[] worldBuffer;
  tri[] wetTris;
  tri[] dryTris;
  //These two variables will store the number of valid triangles in each cache arrays. They are different from array.Length !
  uint nbrWet, nbrDry;

  [SerializeField] private float additionalAngularDamping = 0.5f;
  [SerializeField] private float additionalLinearDamping = 0.1f;
  [SerializeField] private int waterSampleCount = 5;
  [SerializeField] private float waterSampleRadius = 0.5f;
  [SerializeField] private float stabilizationTorque = 1f;
  [SerializeField] private float angularVelocitySmoothing = 0.1f;

  private Vector3 smoothedAngularVelocity;

  WaterSurface.GetWaterHeight realist;

  protected override void Awake()
  {
    base.Awake();

    // Initialize the water height sampling function
    realist = delegate (Vector3 pos)
    {
      float totalHeight = 0f;
      int validSamples = 0;
      for (int i = 0; i < waterSampleCount; i++)
      {
        float angle = (float)i / waterSampleCount * 2f * Mathf.PI;
        Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * waterSampleRadius;
        try
        {
          float height = OceanAdvanced.GetWaterHeight(pos + offset);
          totalHeight += height;
          validSamples++;
        }
        catch (System.NullReferenceException)
        {
          Debug.LogWarning("NullReferenceException caught in water height sampling. Skipping this sample.");
        }
      }
      return validSamples > 0 ? totalHeight / validSamples : 0f;
    };

    //By default, this script will take the render mesh to compute forces. You can override it, using a simpler mesh.
    Mesh m = simplifiedBuoyancyMesh != null ? simplifiedBuoyancyMesh :
             (buoyancyMesh != null ? buoyancyMesh : GetComponent<MeshFilter>().mesh);

    Mesh offsetMesh = ApplyMeshOffset(m, meshOffset);
    //Setting up the cache for the game. Here we use variables with a game-long lifetime.
    WaterCutter.CookCache(offsetMesh, ref _triangles, ref worldBuffer, ref wetTris, ref dryTris);

    // Adjust center of mass
    rb.centerOfMass = new Vector3(0, -0.5f, 0.2f); // Adjust as needed
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
      // This will prepare static cache, modifying vertices using rotation and position offset.
      WaterCutter.CookMesh(transform.position, transform.rotation, ref _triangles, ref worldBuffer);

      // Split the mesh into wet and dry triangles
      WaterCutter.SplitMesh(worldBuffer, ref wetTris, ref dryTris, out nbrWet, out nbrDry, realist);

      // Compute and apply forces
      Archimeds.ComputeAllForces(wetTris, dryTris, nbrWet, nbrDry, speed, rb);

      // Apply additional damping
      rb.angularVelocity *= (1f - additionalAngularDamping * Time.fixedDeltaTime);
      rb.velocity *= (1f - additionalLinearDamping * Time.fixedDeltaTime);

      // Apply stabilizing torque
      Vector3 stabilizingTorque = Vector3.Cross(transform.up, Vector3.up) * this.stabilizationTorque;
      rb.AddTorque(stabilizingTorque);

      // Smooth angular velocity
      smoothedAngularVelocity = Vector3.Lerp(smoothedAngularVelocity, rb.angularVelocity, angularVelocitySmoothing);
      rb.angularVelocity = smoothedAngularVelocity;
    }
    catch (System.Exception e)
    {
      Debug.LogError($"Exception in FloatingGameEntityRealist.FixedUpdate: {e.GetType().Name} - {e.Message}\n{e.StackTrace}");
      // Handle the error gracefully, perhaps by skipping this frame's update
    }
  }

#if UNITY_EDITOR
  //Some visualizations for this buoyancy script.
  protected override void OnDrawGizmos()
  {
    base.OnDrawGizmos();

    if (!Application.isPlaying)
      return;

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

    // Visualize center of mass
    Gizmos.color = Color.red;
    Gizmos.DrawSphere(transform.TransformPoint(rb.centerOfMass), 0.2f);

    // Visualize up vector
    Gizmos.color = Color.green;
    Gizmos.DrawLine(transform.position, transform.position + transform.up * 2f);

    // Visualize stabilizing torque
    Gizmos.color = Color.cyan;
    Vector3 torqueDirection = Vector3.Cross(transform.up, Vector3.up).normalized;
    Gizmos.DrawLine(transform.position, transform.position + torqueDirection * stabilizationTorque);
  }
#endif
}