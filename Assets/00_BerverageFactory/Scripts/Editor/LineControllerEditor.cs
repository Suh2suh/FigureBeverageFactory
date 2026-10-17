using UnityEditor;
using UnityEngine;

namespace FBFactory.Editor
{
    [CustomEditor(typeof(LineController))]
    public class LineControllerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            LineController lineController = (LineController)target;

            if (GUILayout.Button("CollectAllUnits"))
                lineController.CollectAllUnits();

            if (GUILayout.Button("StartAllUnits"))
                lineController.StartAllUnits();

            if (GUILayout.Button("StopAllUnits"))
                lineController.StopAllUnits();
        }
    }
}
