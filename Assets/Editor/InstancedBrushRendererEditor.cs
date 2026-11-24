#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(InstancedBrushRenderer))]
public class InstancedBrushRendererEditor : Editor
{
    InstancedBrushRenderer painter;

    bool paintingEnabled = true;
    bool eraseMode = false;

    void OnEnable()
    {
        painter = (InstancedBrushRenderer)target;
        SceneView.duringSceneGui += DuringSceneGUI;
    }

    void OnDisable()
    {
        SceneView.duringSceneGui -= DuringSceneGUI;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();

        paintingEnabled = EditorGUILayout.Toggle("Scene Paint Enabled", paintingEnabled);
        eraseMode = EditorGUILayout.Toggle("Erase Mode", eraseMode);

        EditorGUILayout.HelpBox(
            "Scene View controls:\nLeft click + drag: paint\nShift + left click: erase",
            MessageType.Info
        );

        if (GUILayout.Button("Clear Instances"))
        {
            Undo.RecordObject(painter, "Clear Instanced Foliage");
            painter.instances.Clear();
            EditorUtility.SetDirty(painter);
        }

        if (GUILayout.Button("Update Instance Transforms"))
        {
            Undo.RecordObject(painter, "Update Instanced Foliage");
            painter.UpdateInstances();
            EditorUtility.SetDirty(painter);
        }
    }

    void DuringSceneGUI(SceneView sceneView)
    {
        if (!paintingEnabled || painter == null)
            return;

        Event e = Event.current;

        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, 10000f, painter.paintMask))
            return;

        Handles.color = eraseMode || e.shift ? Color.red : Color.green;
        Handles.DrawWireDisc(hit.point, hit.normal, painter.brushRadius);

        if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) &&
            e.button == 0 &&
            !e.alt)
        {
            if (eraseMode || e.shift)
                painter.Erase(hit.point);
            else
                painter.Paint(hit.point, hit.normal);

            e.Use();
            SceneView.RepaintAll();
        }
    }
}
#endif