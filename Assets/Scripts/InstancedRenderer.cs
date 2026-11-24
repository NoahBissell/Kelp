using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

[ExecuteAlways]
public class InstancedRenderer : MonoBehaviour
{
    public Mesh[] meshes;
    public Material material;

    
    public bool alignToNormal = true;
    public float surfaceOffset = 0.02f;
    

    [System.Serializable]
    public struct Instance
    {
        public int meshIndex;
        public Vector3 position;
        public Vector3 placePosition;
        public Vector3 originPosition;
        public Vector3 placeNormal;
        public Quaternion rotation;
        public Vector3 scale;
    }

    public List<Instance> instances = new();

    const int BatchSize = 1023;

    protected virtual void Update()
    {
        Draw();
    }

    void OnRenderObject()
    {
        if (!Application.isPlaying)
            Draw();
    }

    public void Add(Vector3 pos, Vector3 normal, Quaternion rotation, Vector3 scale, Vector3 origin)
    {
        int meshIndex = Random.Range(0, meshes.Length);
        
        instances.Add(new Instance
        {
            meshIndex = meshIndex,
            position = InstancePosition(pos, normal),
            placePosition = pos,
            placeNormal = normal,
            rotation = rotation,
            scale = scale,
            originPosition = origin
        });
    }

    protected Vector3 InstancePosition(Vector3 pos, Vector3 normal)
    {
        return pos + normal * surfaceOffset;
    }

    

    public void Draw()
    {
        if (material == null || meshes == null)
            return;

        material.enableInstancing = true;

        Dictionary<int, List<Matrix4x4>> grouped = new();

        foreach (var inst in instances)
        {
            if (inst.meshIndex < 0 || inst.meshIndex >= meshes.Length)
                continue;

            if (!grouped.ContainsKey(inst.meshIndex))
                grouped[inst.meshIndex] = new List<Matrix4x4>();

            grouped[inst.meshIndex].Add(Matrix4x4.TRS(
                inst.position,
                inst.rotation,
                inst.scale
            ));
        }

        foreach (var pair in grouped)
        {
            Mesh mesh = meshes[pair.Key];
            List<Matrix4x4> matrices = pair.Value;

            for (int i = 0; i < matrices.Count; i += BatchSize)
            {
                int count = Mathf.Min(BatchSize, matrices.Count - i);
                Matrix4x4[] batch = new Matrix4x4[count];

                for (int j = 0; j < count; j++)
                    batch[j] = matrices[i + j];

                Graphics.DrawMeshInstanced(
                    mesh,
                    0,
                    material,
                    batch,
                    count,
                    null,
                    ShadowCastingMode.On,
                    true,
                    gameObject.layer
                );
            }
        }
    }

    
}