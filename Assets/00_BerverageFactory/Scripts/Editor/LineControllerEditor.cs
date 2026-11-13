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

        }
    }
}
