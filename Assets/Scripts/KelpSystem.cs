using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

[ExecuteAlways]
public class InstancedBrushRenderer : MonoBehaviour
{
    public Mesh[] meshes;
    public Material material;

    public float brushRadius = 2f;
    public int densityPerStroke = 20;
    public float spacing = 0.35f;
    public Vector2 scaleRange = new Vector2(0.8f, 1.3f);
    public LayerMask paintMask = ~0;
    public bool alignToNormal = true;
    public float surfaceOffset = 0.02f;
    public Vector3 rotateOffset;

    [System.Serializable]
    public struct Instance
    {
        public int meshIndex;
        public Vector3 position;
        public Vector3 placePosition;
        public Vector3 placeNormal;
        public Quaternion rotation;
        public Vector3 scale;
    }

    public List<Instance> instances = new();

    const int BatchSize = 1023;

    void Update()
    {
        Draw();
    }

    void OnRenderObject()
    {
        if (!Application.isPlaying)
            Draw();
    }

    private void OnValidate()
    {
        
    }

    public void UpdateInstances()
    {
        UndoRecord();

        for (int i = 0; i < instances.Count; i++)
        {
            Instance instance = instances[i];
            instance.position = InstancePosition(instance.placePosition, instance.placeNormal);
            instance.scale = InstanceScale();
            instance.rotation = InstanceRotation(instance.placeNormal);
            instances[i] = instance;
        }
    }

    public void Paint(Vector3 center, Vector3 normal)
    {
        if (meshes == null || meshes.Length == 0)
            return;

        UndoRecord();

        for (int i = 0; i < densityPerStroke; i++)
        {
            Vector2 r = Random.insideUnitCircle * brushRadius;

            Vector3 tangent = Vector3.Cross(normal, Vector3.forward);
            if (tangent.sqrMagnitude < 0.001f)
                tangent = Vector3.Cross(normal, Vector3.right);

            tangent.Normalize();
            Vector3 bitangent = Vector3.Cross(normal, tangent).normalized;

            Vector3 sample = center + tangent * r.x + bitangent * r.y;
            Vector3 rayStart = sample + normal * 5f;

            if (!Physics.Raycast(rayStart, -normal, out RaycastHit hit, 10f, paintMask))
                continue;

            if (HasNearby(hit.point))
                continue;

            Add(hit.point, hit.normal);
        }
    }

    public void Erase(Vector3 center)
    {
        UndoRecord();

        float r2 = brushRadius * brushRadius;

        for (int i = instances.Count - 1; i >= 0; i--)
        {
            if ((instances[i].position - center).sqrMagnitude <= r2)
                instances.RemoveAt(i);
        }
    }

    bool HasNearby(Vector3 pos)
    {
        float s2 = spacing * spacing;

        foreach (var inst in instances)
        {
            if ((inst.position - pos).sqrMagnitude < s2)
                return true;
        }

        return false;
    }

    void Add(Vector3 pos, Vector3 normal)
    {
        int meshIndex = Random.Range(0, meshes.Length);
        
        instances.Add(new Instance
        {
            meshIndex = meshIndex,
            position = InstancePosition(pos, normal),
            placePosition = pos,
            placeNormal = normal,
            rotation = InstanceRotation(normal),
            scale = InstanceScale()
        });
    }

    Vector3 InstancePosition(Vector3 pos, Vector3 normal)
    {
        return pos + normal * surfaceOffset;
    }

    Quaternion InstanceRotation(Vector3 normal)
    {
        Quaternion rot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        if (alignToNormal)
            rot = Quaternion.FromToRotation(Vector3.up, normal) * rot;
        
        return rot * Quaternion.Euler(rotateOffset);
    }

    Vector3 InstanceScale()
    {
        return Random.Range(scaleRange.x, scaleRange.y) * Vector3.one;
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

    void UndoRecord()
    {
#if UNITY_EDITOR
        UnityEditor.Undo.RecordObject(this, "Paint Instanced Foliage");
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}