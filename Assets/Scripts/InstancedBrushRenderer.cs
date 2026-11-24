using UnityEngine;

public class InstancedBrushRenderer : InstancedRenderer
{
    public float brushRadius = 2f;
    public int densityPerStroke = 20;
    public float spacing = 0.35f;
    public Vector2 scaleRange = new Vector2(0.8f, 1.3f);
    public LayerMask paintMask = ~0;
    
    public Vector3 rotateOffset;
    public Vector2 rotationRange = new Vector2(0, 360f);
    
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

            Add(hit.point, hit.normal, InstanceRotation(hit.normal), InstanceScale(), Vector3.zero);
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
    
    protected Quaternion InstanceRotation(Vector3 normal)
    {
        Quaternion rot = Quaternion.Euler(0f, Random.Range(rotationRange.x, rotationRange.y), 0f);

        if (alignToNormal)
            rot = Quaternion.FromToRotation(Vector3.up, normal) * rot;
        
        return rot * Quaternion.Euler(rotateOffset);
    }

    protected Vector3 InstanceScale()
    {
        return Random.Range(scaleRange.x, scaleRange.y) * Vector3.one;
    }
    
    void UndoRecord()
    {
#if UNITY_EDITOR
        UnityEditor.Undo.RecordObject(this, "Paint Instanced Foliage");
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}