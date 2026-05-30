using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(Spawner))]
public class SpawnerEditor : Editor
{
   public override VisualElement CreateInspectorGUI()
   {
      // Create a new VisualElement to be the root of our Inspector UI.
      VisualElement myInspector = new VisualElement();
      InspectorElement.FillDefaultInspector(myInspector.contentContainer, serializedObject, this);
      // Add a simple label.
      myInspector.Add(new Label("This is a custom Inspector"));
      myInspector.Add(new Button((() => {(target as Spawner)?.Spawn(); })));

      // Return the finished Inspector UI.
      return myInspector;
   }
}
